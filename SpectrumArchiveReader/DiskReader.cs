using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml;

namespace SpectrumArchiveReader
{
    public class DiskReader
    {
        public bool Aborted;
        public DiskReaderParams Params;
        private Timer trackTimer = new Timer();
        private Timer scanFormatStopwatch = new Timer();
        private Timer scanFormatTotalTime = new Timer();
        private TrackFormat workTrackFormat = new TrackFormat(55);
        private TrackFormat longestTrackFormat = new TrackFormat(55);
        private TrackFormat scanFormatBuffer = new TrackFormat(55);
        public IntPtr DriverHandle;
        private IntPtr memoryHandle;

        public DiskReader()
        {
            
        }

        public DiskReader(DiskReaderParams pars)
        {
            Params = pars;
        }

        public bool OpenDriver()
        {
            DriverHandle = Driver.Open(Params.DataRate, Params.Drive);
            if ((int)DriverHandle == Driver.INVALID_HANDLE_VALUE) return false;
            memoryHandle = Driver.VirtualAlloc(8192);
            if (memoryHandle == IntPtr.Zero) return false;
            return true;
        }

        public void CloseDriver()
        {
            if ((int)DriverHandle != Driver.INVALID_HANDLE_VALUE) Driver.Close(DriverHandle);
            if (memoryHandle != IntPtr.Zero) Driver.VirtualFree(memoryHandle, 0, Driver.AllocationType.Release);
            DriverHandle = (IntPtr)Driver.INVALID_HANDLE_VALUE;
            memoryHandle = IntPtr.Zero;
        }

        public unsafe int ReadRandomSectors(TimeSpan timeout, int stopOnNthFail = 0)
        {
            int goodSectors = Params.Image.GoodSectors;
            int imageTracks = Params.Image.SizeTracks;
            int* trackHead = stackalloc int[imageTracks];
            for (int i = 0; i < imageTracks; i++)
            {
                trackHead[i] = -1;
            }
            try
            {
                bool useTimeout = timeout > TimeSpan.Zero;
                DateTime timeoutTime = DateTime.Now.Add(timeout);
                Timer sectorTimer = new Timer();
                Random random = new Random();
                UpperSideHead upperSideHead = Params.UpperSideHead;
                bool upperSideHeadScanned = false;
                MList<int> sectorArray = new MList<int>(Params.Image.SizeSectors);
                for (int i = Params.FirstSectorNum; i < Params.LastSectorNum; i++)
                {
                    int track = i / Params.Image.SectorsOnTrack;
                    if (Params.Side == DiskSide.Side0 && track % 2 != 0) continue;
                    if (Params.Side == DiskSide.Side1 && track % 2 != 1) continue;
                    if (Params.Image.Sectors[i] != SectorProcessResult.Good) sectorArray.Add(i);
                }
                int prevCylinder = -1;
                int failCounter = 0;
                while (sectorArray.Cnt > 0 && ((useTimeout && DateTime.Now < timeoutTime) || !useTimeout) && (stopOnNthFail == 0 || (failCounter < stopOnNthFail)))
                {
                    int index = random.Next(sectorArray.Cnt);
                    int sectorNum = sectorArray.Data[index];
                    int track = sectorNum / Params.SectorsOnTrack;
                    SectorInfo sector = Params.Image.StandardFormat.Layout.Data[sectorNum - track * Params.SectorsOnTrack];
                    int error = 23;
                    Params.Image.Map?.ClearHighlight(MapCell.Processing);
                    Params.Image.Map?.MarkAsProcessing(sectorNum);
                    if (Params.UpperSideHeadAutodetect && trackHead[track] != -1) upperSideHead = (UpperSideHead)trackHead[track];
                    if (Params.UpperSideHeadAutodetect && !upperSideHeadScanned && (track & 1) != 0 && trackHead[track] == -1)
                    {
                        UpperSideHead ush = upperSideHead;
                        if (ScanHeadParameter(ref ush, track, Params.CurrentTrackFormat) == 0)
                        {
                            upperSideHead = ush;
                            trackHead[track] = (int)upperSideHead;
                            Log.Trace?.Out($"Параметр Head для трека {track} определен: {(int)upperSideHead}");
                        }
                        else
                        {
                            Log.Trace?.Out($"Параметр Head для трека {track} определить не удалось.");
                        }
                        upperSideHeadScanned = true;
                    }
                    bool noHeader = false;
                    bool badWritten = Params.Image.Sectors[sectorNum] == SectorProcessResult.Bad;
                    for (int attempt = 0; attempt < Params.SectorReadAttempts; attempt++)
                    {
                        if (Aborted) goto abort;
                        int cylinder = track / 2;
                        if (cylinder == prevCylinder)
                        {
                            int tempCylinder = cylinder + (random.Next(2) == 0 ? -1 : 1);
                            tempCylinder = Math.Max(0, tempCylinder);
                            tempCylinder = Math.Min(tempCylinder, Params.LastTrack / 2);
                            Driver.Seek(DriverHandle, tempCylinder * 2);
                            if (Aborted) goto abort;
                            Thread.Sleep(random.Next((int)TrackFormat.SpinTimeStandard)); // Ждем случайное время чтобы приехать на нужный цилиндр в случайной точке.
                        }
                        prevCylinder = cylinder;
                        Driver.Seek(DriverHandle, track);
                        if (Aborted) goto abort;
                        sectorTimer.Start();
                        error = Driver.ReadSector(DriverHandle, memoryHandle, track, sector.SectorNumber, upperSideHead, sector.SizeCode);
                        sectorTimer.Stop();
                        if (error == 0) goto sectorReadSuccessfully;
                        noHeader = error == 21 || (error == 27 && sectorTimer.ElapsedMs > Params.CurrentTrackFormat.SpinTime);
                        Make30HeadPositionings(error, track);
                        if (error == 23)
                        {
                            Params.Image.WriteBadSectors(sectorNum, memoryHandle, 1, false);
                            badWritten = true;
                        }
                        if (noHeader && track % 2 == 1 && Params.UpperSideHeadAutodetect && trackHead[track] == -1)
                        {
                            UpperSideHead ush = upperSideHead;
                            if (ScanHeadParameter(ref ush, track, Params.CurrentTrackFormat) == 0)
                            {
                                upperSideHead = ush;
                                trackHead[track] = (int)upperSideHead;
                                Log.Trace?.Out($"Параметр Head для трека {track} определен: {(int)upperSideHead}");
                                attempt--;
                            }
                            else
                            {
                                Log.Trace?.Out($"Параметр Head для трека {track} определить не удалось.");
                            }
                        }
                    }
                    failCounter++;
                    if (!badWritten) Params.Image.WriteBadSectors(sectorNum, 1, noHeader);
                    continue;
                    sectorReadSuccessfully:
                    Params.Image.WriteGoodSectors(sectorNum, memoryHandle, 1);
                    trackHead[track] = (int)upperSideHead;
                    sectorArray.RemoveAt(index);
                    failCounter = 0;
                    timeoutTime = DateTime.Now.Add(timeout);
                }
                return Params.Image.GoodSectors - goodSectors;
                abort:
                Log.Info?.Out("Чтение прервано.");
            }
            finally
            {
                Params.Image.Map?.ClearHighlight(MapCell.Processing);
            }
            return Params.Image.GoodSectors - goodSectors;
        }

