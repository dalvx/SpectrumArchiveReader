using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

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
        private IntPtr driverHandle;
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
            driverHandle = Driver.Open(Params.DataRate);
            if ((int)driverHandle == Driver.INVALID_HANDLE_VALUE) return false;
            memoryHandle = Driver.VirtualAlloc(Params.SectorSize);
            if (memoryHandle == IntPtr.Zero) return false;
            return true;
        }

        public void CloseDriver()
        {
            if ((int)driverHandle != Driver.INVALID_HANDLE_VALUE) Driver.Close(driverHandle);
            if (memoryHandle != IntPtr.Zero) Driver.VirtualFree(memoryHandle, 0, Driver.AllocationType.Release);
            driverHandle = (IntPtr)Driver.INVALID_HANDLE_VALUE;
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
                for (int i = Params.SectorNumFrom; i < Params.SectorNumTo; i++)
                {
                    if (Params.Side == DiskSide.Side0 && i % 2 != 0) continue;
                    if (Params.Side == DiskSide.Side1 && i % 2 != 1) continue;
                    if (Params.Image.Sectors[i] != SectorProcessResult.Good) sectorArray.Add(i);
                }
                int prevCylinder = -1;
                int failCounter = 0;
                while (sectorArray.Cnt > 0 && ((useTimeout && DateTime.Now < timeoutTime) || !useTimeout) && (stopOnNthFail == 0 || (failCounter < stopOnNthFail)))
                {
                    int index = random.Next(sectorArray.Cnt);
                    int sectorNum = sectorArray.Data[index];
                    int track = sectorNum / Params.SectorsOnTrack;
                    int sector = Params.ImageSectorLayout.Layout.Data[sectorNum - track * Params.SectorsOnTrack].SectorNumber;
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
                    for (int attempt = 0; attempt < Params.SectorReadAttempts; attempt++)
                    {
                        if (Aborted) goto abort;
                        int cylinder = track / 2;
                        if (cylinder == prevCylinder)
                        {
                            int tempCylinder = cylinder + (random.Next(2) == 0 ? -1 : 1);
                            tempCylinder = Math.Max(0, tempCylinder);
                            tempCylinder = Math.Min(tempCylinder, Params.TrackTo / 2);
                            Driver.Seek(driverHandle, tempCylinder * 2);
                            if (Aborted) goto abort;
                            Thread.Sleep(random.Next(200)); // Ждем случайное время чтобы приехать на нужный цилиндр в случайной точке.
                        }
                        prevCylinder = cylinder;
                        Driver.Seek(driverHandle, track);
                        if (Aborted) goto abort;
                        sectorTimer.Start();
                        error = Driver.ReadSector(driverHandle, memoryHandle, track, sector, upperSideHead, SectorInfo.GetSizeCode(Params.SectorSize));
                        sectorTimer.Stop();
                        if (error == 0) goto sectorReadSuccessfully;
                        noHeader = error == 21 || (error == 27 && sectorTimer.ElapsedMs > Params.CurrentTrackFormat.SpinTime);
                        if (error == 21)
                        {
                            for (int u = 0; u < 30; u++)
                            {
                                if (Aborted) goto abort;
                                Driver.Seek(driverHandle, track);
                            }
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
                    Params.Image.WriteBadSectors(sectorNum, 1, noHeader);
                    if (error == 21)
                    {
                        for (int u = 0; u < 30; u++)
                        {
                            if (Aborted) goto abort;
                            Driver.Seek(driverHandle, track);
                        }
                    }
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

        public MList<TrackFormat> ScanDiskFormat(MList<TrackFormat> diskFormat)
        {
            try
            {
                int trackFrom = Params.TrackFrom;
                int prevCylinder = -1;
                int trackTo = Params.TrackTo;
                TrackFormat trackFormat = new TrackFormat(55);
                trackTimer.Restart();
                for (int track = trackFrom; track < trackTo; track++)
                {
                    if (diskFormat[track].FormatName <= TrackFormatName.TrDosGeneric) continue;
                    if (Aborted) goto abort;
                    int cylinder = track / 2;
                    if (cylinder != prevCylinder)
                    {
                        Driver.Seek(driverHandle, track);
                        if (Aborted) goto abort;
                        prevCylinder = cylinder;
                    }
                    for (int attempt = 0; attempt < Params.SectorReadAttempts; attempt++)
                    {
                        ScanFormat(trackFormat, track, true);
                        if (Aborted) goto abort;
                        if (diskFormat[track].Layout.Cnt < trackFormat.Layout.Cnt) diskFormat[track].Assign(trackFormat);
                        if (trackFormat.Layout.Cnt == 0)
                        {
                            Log.Info?.Out($"Заголовки не найдены. Трек: {track}");
                            for (int i = 0; i < 30; i++)
                            {
                                Driver.Seek(driverHandle, track);
                            }
                            continue;
                        }
                        else if (trackFormat.FormatName == TrackFormatName.Unrecognized)
                        {
                            Log.Info?.Out($"Формат не TR-DOS ({trackFormat.Layout.Cnt} секторов) Трек: {track}");
                            continue;
                        }
                        break;
                    }
                }
                return diskFormat;
                abort:
                Log.Info?.Out("Сканирование прервано.");
                return diskFormat;
            }
            finally
            {
                Params.Image.Map?.ClearHighlight(MapCell.Processing);
            }
        }

        public int ReadForward()
        {
            int goodSectors = Params.Image.GoodSectors;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GCLatencyMode oldGCLatencyMode = GCSettings.LatencyMode;
            try
            {
                GCSettings.LatencyMode = GCLatencyMode.LowLatency;
                int trackFrom = Params.SectorNumFrom / Params.SectorsOnTrack;
                int prevCylinder = -1;
                int trackTo = (int)Math.Ceiling((double)Params.SectorNumTo / Params.SectorsOnTrack);
                trackTimer.Restart();
                bool upperHeadScanned = false;
                for (int track = trackFrom; track < trackTo; track++)
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
                        Driver.Seek(driverHandle, track);
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

        public int ReadBackward()
        {
            int goodSectors = Params.Image.GoodSectors;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GCLatencyMode oldGCLatencyMode = GCSettings.LatencyMode;
            try
            {
                GCSettings.LatencyMode = GCLatencyMode.LowLatency;
                int trackFrom = Params.SectorNumFrom / Params.SectorsOnTrack;
                int prevCylinder = -1;
                int trackTo = (int)Math.Ceiling((double)Params.SectorNumTo / Params.SectorsOnTrack);
                trackTimer.Restart();
                bool upperHeadScanned = false;
                for (int track = trackTo - 1; track >= trackFrom; track--)
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
                        Driver.Seek(driverHandle, track);
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

        private unsafe void ReadTrack(int track, DiskReaderParams pars)
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
                SectorInfo diskSector = new SectorInfo() { SizeCode = SectorInfo.GetSizeCode(pars.SectorSize) };
                while (processedSectors < pars.SectorsOnTrack)
                {
                    bool sync = false;
                    if (pars.ReadMode == ReadMode.Fast && pars.CurrentTrackFormat.IsSync)
                    {
                        diskSector = pars.CurrentTrackFormat.GetNextSector(track, 0, skip, out timeAfterSync);
                        sync = true;
                    }
                    else
                    {
                        diskSector.SectorNumber = pars.ImageSectorLayout.Layout.Data[sectorCounter].SectorNumber;
                        sectorCounter++;
                    }
                    int sectorIndex = pars.ImageSectorLayout.FindSectorIndex(diskSector.SectorNumber);
                    int diskSectorNum = track * Params.SectorsOnTrack + sectorIndex;
                    skip++;
                    if ((sectors[sectorIndex] & 1) != 0) continue;
                    sectors[sectorIndex] |= 1;
                    processedSectors++;
                    if ((sectors[sectorIndex] & 4) != 0) continue;
                    if (diskSectorNum < pars.SectorNumFrom || diskSectorNum >= pars.SectorNumTo) continue;
                    if (pars.Image.Sectors[diskSectorNum] == SectorProcessResult.Good) continue;
                    skip = 0;
                    pars.Image.Map?.ClearHighlight(MapCell.Processing);
                    pars.Image.Map?.MarkAsProcessing(diskSectorNum);
                    int error = Driver.ReadSector(driverHandle, memoryHandle, track, diskSector.SectorNumber, pars.UpperSideHead, diskSector.SizeCode);
                    double curTimeSinceSync = pars.CurrentTrackFormat.Timer.ElapsedMs;
                    if (error == 0)
                    {
                        pars.CurrentTrackFormat.Sync(track, diskSector.SectorNumber);
                        pars.Image.WriteGoodSectors(diskSectorNum, memoryHandle, 1);
                        wasCorrectHead = true;
                        if (Aborted) return;

                        // Проверка несовпадения расположения секторов с заданным. Работает только для TR-DOS.

                        double remainderTime = curTimeSinceSync - (int)(curTimeSinceSync / pars.CurrentTrackFormat.SpinTime) * pars.CurrentTrackFormat.SpinTime;
                        if (pars.ReadMode == ReadMode.Fast
                            && pars.TrackLayoutAutodetect
                            && sync
                            && !formatScanned
                            && !missedHeaders
                            && curTimeSinceSync < 2 * pars.CurrentTrackFormat.SpinTime
                            && Math.Abs(remainderTime - timeAfterSync) > 6)     // 6 - половина времени сектора TR-DOS (около 12 мс).
                        {
                            Log.Trace?.Out($"Несовпадение расположения секторов. Calculated Time: {GP.ToString(timeAfterSync, 2)} | Real Time: {GP.ToString(curTimeSinceSync, 2)} | Remainder Time: {GP.ToString(remainderTime, 2)}");
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
                        }
                    }
                    else
                    {
                        wasError = true;
                        bool writeBad = true;
                        bool noHeader = error == 21 || error == 1112 || (error == 27 && curTimeSinceSync > pars.CurrentTrackFormat.SpinTime);
                        if (noHeader)
                        {
                            // Здесь делается 30 вызовов позиционирования.
                            // Причина в следующем. Если случается ошибка 21, то серия следующих чтений приводит к ошибке 21
                            // (не важно где эти чтения происходят, даже если треки находятся далеко от того где произошла первоначальная ошибка 21).
                            // Последующие ошибки 21 не вызывают новых ошибок 21 (иначе эта серия продолжалась бы бесконечно).
                            // Но если сделать вызов Seek около 16-20 раз, то этот эффект исчезает и можно читать следующий сектор, который вернет 
                            // его ошибку, а не просто повторит 21.
                            // Следующая проблема заключается в том что после ошибки 21, и даже после последующих 20 команд Seek, 
                            // команда ReadId всегда возвращает false даже если сектора существуют на диске.
                            // Поскольку она используется для определения параметра Head верхней стороны диска,
                            // и используется именно при ошибке 21, то получается что она становится неработоспособной именно тогда когда оказывается нужна.
                            // Но потом выяснилось что если вызвать 30 позиционирований (а не 20), то начинает работать команда ReadId и читать заголовки.
                            // Поэтому здесь делается 30 вызовов Seek. Работает и 50 и 300 вызовов Seek, но я решил сделать число поменьше на всякий случай.
                            // Неизвестно нужно ли это делать если ошибка была 27 (т.е. когда заголовок не найден и есть синхроимпульс), и будет ли произведена
                            // рекалибрация после такой ошибки.

                            for (int u = 0; u < 30; u++)
                            {
                                if (Aborted) return;
                                Driver.Seek(driverHandle, track);
                            }

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
                                int sec0Num = Math.Max(track * pars.SectorsOnTrack, pars.SectorNumFrom);
                                int sec0NtNum = Math.Min((track + 1) * pars.SectorsOnTrack, pars.SectorNumTo);
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
                                            int dSector = pars.ImageSectorLayout.Layout.Data[si].SectorNumber;
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
                                                    Log.Info?.Out($"Не совпадает Cylinder или Head в заголовке сектора {dSector} на треке {track}: {workTrackFormat.Layout.Data[sindex]}");
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
                                            pars.Image.WriteBadSectors(sn, 1, true);
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
                        if (writeBad) pars.Image.WriteBadSectors(diskSectorNum, 1, noHeader);
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
        /// Если byLooping == true, то конец трека определяется по зацикливанию потока секторов, но сектора сканируются не менее 150 мс и не более 300 мс.
        /// 21.05.2020: Сканирование по времени работает плохо если делается при шаге головки без задержек, не находит один сектор как правило.
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public unsafe bool ScanFormat(TrackFormat result, int track, bool byLooping)
        {
            SectorInfo* ilayout = stackalloc SectorInfo[55];
            int ilayoutCnt = 0;
            tagFD_READ_ID_PARAMS pars = new tagFD_READ_ID_PARAMS()
            {
                flags = Driver.FD_OPTION_MFM,
                head = (byte)(track & 1)
            };
            tagFD_CMD_RESULT cmdResult = new tagFD_CMD_RESULT();
            scanFormatStopwatch.Restart();
            scanFormatTotalTime.Restart();
            int sector1Index = 0;
            tagFD_CMD_RESULT firstHeaderEncountered = new tagFD_CMD_RESULT();
            for (int i = 0; i < 55; i++)
            {
                if (Aborted) return false;
                if (!Driver.ReadId(driverHandle, pars, out cmdResult))
                {
                    Log.Trace?.Out($"Функция ReadId вернула false. i={i}. LastError={Marshal.GetLastWin32Error()}");
                    break;
                }
                double timeMs = scanFormatStopwatch.ElapsedMs;
                scanFormatStopwatch.Restart();
                if (i == 0)
                {
                    firstHeaderEncountered = cmdResult;
                    continue;
                }
                if (!byLooping && scanFormatTotalTime.ElapsedMs > 209) break;
                if (cmdResult.sector == 1) sector1Index = ilayoutCnt;
                ilayout[ilayoutCnt++] = new SectorInfo()
                {
                    Cylinder = cmdResult.cyl,
                    Head = cmdResult.head,
                    SectorNumber = cmdResult.sector,
                    SizeCode = cmdResult.size,
                    TimeMs = timeMs
                };
                if (byLooping
                    && cmdResult.cyl == firstHeaderEncountered.cyl
                    && cmdResult.head == firstHeaderEncountered.head
                    && cmdResult.sector == firstHeaderEncountered.sector
                    && cmdResult.size == firstHeaderEncountered.size
                    && scanFormatTotalTime.ElapsedMs > 150)
                {
                    break;
                }
                if (byLooping && scanFormatTotalTime.ElapsedMs > 300) break;
            }
            result.AcceptLayout(track, ilayout, ilayoutCnt, cmdResult.sector);
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
            if (!Driver.ReadId(driverHandle, pars, out cmdResult))
            {
                Log.Trace?.Out($"Функция ReadId вернула false. Cylinder: {cmdResult.cyl} | Sector: {cmdResult.sector} | Size: {cmdResult.size} | Head: {cmdResult.head} | LastError: {Marshal.GetLastWin32Error()}");
                return 1;
            }
            if (cmdResult.sector < 1 || cmdResult.sector > 16 || cmdResult.size != 1)
            {
                Log.Trace?.Out($"Формат не TR-DOS. Cylinder: {cmdResult.cyl} | Sector: {cmdResult.sector} | Size: {cmdResult.size} | Head: {cmdResult.head}");
                return 2;
            }
            trackFormat.SyncByHeader(track, cmdResult.sector);
            Log.Trace?.Out($"Cylinder: {cmdResult.cyl} | Sector: {cmdResult.sector} | Size: {cmdResult.size} | Head: {cmdResult.head}");
            result = (UpperSideHead)cmdResult.head;
            return 0;
        }
    }

    public class DiskReaderParams : ICloneable
    {
        public UpperSideHead UpperSideHead;
        public bool UpperSideHeadAutodetect;
        public ReadMode ReadMode;
        public bool TrackLayoutAutodetect;
        public TrackFormat CurrentTrackFormat = new TrackFormat(16);
        public TrackFormat ImageSectorLayout = new TrackFormat(16);
        public DataRate DataRate;
        public DiskImage Image;
        public int SectorNumFrom;
        /// <summary>
        /// Параметр не должен выходить за границы образа (может быть равен числу секторов образа).
        /// </summary>
        public int SectorNumTo;
        public DiskSide Side;
        /// <summary>
        /// Параметр должен быть больше нуля.
        /// </summary>
        public int SectorReadAttempts;
        /// <summary>
        /// Размер сектора в байтах.
        /// </summary>
        public int SectorSize;
        public int SectorsOnTrack;
        public int TrackFrom;
        public int TrackTo;

        public object Clone()
        {
            DiskReaderParams r = (DiskReaderParams)MemberwiseClone();
            r.CurrentTrackFormat = (TrackFormat)CurrentTrackFormat.Clone();
            return r;
        }
    }

    public enum ReadMode
    {
        Standard,
        Fast
    }
}
