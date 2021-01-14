using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml;

namespace SpectrumArchiveReader
{
    public class DiskReader2
    {
        public bool Aborted;
        public DiskReaderParams2 Params;
        private Timer trackTimer = new Timer();
        private Timer scanFormatStopwatch = new Timer();
        private Timer scanFormatTotalTime = new Timer();
        private TrackFormat workTrackFormat = new TrackFormat(50);
        private TrackFormat longestTrackFormat = new TrackFormat(50);
        private TrackFormat scanFormatBuffer = new TrackFormat(50);
        public IntPtr DriverHandle;
        private IntPtr memoryHandle;

        public DiskReader2()
        {

        }

        public DiskReader2(DiskReaderParams2 pars)
        {
            Params = pars;
        }

        public bool OpenDriver()
        {
            DriverHandle = Driver.Open(Params.DataRate, Params.Drive);
            if ((int)DriverHandle == Driver.INVALID_HANDLE_VALUE) return false;
            memoryHandle = Driver.VirtualAlloc(65536);
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
            try
            {
                bool useTimeout = timeout > TimeSpan.Zero;
                DateTime timeoutTime = DateTime.Now.Add(timeout);
                Timer sectorTimer = new Timer();
                Random random = new Random();
                MList<Point> sectorArray = new MList<Point>(Params.Image.SizeSectors);
                for (int track = Params.FirstTrack; track < Params.LastTrack; track++)
                {
                    if (Params.Side == DiskSide.Side0 && track % 2 != 0) continue;
                    if (Params.Side == DiskSide.Side1 && track % 2 != 1) continue;
                    for (int sector = 0; sector < Params.Image[track].Layout.Cnt; sector++)
                    {
                        if (Params.Image[track][sector].ProcessResult != SectorProcessResult.Good) sectorArray.Add(new Point(track, sector));
                    }
                }
                int prevCylinder = -1;
                int failCounter = 0;
                while (sectorArray.Cnt > 0 && ((useTimeout && DateTime.Now < timeoutTime) || !useTimeout) && (stopOnNthFail == 0 || (failCounter < stopOnNthFail)))
                {
                    int index = random.Next(sectorArray.Cnt);
                    int track = sectorArray[index].X;
                    int sectorIndex = sectorArray[index].Y;
                    SectorInfo sector = Params.Image[track][sectorIndex];
                    TrackFormat tf = Params.Image[track];
                    int error = 23;
                    Params.Map.MarkSectorAsProcessing(track, sectorIndex);
                    bool badWritten = sector.ProcessResult == SectorProcessResult.Bad;
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
                        WinApi.RtlZeroMemory(memoryHandle, (UIntPtr)sector.SizeBytes);
                        sectorTimer.Start();
                        error = Driver.ReadSectorF(DriverHandle, memoryHandle, sector.Cylinder, sector.SectorNumber, sector.SizeCode, track & 1, sector.Head, 0x0a, 0xff);
                        sectorTimer.Stop();
                        if (error == 0)
                        {
                            Params.Image.WriteGoodSector(memoryHandle, tf, sectorIndex);
                            sectorArray.RemoveAt(index);
                            failCounter = 0;
                            timeoutTime = DateTime.Now.Add(timeout);
                            break;
                        }
                        // Ошибка 27 может быть как в случае отсутствия заголовка, так и в случае когда заголовок имеет ошибку CRC.
                        // Тут сложный вопрос что с этим делать: писать сектор как CrcError или как NoHeader. Пишется как NoHeader.
                        // (проверить была ошибка CRC в заголовке или заголовок вообще не был найден можно сравнив sectorTimer.ElapsedMs с временем вращения tf.SpinTime)
                        bool noHeader = error == 21 || error == 1112 || error == 27;
                        Make30HeadPositionings(error, track);
                        if (error == 23)
                        {
                            Params.Image.WriteBadSector(memoryHandle, tf, sectorIndex);
                            badWritten = true;
                        }
                        else if (noHeader && !badWritten)
                        {
                            Params.Image.WriteNoHeader(memoryHandle, tf, sectorIndex);
                        }
                    }
                    failCounter++;
                }
                return Params.Image.GoodSectors - goodSectors;
                abort:
                Log.Info?.Out("Чтение прервано.");
            }
            finally
            {
                Params.Map?.ClearHighlight(MapCell.Processing);
            }
            return Params.Image.GoodSectors - goodSectors;
        }