        /// <summary>
        /// Чтение вперед. Возвращает количество успешно прочитанных секторов.
        /// </summary>
        /// <returns>Количество успешно прочитанных секторов.</returns>
        public int ReadForward()
        {
            int goodSectors = Params.Image.GoodSectors;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GCLatencyMode oldGCLatencyMode = GCSettings.LatencyMode;
            try
            {
                GCSettings.LatencyMode = GCLatencyMode.LowLatency;
                int firstTrack = Params.FirstSectorNum / Params.SectorsOnTrack;
                int prevCylinder = -1;
                int lastTrack = (int)Math.Ceiling((double)Params.LastSectorNum / Params.SectorsOnTrack);
                trackTimer.Restart();
                bool upperHeadScanned = false;
                for (int track = firstTrack; track < lastTrack; track++)
                {
                    if (Aborted) goto abort;
                    if (Params.Side == DiskSide.Side0 && (track % 2 != 0)) continue;
                    if (Params.Side == DiskSide.Side1 && (track % 2 != 1)) continue;
                    int diskSectorNum = track * Params.SectorsOnTrack;
                    for (int i = diskSectorNum; i < diskSectorNum + Params.SectorsOnTrack; i++)
                    {
                        if (Params.Image.Sectors[i] != SectorProcessResult.Good) goto nread;
                    }
                    continue;
                    nread:
                    int cylinder = track / 2;
                    if (cylinder != prevCylinder)
                    {
                        Driver.Seek(DriverHandle, track);
                        if (Aborted) goto abort;
                        prevCylinder = cylinder;
                    }
                    if (Params.UpperSideHeadAutodetect && !upperHeadScanned && (track & 1) != 0)
                    {
                        if (ScanHeadParameter(ref Params.UpperSideHead, track, Params.CurrentTrackFormat) == 0)
                        {
                            Log.Trace?.Out($"Параметр Head трека {track} определен: {(int)Params.UpperSideHead}");
                            upperHeadScanned = true;
                        }
                        else
                        {
                            Log.Trace?.Out($"Параметр Head трека {track} определить не удалось.");
                        }
                        if (Aborted) goto abort;
                    }
                    ReadTrack(track, Params);
                }
                return Params.Image.GoodSectors - goodSectors;
                abort:
                Log.Info?.Out("Чтение прервано.");
            }
            finally
            {
                GCSettings.LatencyMode = oldGCLatencyMode;
                Params.Image.Map?.ClearHighlight(MapCell.Processing);
            }
            return Params.Image.GoodSectors - goodSectors;
        }

