using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SpectrumArchiveReader
{
    public class Map
    {
        public ChartArea ChartArea;
        public int TrackFrom;
        public int TrackTo;
        private int trackFromOld;
        private int trackToOld;
        public const int CellWidth = 6;
        public const int CellHeight = 6;
        public int MaxTrack;
        public int SectorsOnTrack;
        public int SectorSize;
        public byte ZeroByte;
        public MapCell[] WorkMap;
        private MapCell[] oldMap;
        private Color backColor;
        private const int headerHeight = 10;
        private const int stripHeight = 5;
        private int oldSectorArraySize;
        public event MouseEventHandler DoubleClick;

        public Map(int maxTrack, int sectorSize, int sectorsOnTrack, Control parent, Color backColor)
        {
            MaxTrack = maxTrack;
            SectorSize = sectorSize;
            SectorsOnTrack = sectorsOnTrack;
            WorkMap = new MapCell[MaxTrack * SectorsOnTrack];
            oldMap = new MapCell[MaxTrack * SectorsOnTrack];
            ChartArea = new ChartArea(parent);
            this.backColor = backColor;
            for (int i = 0; i < oldMap.Length; i++)
            {
                oldMap[i] = (MapCell)0xFFFF;
            }
            ChartArea.Size = new Size(MaxTrack * CellWidth, SectorsOnTrack * CellHeight + headerHeight);
            ChartArea.MouseDoubleClick += ChartArea_MouseDoubleClick;
        }

        private void ChartArea_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (DoubleClick != null) DoubleClick(sender, e);
        }

        public void SetPosition(int left, int top)
        {
            ChartArea.Left = left;
            ChartArea.Top = top;
        }

        public void ClearHighlight(MapCell value = MapCell.Highlighted)
        {
            for (int i = 0; i < WorkMap.Length; i++)
            {
                WorkMap[i] = WorkMap[i] & ~value;
            }
        }

        public void BuildMap(byte[] data, SectorProcessResult[] sectors)
        {
            int i;
            for (i = 0; i < sectors.Length; i++)
            {
                switch (sectors[i])
                {
                    case SectorProcessResult.Unprocessed:
                        WorkMap[i] = MapCell.Unprocessed;
                        break;

                    case SectorProcessResult.Good:
                        WorkMap[i] = DiskImage.AllBytes(data, i * SectorSize, SectorSize, ZeroByte) ? MapCell.Zero : MapCell.Good;
                        break;

                    case SectorProcessResult.Bad:
                        WorkMap[i] = MapCell.CrcError;
                        break;

                    case SectorProcessResult.NoHeader:
                        WorkMap[i] = MapCell.NoHeader;
                        break;
                }
            }
            for (; i < WorkMap.Length; i++)
            {
                WorkMap[i] = 0;
            }
            oldSectorArraySize = sectors.Length;
        }

        public void ModifySectors(int index, int length, byte[] data, SectorProcessResult[] sectors)
        {
            int last = Math.Min(index + length, sectors.Length);
            for (int sectorNum = index; sectorNum < last; sectorNum++)
            {
                MapCell cell = WorkMap[sectorNum] & (MapCell.Highlighted | MapCell.Hover);
                switch (sectors[sectorNum])
                {
                    case SectorProcessResult.Unprocessed:
                        WorkMap[sectorNum] = cell | MapCell.Unprocessed;
                        break;

                    case SectorProcessResult.Good:
                        WorkMap[sectorNum] = cell | (DiskImage.AllBytes(data, sectorNum * SectorSize, SectorSize, ZeroByte) ? MapCell.Zero : MapCell.Good);
                        break;

                    case SectorProcessResult.Bad:
                        WorkMap[sectorNum] = cell | MapCell.CrcError;
                        break;

                    case SectorProcessResult.NoHeader:
                        WorkMap[sectorNum] = cell | MapCell.NoHeader;
                        break;
                }
            }
            for (int i = sectors.Length; i < oldSectorArraySize; i++)
            {
                WorkMap[i] = 0;
            }
            oldSectorArraySize = sectors.Length;
        }

        public void Select(int trackFrom, int trackTo)
        {
            TrackFrom = trackFrom;
            TrackTo = trackTo;
            ClearHighlight(MapCell.Highlighted | MapCell.Hover);
            for (int i = trackFrom * SectorsOnTrack, last = trackTo * SectorsOnTrack; i < last; i++)
            {
                WorkMap[i] |= MapCell.Highlighted;
            }
        }

        public void HighlightFile(int sectorNumber, int size)
        {
            for (int i = sectorNumber, last = sectorNumber + size; i < last; i++)
            {
                WorkMap[i] |= MapCell.Highlighted;
            }
        }

        public void MarkAsProcessing(int sectorNumber, int count = 1)
        {
            for (int i = sectorNumber, last = i + count; i < last; i++)
            {
                WorkMap[i] |= MapCell.Processing;
            }
        }

        public void MarkAsScanning(int index, int length = 1)
        {
            for (int i = index, last = i + length; i < last; i++)
            {
                WorkMap[i] |= MapCell.Scanning;
            }
        }

        public void GetTrackSectorByMousePosition(int x, int y, out int track, out int sector)
        {
            track = x >= 0 ? x / CellWidth : -1;
            sector = y >= headerHeight ? (y - headerHeight) / CellHeight : -1;
        }

        private int GetCellColor(MapCell cell)
        {
            if (cell == 0) return ColorTranslator.ToWin32(backColor);
            int color = 0;
            if ((cell & MapCell.Zero) != 0) color = ColorTranslator.ToWin32(Color.Gray);
            if ((cell & MapCell.CrcError) != 0) unchecked { color = ColorTranslator.ToWin32(Color.FromArgb((int)0xFFAF0000)); }
            if ((cell & MapCell.NoHeader) != 0) unchecked { color = ColorTranslator.ToWin32(Color.FromArgb((int)0xFF0000DF)); }
            if ((cell & MapCell.Good) != 0) color = ColorTranslator.ToWin32(Color.Green);
            if ((cell & MapCell.Unprocessed) != 0) color = ColorTranslator.ToWin32(Color.Wheat);
            if ((cell & MapCell.Processing) != 0) unchecked { color = ColorTranslator.ToWin32(Color.Black); }
            if ((cell & MapCell.Scanning) != 0) color = ColorTranslator.ToWin32(Color.Cyan);
            if ((cell & MapCell.Highlighted) != 0) color += 0x00202020;
            return color;
        }

        private bool PaintMap()
        {
            bool paintMap = false;
            for (int i = 0; i < WorkMap.Length; i++)
            {
                if (WorkMap[i] != oldMap[i])
                {
                    paintMap = true;
                    break;
                }
            }
            bool paintHeader = TrackFrom != trackFromOld || TrackTo != trackToOld;
            if (!paintMap && !paintHeader) return false;
            IntPtr DC = ChartArea.bDC;
            IntPtr penNull = WinApi.CreatePen(WinApi.PS_NULL, 1, (uint)ColorTranslator.ToWin32(Color.White));
            IntPtr oldBrush = WinApi.SelectObject(DC, WinApi.GetStockObject(WinApi.StockObjects.DC_BRUSH));
            IntPtr oldPen = WinApi.SelectObject(DC, penNull);
            IntPtr penBlack = WinApi.CreatePen(WinApi.PS_SOLID, 1, (uint)ColorTranslator.ToWin32(Color.Black));
            if (paintMap)
            {
                for (int i = 0; i < WorkMap.Length; i++)
                {
                    if (WorkMap[i] == oldMap[i]) continue;
                    oldMap[i] = WorkMap[i];
                    WinApi.SetDCBrushColor(DC, GetCellColor(WorkMap[i]));
                    int track = i / SectorsOnTrack;
                    int sector = i % SectorsOnTrack;
                    if ((WorkMap[i] & MapCell.Hover) != 0)
                    {
                        IntPtr penX = WinApi.SelectObject(DC, penBlack);
                        WinApi.Rectangle(DC, CellWidth * track, headerHeight + sector * CellHeight, CellWidth * (track + 1), headerHeight + (sector + 1) * CellHeight);
                        WinApi.SelectObject(DC, penX);
                    }
                    else
                    {
                        WinApi.Rectangle(DC, CellWidth * track, headerHeight + sector * CellHeight, CellWidth * (track + 1) + 1, headerHeight + (sector + 1) * CellHeight + 1);
                    }
                }
            }
            if (paintHeader)
            {
                trackFromOld = TrackFrom;
                trackToOld = TrackTo;
                WinApi.SetDCBrushColor(DC, ColorTranslator.ToWin32(backColor));
                WinApi.Rectangle(DC, 0, stripHeight, MaxTrack * CellWidth + 1, headerHeight + 1);
                if (TrackFrom > 0) WinApi.Rectangle(DC, 0, 0, TrackFrom * CellWidth + 1, stripHeight + 1);
                if (TrackTo < MaxTrack) WinApi.Rectangle(DC, TrackTo * CellWidth, 0, MaxTrack * CellWidth + 1, stripHeight + 1);
                WinApi.SetDCBrushColor(DC, ColorTranslator.ToWin32(Color.Black));
                WinApi.Rectangle(DC, TrackFrom * CellWidth, 0, TrackTo * CellWidth + 1, stripHeight + 1);
                WinApi.Rectangle(DC, 160 * CellWidth, stripHeight + 1, 172 * CellWidth + 1, headerHeight);
            }
            WinApi.SelectObject(DC, oldPen);
            WinApi.SelectObject(DC, oldBrush);
            WinApi.DeleteObject(penNull);
            WinApi.DeleteObject(penBlack);
            return true;
        }

        public void Repaint()
        {
            if (PaintMap()) ChartArea.Repaint();
        }
    }

    [Flags]
    public enum MapCell
    {
        Unprocessed = 0x01,
        Good = 0x02,
        NoHeader = 0x04,
        CrcError = 0x08,
        Zero = 0x10,
        Highlighted = 0x20,
        Hover = 0x40,
        Processing = 0x80,
        Scanning = 0x0100
    }
}