        /// <summary>
        /// Чтение вперед или назад. Возвращает количество успешно прочитанных секторов.
        /// </summary>
        /// <returns>Возвращает количество успешно прочитанных секторов.</returns>
        public int Read(ScanMode scanMode, bool forward)
        {
            int goodSectors = Params.Image.GoodSectors;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GCLatencyMode oldGCLatencyMode = GCSettings.LatencyMode;
            try
            {
                GCSettings.LatencyMode = GCLatencyMode.LowLatency;
                int firstTrack = Params.FirstTrack;
                int prevCylinder = -1;
                int lastTrack = Params.LastTrack;
                trackTimer.Restart();
                int track = forward ? firstTrack : lastTrack - 1;
                for (int p = firstTrack; p < lastTrack; p++, track = forward ? track + 1 : track - 1)
                {
                    if (Aborted) goto abort;
                    if (Params.Side == DiskSide.Side0 && (track % 2 != 0)) continue;
                    if (Params.Side == DiskSide.Side1 && (track % 2 != 1)) continue;
                    TrackFormat trackF = Params.Image[track];
                    bool scan = ((scanMode == ScanMode.Once)
                        || (scanMode == ScanMode.UnscannedOnly && trackF.FormatName == TrackFormatName.Unscanned)
                        || scanMode == ScanMode.EachTrackRead)
                        && (trackF.MaxGap() > 128 + TrackFormat.MinSectorHeaderSize || trackF.ContainsCalculatedTime());
                    if (!scan && trackF.NotGoodSectors == 0) continue;
                    int cylinder = track / 2;
                    if (cylinder != prevCylinder)
                    {
                        Driver.Seek(DriverHandle, track);
                        if (Aborted) goto abort;
                        prevCylinder = cylinder;
                    }
                    ReadTrack(track, Params, scanMode);
                }
                return Params.Image.GoodSectors - goodSectors;
                abort:
                Log.Info?.Out("Чтение прервано.");
            }
            finally
            {
                GCSettings.LatencyMode = oldGCLatencyMode;
                Params.Map?.ClearHighlight(MapCell.Processing);
            }
            return Params.Image.GoodSectors - goodSectors;
        }