        /// <summary>
        /// Чтение назад. Возвращает количество успешно прочитанных секторов.
        /// </summary>
        /// <returns>Возвращает количество успешно прочитанных секторов.ы</returns>
        public int ReadBackward()
        {
            int goodSectors = Params.Image.GoodSectors;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GCLatencyMode oldGCLatencyMode = GCSettings.LatencyMode;
            try
            {
                GCSettings.LatencyMode = GCLatencyMode.LowLatency;
                int firstTrack = Params.FirstSectorNum / Params.SectorsOnTrack;
                int prevCylinder = -1;
                int lastTrack = (int)Math.Ceiling((double)Params.LastSectorNum / Params.SectorsOnTrack);
                trackTimer.Restart();
                bool upperHeadScanned = false;
                for (int track = lastTrack - 1; track >= firstTrack; track--)
                {
                    if (Aborted) goto abort;
                    if (Params.Side == DiskSide.Side0 && (track % 2 != 0)) continue;
                    if (Params.Side == DiskSide.Side1 && (track % 2 != 1)) continue;
                    int diskSectorNum = track * Params.SectorsOnTrack;
                    for (int i = diskSectorNum; i < diskSectorNum + Params.SectorsOnTrack; i++)
                    {
                        if (Params.Image.Sectors[i] != SectorProcessResult.Good) goto nread;
                    }
                    continue;
                    nread:
                    int cylinder = track / 2;
                    if (cylinder != prevCylinder)
                    {
                        if (Aborted) goto abort;
                        Driver.Seek(DriverHandle, track);
                        prevCylinder = cylinder;
                    }
                    if (Params.UpperSideHeadAutodetect && !upperHeadScanned && (track & 1) != 0)
                    {
                        if (ScanHeadParameter(ref Params.UpperSideHead, track, Params.CurrentTrackFormat) == 0)
                        {
                            Log.Trace?.Out($"Параметр Head трека {track} определен: {(int)Params.UpperSideHead}");
                            upperHeadScanned = true;
                        }
                        else
                        {
                            Log.Trace?.Out($"Параметр Head трека {track} определить не удалось.");
                        }
                        if (Aborted) goto abort;
                    }
                    ReadTrack(track, Params);
                }
                return Params.Image.GoodSectors - goodSectors;
                abort:
                Log.Info?.Out("Чтение прервано.");
            }
            finally
            {
                GCSettings.LatencyMode = oldGCLatencyMode;
                Params.Image.Map?.ClearHighlight(MapCell.Processing);
            }
            return Params.Image.GoodSectors - goodSectors;
        }

        public unsafe void ReadTrack(int track, DiskReaderParams pars)
        {
            // Массив sectors:
            // D0 - Признак обработанного сектора при чтении трека в одной попытке. Обнуляется перед каждой попыткой чтения трека.
            // D1 - Используется в блоке чтения заголовков. Отмечаются сектора заголовки которых были найдены. Вне этого блока не используется.
            // D2 - Запрет чтения сектора из-за того что его заголовок был прочитан SectorReadAttempts раз и не был найден. Параметр сохраняется между попытками чтения трека.

            int* sectors = stackalloc int[pars.SectorsOnTrack];
            for (int i = 0; i < pars.SectorsOnTrack; i++)
            {
                sectors[i] = 0;
            }
            bool formatScanned = false;
            bool headScanned = false;
            bool trackScanned = false;
            bool wasCorrectHead = false;
            bool missedHeaders = false;
            for (int attempt = 0; attempt < pars.SectorReadAttempts; attempt++)
            {
                if (Aborted) return;
                for (int i = 0; i < pars.SectorsOnTrack; i++)
                {
                    sectors[i] &= ~1;
                }
                bool wasError = false;
                double timeAfterSync = 0;
                trackTimer.Restart();
                int skip = 0;
                int processedSectors = 0;
                int sectorCounter = 0;
                SectorInfo diskSector;
                while (processedSectors < pars.SectorsOnTrack)
                {
                    bool sync = false;
                    if (pars.ReadMode == ReadMode.Fast && pars.CurrentTrackFormat.IsSync)
                    {
                        bool gcsError;
                        diskSector = pars.CurrentTrackFormat.GetClosestSector(track, 0, skip, out timeAfterSync, out gcsError);
                        sync = true;
                        // Переключение в стандартный режим чтения в случае если функция GetClosestSector отработала с ошибкой.
                        // Такое было у одного из пользователей и причина ошибки непонятна.
                        if (gcsError) pars.ReadMode = ReadMode.Standard;
                    }
                    else
                    {
                        diskSector = pars.Image.StandardFormat.Layout.Data[sectorCounter];
                        sectorCounter++;
                    }
                    int sectorIndex = pars.Image.StandardFormat.FindSectorIndex(diskSector.SectorNumber);
                    int diskSectorNum = track * Params.SectorsOnTrack + sectorIndex;
                    skip++;
                    if ((sectors[sectorIndex] & 1) != 0) continue;
                    sectors[sectorIndex] |= 1;
                    processedSectors++;
                    if ((sectors[sectorIndex] & 4) != 0) continue;
                    if (diskSectorNum < pars.FirstSectorNum || diskSectorNum >= pars.LastSectorNum) continue;
                    if (pars.Image.Sectors[diskSectorNum] == SectorProcessResult.Good) continue;
                    bool badWritten = pars.Image.Sectors[diskSectorNum] == SectorProcessResult.Bad;
                    skip = 0;
                    pars.Image.Map?.ClearHighlight(MapCell.Processing);
                    pars.Image.Map?.MarkAsProcessing(diskSectorNum);
                    WinApi.RtlZeroMemory(memoryHandle, (UIntPtr)diskSector.SizeBytes);
                    int error = Driver.ReadSector(DriverHandle, memoryHandle, track, diskSector.SectorNumber, pars.UpperSideHead, diskSector.SizeCode);
                    double curTimeSinceSync = pars.CurrentTrackFormat.Timer.ElapsedMs;
                    if (error == 0)
                    {
                        pars.CurrentTrackFormat.Sync(track, diskSector.SectorNumber);
                        pars.Image.WriteGoodSectors(diskSectorNum, memoryHandle, 1);
                        wasCorrectHead = true;
                        if (Aborted) return;

                        // Проверка несовпадения расположения секторов с заданным. Работает только для TR-DOS.

                        if (pars.ReadMode == ReadMode.Fast
                            && pars.TrackLayoutAutodetect
                            && sync
                            && !formatScanned
                            && !missedHeaders
                            && curTimeSinceSync < 2 * pars.CurrentTrackFormat.SpinTime)
                        {
                            double remainderTime = curTimeSinceSync;
                            for (int t = 0; remainderTime > 0 && t < 20; remainderTime -= pars.CurrentTrackFormat.SpinTime, t++)
                            {
                                // 6 - половина времени сектора TR-DOS (около 12 мс).
                                if (Math.Abs(remainderTime - timeAfterSync) <= 6) goto skip;
                            }
                            Log.Trace?.Out($"Несовпадение расположения секторов. Calculated Time: {GP.ToString(timeAfterSync, 2)} | Real Time: {GP.ToString(curTimeSinceSync, 2)}");
                            formatScanned = true;
                            bool scanResult = ScanFormat(workTrackFormat, track, true);
                            if (Aborted) return;
                            if (scanResult)
                            {
                                Log.Trace?.Out($"Формат трека {track}: {workTrackFormat.FormatName} | {workTrackFormat.Layout.Cnt} sectors | {workTrackFormat.ToStringAsSectorArray()}");
                                if (workTrackFormat.FormatName <= TrackFormatName.TrDosGeneric) pars.CurrentTrackFormat.Assign(workTrackFormat);
                            }
                            //pars.Image.Map?.ClearHighlight(MapCell.Scanning);
                            //findex = pars.TrackFormat.GetNextSectorIndex(track);
                            skip:;
                        }
                    }
                    else
                    {
                        wasError = true;
                        bool writeBad = true;
                        bool noHeader = error == 21 || error == 1112 || error == 1122 || (error == 27 /* && curTimeSinceSync > pars.CurrentTrackFormat.SpinTime*/);
                        if (noHeader)
                        {
                            Make30HeadPositionings(error, track);
                            
                            // Проверка параметра Head верхней стороны.

                            bool scanWholeTrack = true;
                            int scanHeadResult = 0;
                            if (track % 2 == 1 && !headScanned && !wasCorrectHead && pars.UpperSideHeadAutodetect)
                            {
                                headScanned = true;
                                UpperSideHead head = pars.UpperSideHead;
                                scanHeadResult = ScanHeadParameter(ref head, track, pars.CurrentTrackFormat);
                                if (scanHeadResult == 0)
                                {
                                    if (head != pars.UpperSideHead)
                                    {
                                        Log.Trace?.Out($"Параметр Head верхней стороны для трека {track} определен: {(int)head}");
                                        pars.UpperSideHead = head;
                                        writeBad = false;
                                        sectors[sectorIndex] = 0;
                                        processedSectors--;
                                        scanWholeTrack = false;
                                    }
                                    else
                                    {
                                        Log.Trace?.Out($"Параметр Head верхней стороны для трека {track} определен и совпадает с предыдущим: {(int)head}");
                                    }
                                }
                                else
                                {
                                    Log.Trace?.Out($"Параметр Head верхней стороны для трека {track} определить не удалось.");
                                }
                            }

                            // Чтение неформатных областей.
                            // Если встретился NoHeader, то предполагается что на этом треке может быть еще много случаев NoHeader, 
                            // поэтому они проверяются сканированием трека целиком. Это происходит быстрее, чем если пытаться читать их по-отдельности.

                            if (scanWholeTrack && !trackScanned)
                            {
                                int sec0Num = Math.Max(track * pars.SectorsOnTrack, pars.FirstSectorNum);
                                int sec0NtNum = Math.Min((track + 1) * pars.SectorsOnTrack, pars.LastSectorNum);
                                // sectorsToCheck - число секторов которые надо проверить на предмет отсутствия заголовков.
                                // Если подозреваемый сектор только один (т.е. только что прочитанный) и число попыток чтения 1, то sectorsToCheck после цикла будет нулём.
                                bool ignoreCurSector = pars.SectorReadAttempts == 1;
                                writeBad = ignoreCurSector;
                                int sectorsToCheck = ignoreCurSector ? 0 : 1;    // Учет только что обработанного сектора.
                                // Далее к sectorsToCheck добавляются только необработанные сектора.
                                for (int sn = sec0Num, si = sn - track * pars.SectorsOnTrack; sn < sec0NtNum; sn++, si++)
                                {
                                    if (pars.Image.Sectors[sn] != SectorProcessResult.Good && (sectors[si] & 1) == 0) sectorsToCheck++;
                                }
                                if (sectorsToCheck > 0)
                                {
                                    trackScanned = true;
                                    int foundHeaders = 0;
                                    bool scanResult = false;
                                    int maxExtraSectors = 0;
                                    if (!ignoreCurSector) sectors[sectorIndex] &= ~1;
                                    int attempts = !ignoreCurSector && sectorsToCheck == 1 ? pars.SectorReadAttempts - 1 : pars.SectorReadAttempts;
                                    for (int attemptnh = 0; attemptnh < attempts; attemptnh++)
                                    {
                                        scanResult = false;
                                        if (scanHeadResult == 1 && attemptnh == 0)
                                        {
                                            workTrackFormat.Layout.Cnt = 0;
                                        }
                                        else
                                        {
                                            scanResult = ScanFormat(workTrackFormat, track, true);
                                        }
                                        if (workTrackFormat.Layout.Cnt == 0)
                                        {
                                            Log.Info?.Out($"На треке {track} заголовки не найдены.");
                                        }
                                        else
                                        {
                                            Log.Trace?.Out($"Формат трека {track}: {workTrackFormat.FormatName} | {workTrackFormat.Layout.Cnt} sectors | {workTrackFormat.ToStringAsSectorArray()}");
                                        }
                                        for (int sn = sec0Num, si = sn - track * pars.SectorsOnTrack; sn < sec0NtNum; sn++, si++)
                                        {
                                            if (pars.Image.Sectors[sn] == SectorProcessResult.Good || (sectors[si] & 1) == 1 || (sectors[si] & 2) != 0) continue;
                                            int dSector = pars.Image.StandardFormat.Layout.Data[si].SectorNumber;
                                            int sindex = workTrackFormat.FindSectorIndex(dSector);
                                            int head = pars.UpperSideHead == UpperSideHead.Head1 ? track & 1 : 0;
                                            int sizeCode = pars.CurrentTrackFormat.Layout.Data[pars.CurrentTrackFormat.FindSectorIndex(dSector)].SizeCode;
                                            if (sindex >= 0 && workTrackFormat.Layout.Data[sindex].SizeCode == sizeCode)
                                            {
                                                if (workTrackFormat.Layout.Data[sindex].Cylinder == track / 2
                                                    && workTrackFormat.Layout.Data[sindex].Head == head)
                                                {
                                                    sectors[si] |= 2;
                                                    foundHeaders++;
                                                }
                                                else
                                                {
                                                    Log.Info?.Out($"Не совпадает Cylinder или Head в заголовке сектора {dSector} на треке {track} (цилиндр {track / 2} сторона {track % 2}): {workTrackFormat.Layout.Data[sindex]}");
                                                }
                                            }
                                        }

                                        // Поиск экстра-секторов на треке.

                                        int extraSectors = 0;
                                        for (int ii = 0; ii < workTrackFormat.Layout.Cnt; ii++)
                                        {
                                            for (int jj = 0; jj < pars.CurrentTrackFormat.Layout.Cnt; jj++)
                                            {
                                                if (pars.CurrentTrackFormat.Layout.Data[jj].SectorNumber == workTrackFormat.Layout.Data[ii].SectorNumber
                                                    && pars.CurrentTrackFormat.Layout.Data[jj].SizeCode == workTrackFormat.Layout.Data[ii].SizeCode)
                                                {
                                                    goto found;
                                                }
                                            }
                                            extraSectors++;
                                            found:;
                                        }
                                        if (extraSectors > maxExtraSectors)
                                        {
                                            maxExtraSectors = extraSectors;
                                            longestTrackFormat.Assign(workTrackFormat);
                                        }

                                        if (foundHeaders == sectorsToCheck) break;
                                    }
                                    int noHeaderCnt = 0;
                                    for (int sn = sec0Num, si = sn - track * pars.SectorsOnTrack; sn < sec0NtNum; sn++, si++)
                                    {
                                        if (pars.Image.Sectors[sn] == SectorProcessResult.Good || (sectors[si] & 1) == 1) continue;
                                        if ((sectors[si] & 2) == 0)
                                        {
                                            // 4 (100b) означает что заголовок не найден за нужное число попыток 
                                            // и больше читать этот сектор не надо, т.к. все попытки исчерпаны.
                                            sectors[si] |= 4;
                                            if (!badWritten) pars.Image.WriteBadSectors(sn, 1, true);
                                            noHeaderCnt++;
                                        }
                                    }
                                    if (scanResult) pars.CurrentTrackFormat.Sync(workTrackFormat);
                                    Log.Trace?.Out($"На треке {track} не найдены заголовки {noHeaderCnt} секторов.");
                                    if (noHeaderCnt > 0) missedHeaders = true;
                                    if (maxExtraSectors > 0)
                                    {
                                        Log.Info?.Out($"Трек {track} имеет несоответствующий формат: {longestTrackFormat.GetFormatName()} | {longestTrackFormat.Layout.Cnt} sectors | {longestTrackFormat.ToStringAsSectorArray()}");
                                    }
                                }
                            }
                        }
                        else if (error == 23)
                        {
                            writeBad = false;
                            pars.Image.WriteBadSectors(diskSectorNum, memoryHandle, 1, false);
                        }
                        if (writeBad && !badWritten) pars.Image.WriteBadSectors(diskSectorNum, 1, noHeader);
                    }
                    if (Aborted) return;
                }
                Log.Trace?.Out($"Время чтения трека: {GP.ToString(trackTimer.ElapsedMs, 2)}");
                trackTimer.Restart();
                if (!wasError) break;
            }
        }