        public unsafe void ReadTrack(int track, DiskReaderParams2 pars, ScanMode scanMode)
        {
            // Массив sectors:
            // D0 - Признак обработанного сектора при чтении трека в одной попытке. Обнуляется перед каждой попыткой чтения трека.
            // D1 - Используется в блоке чтения заголовков. Отмечаются сектора заголовки которых были найдены. Вне этого блока не используется.
            // D2 - Запрет чтения сектора из-за того что его заголовок был прочитан SectorReadAttempts раз и не был найден. Параметр сохраняется между попытками чтения трека.

            TrackFormat trackF = pars.Image.Tracks[track];

            const int sectorArrayLen = 50;
            int* sectors = stackalloc int[sectorArrayLen];
            for (int i = 0; i < sectorArrayLen; i++)
            {
                sectors[i] = 0;
            }
            bool trackScanned = false;
            for (int attempt = 0; attempt < pars.SectorReadAttempts; attempt++)
            {
                if (Aborted) return;

                // Сканирование трека.

                if (
                    ((scanMode == ScanMode.Once && !trackScanned)
                    || (scanMode == ScanMode.UnscannedOnly && trackF.FormatName == TrackFormatName.Unscanned)
                    || scanMode == ScanMode.EachTrackRead)

                    && (trackF.MaxGap() > 128 + TrackFormat.MinSectorHeaderSize || trackF.ContainsCalculatedTime())
                    )
                {
                    // workTrackFormat и longestTrackFormat используются чтобы не плодить объекты.
                    // Объект scanFormatBuffer используется функциями ScanFormat и CombineFormats, поэтому его брать нельзя.

                    trackF.Scanning = true;
                    trackF.MapModified = true;
                    if (ScanFormat(workTrackFormat, track, true))
                    {
                        for (int i = 0; i < workTrackFormat.Layout.Cnt; i++)
                        {
                            workTrackFormat.Layout.Data[i].MapCellValue = MapCell.Unprocessed;
                        }
                        if (CombineFormats(track, trackF, workTrackFormat, longestTrackFormat))
                        {
                            trackF.Assign(longestTrackFormat);
                            trackF.MapModified = true;
                        }
                    }
                    trackF.Scanning = false;
                    trackF.MapModified = true;
                    trackScanned = true;
                    if (Aborted) return;
                }

                if (trackF.Layout.Cnt > sectorArrayLen)
                {
                    Log.Error?.Out($"Число секторов превышает размер рабочего массива: {trackF.Layout.Cnt}");
                    throw new Exception();
                }
                for (int i = 0; i < trackF.Layout.Cnt; i++)
                {
                    sectors[i] &= ~1;
                }
                bool wasError = false;
                trackTimer.Restart();
                int skip = 0;
                int processedSectors = 0;
                int sectorCounter = 0;
                SectorInfo diskSector;
                while (processedSectors < trackF.Layout.Cnt)
                {
                    diskSector = trackF.Layout.Data[sectorCounter];
                    int sectorIndex = sectorCounter;
                    sectorCounter++;
                    skip++;
                    if ((sectors[sectorIndex] & 1) != 0) continue;
                    sectors[sectorIndex] |= 1;
                    processedSectors++;
                    if ((sectors[sectorIndex] & 4) != 0) continue;
                    if (trackF.Layout.Data[sectorIndex].ProcessResult == SectorProcessResult.Good) continue;
                    skip = 0;
                    pars.Map.MarkSectorAsProcessing(track, sectorIndex);
                    WinApi.RtlZeroMemory(memoryHandle, (UIntPtr)diskSector.SizeBytes);
                    int error = Driver.ReadSectorF(DriverHandle, memoryHandle, diskSector.Cylinder, diskSector.SectorNumber, diskSector.SizeCode, track & 1, diskSector.Head, 0x0a, 0xff);
                    double curTimeSinceSync = trackF.Timer.ElapsedMs;
                    bool badWritten = diskSector.ProcessResult == SectorProcessResult.Bad;
                    if (error == 0)
                    {
                        pars.Image.WriteGoodSector(memoryHandle, trackF, sectorIndex);
                    }
                    else
                    {
                        wasError = true;
                        // Если надо проверить CRC-Error заголовка: (error == 27 && curTimeSinceSync > trackF.SpinTime)
                        bool noHeader = error == 21 || error == 1112 || error == 27;
                        if (noHeader)
                        {
                            if (!badWritten) pars.Image.WriteNoHeader(memoryHandle, trackF, sectorIndex);
                            Make30HeadPositionings(error, track);
                        }
                        else if (error == 23)
                        {
                            pars.Image.WriteBadSector(memoryHandle, trackF, sectorIndex);
                            badWritten = true;
                        }
                        else
                        {
                            Log.Info?.Out($"Необработанная ошибка при чтении сектора: {error}");
                        }
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
        /// Объединение форматов двух результатов сканирования одного трека. У треков main и add будут изменены тайминги при работе этой функции!
        /// </summary>
        /// <param name="track"></param>
        /// <param name="main">Основной трек.</param>
        /// <param name="add">Добавляемый трек.</param>
        /// <param name="result"></param>
        /// <returns>true - объединение успешно, false - объединение не удалось по какой-то причине (невозможность синхронизации, несовместимость треков).</returns>
        public bool CombineFormats(int track, TrackFormat main, TrackFormat add, TrackFormat result)
        {
            if (add.Layout.Cnt == 0)
            {
                result.Assign(main);
                if (result.FormatName == TrackFormatName.Unscanned) result.FormatName = TrackFormatName.NoHeaders;
                return true;
            }
            if (main.Layout.Cnt == 0 && add.Layout.Cnt > 0)
            {
                result.AssignLayout(add, track);
                return true;
            }

            int indexM = 0;
            int indexA = 0;
            for (int i = 0; i < main.Layout.Cnt; i++)
            {
                for (int j = 0; j < add.Layout.Cnt; j++)
                {
                    if (main.Layout.Data[i].Parameters == add.Layout.Data[j].Parameters)
                    {
                        indexM = i;
                        indexA = j;
                        goto outside;
                    }
                }
            }
            return false;

            outside:
            int iM = indexM;
            int iA = indexA;
            int iR = 0;
            scanFormatBuffer.Layout.EnsureCapacity(main.Layout.Cnt + add.Layout.Cnt);
            scanFormatBuffer.Layout.Cnt = 0;
            int totalPassedInMain = 0;
            double timeM = main.Layout.Data[(iM + 1) % main.Layout.Cnt].TimeMs;
            double timeA = add.Layout.Data[(iA + 1) % add.Layout.Cnt].TimeMs;

            while (totalPassedInMain < main.Layout.Cnt)
            {
                int iM1 = (iM + 1) % main.Layout.Cnt;
                int iA1 = (iA + 1) % add.Layout.Cnt;
                if (main.Layout.Data[iM1].TimeCalculated)
                {
                    if (main.Layout.Data[iM1].Parameters == add.Layout.Data[iA1].Parameters)
                    {
                        iM = iM1;
                        totalPassedInMain++;
                        iA = iA1;
                        scanFormatBuffer.Layout.Data[iR] = main.Layout.Data[iM];
                        scanFormatBuffer.Layout.Data[iR].TimeMs = add.Layout.Data[iA].TimeMs;
                        scanFormatBuffer.Layout.Data[iR].TimeCalculated = false;
                        iR++;
                    }
                    else
                    {
                        return false;
                        //totalPassedInMain++;
                        //for (int t = 0, r = iM1, tlast = main.Layout.Cnt - totalPassedInMain; t < tlast; t++, totalPassedInMain++, r = r % main.Layout.Cnt)
                        //{
                        //    for (int y = 0; y < add.Layout.Cnt; y++)
                        //    {
                        //        if (main.Layout.Data[r].Parameters == add.Layout.Data[y].Parameters)
                        //        {
                        //            iM = r;
                        //            iA = y;
                        //            goto outOfLoop;
                        //        }
                        //    }
                        //}
                        //outOfLoop:;
                    }
                }
                else if (Math.Abs(timeM - timeA) / timeM < 0.1)
                {
                    iM = iM1;
                    totalPassedInMain++;
                    iA = iA1;
                    if (main.Layout.Data[iM].Parameters != add.Layout.Data[iA].Parameters)
                    {
                        Log.Info?.Out($"Несовпадение параметров секторов при совмещении форматов. Основной сектор: {main[iM]} | Добавляемый сектор: {add[iA]}");
                        return false;
                    }
                    scanFormatBuffer.Layout.Data[iR] = main.Layout.Data[iM];
                    iR++;
                    timeM = main.Layout.Data[(iM + 1) % main.Layout.Cnt].TimeMs;
                    timeA = add.Layout.Data[(iA + 1) % add.Layout.Cnt].TimeMs;
                }
                else if (timeA < timeM)
                {
                    timeM -= timeA;
                    main.Layout.Data[iM1].TimeMs -= timeA;
                    iA = iA1;
                    scanFormatBuffer.Layout.Data[iR] = add.Layout.Data[iA];

                    // Проверки ниже закомментированы, т.к. стало понятно что заголовки могут быть без данных и быть как следствие с более высокой плотностью.

                    // Проверка выхода конца добавляемого сектора дальше начала следующего сектора в массиве main.
                    if (timeM < (add.Layout.Data[iA].SizeBytes + TrackFormat.MinSectorHeaderSize) / add.BytesPerMs) return false;

                    // Проверка выхода начала добавляемого сектора влево от конца предыдущего сектора в массиве main.
                    if (timeA < (main.Layout.Data[iM].SizeBytes + TrackFormat.MinSectorHeaderSize) / main.BytesPerMs) return false;

                    iR++;
                    timeA = add.Layout.Data[(iA + 1) % add.Layout.Cnt].TimeMs;
                }
                else
                {
                    timeA -= timeM;
                    add.Layout.Data[iA1].TimeMs -= timeM;
                    iM = iM1;
                    totalPassedInMain++;
                    scanFormatBuffer.Layout.Data[iR] = main.Layout.Data[iM];
                    iR++;
                    timeM = main.Layout.Data[(iM + 1) % main.Layout.Cnt].TimeMs;
                }
            }
            scanFormatBuffer.Layout.Cnt = iR;
            result.AssignLayout(scanFormatBuffer, track);
            return true;
        }

        /// <summary>
        /// Здесь делается 30 вызовов позиционирования.
        /// Причина в следующем. Если случается ошибка 21, то серия следующих чтений приводит к ошибке 21
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

    public class DiskReaderParams2 : ICloneable
    {
        public Drive Drive;
        public ReadMode ReadMode;
        public bool TrackLayoutAutodetect;
        public DataRate DataRate;
        public DiskImage2 Image;
        public Map2 Map;
        public DiskSide Side;
        /// <summary>
        /// Параметр должен быть больше нуля.
        /// </summary>
        public int SectorReadAttempts;
        public int FirstTrack;
        public int LastTrack;

        public object Clone()
        {
            DiskReaderParams2 r = (DiskReaderParams2)MemberwiseClone();
            return r;
        }

        public void SaveToXml(XmlTextWriter xml, string name)
        {
            xml.WriteStartElement(name);
            xml.WriteAttributeString("Drive", Drive.ToString());
            xml.WriteAttributeString("DataRate", DataRate.ToString());
            xml.WriteAttributeString("Side", Side.ToString());
            xml.WriteAttributeString("ReadMode", ReadMode.ToString());
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
            SectorReadAttempts = Math.Max(0, int.Parse(xml.GetAttribute("SectorReadAttempts")));
            FirstTrack = Math.Max(0, int.Parse(xml.GetAttribute("FirstTrack")));
            FirstTrack = Math.Min(171, FirstTrack);
            LastTrack = Math.Min(172, int.Parse(xml.GetAttribute("LastTrack")));
            LastTrack = Math.Max(0, LastTrack);
        }
    }
}