        /// <summary>
        /// Сканирование трека. Сканировать может в двух режимах: по времени оборота диска и по зацикливанию потока секторов.
        /// При сканировании по времени оборота диска скорость диска должна быть в районе 300 об/мин, т.е. замедлять диск не надо, иначе будут ошибки.
        /// Если byLooping == true, то конец трека определяется по зацикливанию потока секторов, но сектора сканируются не менее 150 мс и не более 1000 мс.
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public unsafe bool ScanFormat(TrackFormat result, int track, bool byLooping)
        {
            int ilayoutCnt = 0;
            scanFormatBuffer.Layout.Cnt = 0;
            SectorInfo[] layout = scanFormatBuffer.Layout.Data;
            tagFD_READ_ID_PARAMS pars = new tagFD_READ_ID_PARAMS()
            {
                flags = Driver.FD_OPTION_MFM,
                head = (byte)(track & 1)
            };
            tagFD_CMD_RESULT cmdResult = new tagFD_CMD_RESULT();
            scanFormatStopwatch.Restart();
            scanFormatTotalTime.Stop();
            int firstIndex = 1;
            for (int i = 0; i < scanFormatBuffer.Layout.Capacity; i++)
            {
                if (Aborted) return false;
                if (!Driver.ReadId(DriverHandle, pars, out cmdResult))
                {
                    int error = Marshal.GetLastWin32Error();
                    Log.Trace?.Out($"Функция ReadId вернула false. i={i}. LastError={error}");
                    if (Aborted) return false;
                    Make30HeadPositionings(error, track);
                    if (scanFormatBuffer.Layout.Cnt == 0) goto success;
                    return false;
                }
                double timeMs = scanFormatStopwatch.ElapsedMs;
                scanFormatStopwatch.Restart();
                if (!scanFormatTotalTime.IsRunning) scanFormatTotalTime.Restart();
                if (!byLooping && scanFormatTotalTime.ElapsedMs > 209) goto success;
                layout[ilayoutCnt] = new SectorInfo()
                {
                    Cylinder = cmdResult.cyl,
                    Head = cmdResult.head,
                    SectorNumber = cmdResult.sector,
                    SizeCode = cmdResult.size,
                    TimeMs = timeMs
                };
                ilayoutCnt++;
                scanFormatBuffer.Layout.Cnt = ilayoutCnt;

                // Бывают случаи когда заголовок сектора читается на первом обороте и не читается на втором.
                // Из-за этого алгоритм, в котором повтор сектора определяется сравнением с первым прочитанным заголовком, иногда даёт сбои.
                // Поэтому ищем встечался ли ранее только что прочитанный сектор среди всех прочитанных заголовков, а не только сравниваем с первым.
                // По отношению к найденному сектору замеряем время вращения. Если оно больше 250 мс, значит этот сектор был пропущен на одном из оборотов 
                // и такую последовательность брать нельзя. Если время меньше 150, значит на треке есть сектора с одинаковыми параметрами.

                if (byLooping)
                {
                    for (int u = 0; u < ilayoutCnt - 1; u++)
                    {
                        if (layout[u].Cylinder == cmdResult.cyl
                            && layout[u].Head == cmdResult.head
                            && layout[u].SectorNumber == cmdResult.sector
                            && layout[u].SizeCode == cmdResult.size)
                        {
                            double spinTime = 0;
                            for (int p = u + 1; p < ilayoutCnt; p++)
                            {
                                spinTime += layout[p].TimeMs;
                            }
                            if (spinTime > 250 || spinTime < 150) continue;
                            firstIndex = u + 1;
                            goto success;
                        }
                    }
                    if (scanFormatTotalTime.ElapsedMs > 1000)
                    {
                        Log.Trace?.Out($"Не удалось найти цикл в последовательности секторов из-за нестабильного чтения. Сканироваие прервано по таймауту.");
                        return false;
                    }
                }
            }
            return false;
            success:
            result.AssignLayout(scanFormatBuffer, track, cmdResult.sector, firstIndex);
            Log.Trace?.Out($"Время сканирования трека {track}: {GP.ToString(scanFormatTotalTime.ElapsedMs, 2)}");
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result">0 - head определен. 1 - Заголовки не найдены. 2 - Заголовок найден, но он не TR-DOS</param>
        /// <param name="track"></param>
        /// <param name="trackFormat"></param>
        /// <returns></returns>
        private int ScanHeadParameter(ref UpperSideHead result, int track, TrackFormat trackFormat)
        {
            tagFD_READ_ID_PARAMS pars = new tagFD_READ_ID_PARAMS()
            {
                flags = Driver.FD_OPTION_MFM,
                head = (byte)(track & 1)
            };
            tagFD_CMD_RESULT cmdResult = new tagFD_CMD_RESULT();
            if (!Driver.ReadId(DriverHandle, pars, out cmdResult))
            {
                Log.Trace?.Out($"Функция ReadId вернула false. Cylinder: {cmdResult.cyl} | Sector: {cmdResult.sector} | Size: {cmdResult.size} | Head: {cmdResult.head} | LastError: {Marshal.GetLastWin32Error()}");
                return 1;
            }
            //if (cmdResult.sector < 1 || cmdResult.sector > 16 || cmdResult.size != 1)
            //{
            //    Log.Trace?.Out($"Формат не TR-DOS. Cylinder: {cmdResult.cyl} | Sector: {cmdResult.sector} | Size: {cmdResult.size} | Head: {cmdResult.head}");
            //    return 2;
            //}
            trackFormat.SyncByHeader(track, cmdResult.sector);
            Log.Trace?.Out($"Cylinder: {cmdResult.cyl} | Sector: {cmdResult.sector} | Size: {cmdResult.size} | Head: {cmdResult.head}");
            result = (UpperSideHead)cmdResult.head;
            return 0;
        }

        public void DetectDiskSize(int htMaxTracks)
        {
            DiskImage image = Params.Image;
            if (image.SizeTracks < htMaxTracks)
            {
                int realMaxTrack = image.SizeTracks;
                int prevCylinder = (image.SizeTracks - 1) / 2;
                for (int track = image.SizeTracks; track < htMaxTracks; track++)
                {
                    int cylinder = track / 2;
                    if (cylinder != prevCylinder)
                    {
                        Driver.Seek(DriverHandle, track);
                        if (Aborted) return;
                        prevCylinder = cylinder;
                    }
                    if (!ScanFormat(workTrackFormat, track, true)) return;
                    Log.Info?.Out($"Формат трека {track}: {workTrackFormat.FormatName} | {workTrackFormat.Layout.Cnt} sectors | {workTrackFormat.ToStringAsSectorArray()}");
                    if (workTrackFormat.ContainsSectorsFrom(image.StandardFormat, cylinder))
                    {
                        realMaxTrack = track + 1;
                        image.SetSize(realMaxTrack * Params.SectorsOnTrack);
                        Params.LastSectorNum = image.SizeSectors;
                        ReadTrack(track, Params);
                    }
                }
            }
        }

        /// <summary>
        /// Здесь делается 30 вызовов позиционирования.
        /// Причина в следующем.Если случается ошибка 21, то серия следующих чтений приводит к ошибке 21
        /// (не важно где эти чтения происходят, даже если треки находятся далеко от того где произошла первоначальная ошибка 21).
        /// Последующие ошибки 21 не вызывают новых ошибок 21 (иначе эта серия продолжалась бы бесконечно).
        /// Но если сделать вызов Seek около 16-20 раз, то этот эффект исчезает и можно читать следующий сектор, который вернет
        /// его ошибку, а не просто повторит 21.
        /// Следующая проблема заключается в том что после ошибки 21, и даже после последующих 20 команд Seek,
        /// команда ReadId всегда возвращает false даже если сектора существуют на диске.
        /// Поскольку она используется для определения параметра Head верхней стороны диска,
        /// и используется именно при ошибке 21, то получается что она становится неработоспособной именно тогда когда оказывается нужна.
        /// Но потом выяснилось что если вызвать 30 позиционирований (а не 20), то начинает работать команда ReadId и читать заголовки.
        /// Поэтому здесь делается 30 вызовов Seek. Работает и 50 и 300 вызовов Seek, но я решил сделать число поменьше на всякий случай.
        /// Неизвестно нужно ли это делать если ошибка была 27 (т.е.когда заголовок не найден и есть синхроимпульс), и будет ли произведена
        /// рекалибрация после такой ошибки.
        /// </summary>
        /// <param name="error">Код ошибки. 30 позиционирований будет сделано если он равен 21.</param>
        /// <param name="track"></param>
        public void Make30HeadPositionings(int error, int track)
        {
            if (error != 21) return;
            for (int u = 0; u < 30; u++)
            {
                if (Aborted) return;
                Driver.Seek(DriverHandle, track);
            }
        }
    }

    public class DiskReaderParams : ICloneable
    {
        public Drive Drive;
        public UpperSideHead UpperSideHead;
        public bool UpperSideHeadAutodetect;
        public ReadMode ReadMode;
        public bool TrackLayoutAutodetect;
        public TrackFormat CurrentTrackFormat = new TrackFormat(16);
        public DataRate DataRate;
        public DiskImage Image;
        public int FirstSectorNum;
        /// <summary>
        /// Параметр не должен выходить за границы образа (может быть равен числу секторов образа).
        /// </summary>
        public int LastSectorNum;
        public DiskSide Side;
        /// <summary>
        /// Параметр должен быть больше нуля.
        /// </summary>
        public int SectorReadAttempts;
        public int FirstTrack;
        public int LastTrack;

        public int SectorsOnTrack { get { return Image.StandardFormat.Layout.Cnt; } }

        public object Clone()
        {
            DiskReaderParams r = (DiskReaderParams)MemberwiseClone();
            r.CurrentTrackFormat = (TrackFormat)CurrentTrackFormat.Clone();
            return r;
        }

        public void SaveToXml(XmlTextWriter xml, string name)
        {
            xml.WriteStartElement(name);
            xml.WriteAttributeString("Drive", Drive.ToString());
            xml.WriteAttributeString("DataRate", DataRate.ToString());
            xml.WriteAttributeString("Side", Side.ToString());
            xml.WriteAttributeString("ReadMode", ReadMode.ToString());
            xml.WriteAttributeString("UpperSideHead", UpperSideHead.ToString());
            xml.WriteAttributeString("UpperSideHeadAutodetect", UpperSideHeadAutodetect.ToString());
            xml.WriteAttributeString("SectorReadAttempts", SectorReadAttempts.ToString());
            xml.WriteAttributeString("FirstTrack", FirstTrack.ToString());
            xml.WriteAttributeString("LastTrack", LastTrack.ToString());
            xml.WriteEndElement();
        }

        public void ReadFromXml(XmlTextReader xml)
        {
            Drive = (Drive)Enum.Parse(typeof(Drive), xml.GetAttribute("Drive"));
            DataRate = (DataRate)Enum.Parse(typeof(DataRate), xml.GetAttribute("DataRate"));
            Side = (DiskSide)Enum.Parse(typeof(DiskSide), xml.GetAttribute("Side"));
            ReadMode = (ReadMode)Enum.Parse(typeof(ReadMode), xml.GetAttribute("ReadMode"));
            UpperSideHead = (UpperSideHead)Enum.Parse(typeof(UpperSideHead), xml.GetAttribute("UpperSideHead"));
            UpperSideHeadAutodetect = bool.Parse(xml.GetAttribute("UpperSideHeadAutodetect"));
            SectorReadAttempts = Math.Max(0, int.Parse(xml.GetAttribute("SectorReadAttempts")));
            FirstTrack = Math.Max(0, int.Parse(xml.GetAttribute("FirstTrack")));
            FirstTrack = Math.Min(171, FirstTrack);
            LastTrack = Math.Min(172, int.Parse(xml.GetAttribute("LastTrack")));
            LastTrack = Math.Max(0, LastTrack);
        }
    }

    public enum ReadMode
    {
        Standard,
        Fast
    }
}
