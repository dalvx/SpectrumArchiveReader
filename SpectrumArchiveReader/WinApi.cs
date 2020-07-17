using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SpectrumArchiveReader
{
    public static class WinApi
    {
        // ShowWindow
        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;

        // For GetCurrentObject
        public const int OBJ_BITMAP = 7;
        public const int OBJ_BRUSH = 2;
        public const int OBJ_FONT = 6;
        public const int OBJ_PAL = 5;
        public const int OBJ_PEN = 1;
        public const int OBJ_EXTPEN = 11;
        public const int OBJ_REGION = 8;
        public const int OBJ_DC = 3;
        public const int OBJ_MEMDC = 10;
        public const int OBJ_METAFILE = 9;
        public const int OBJ_METADC = 4;
        public const int OBJ_ENHMETAFILE = 13;
        public const int OBJ_ENHMETADC = 12;

        // PenStyles
        public const int PS_SOLID = 0;
        public const int PS_DASH = 1;
        public const int PS_DOT = 2;
        public const int PS_DASHDOT = 3;
        public const int PS_DASHDOTDOT = 4;
        public const int PS_NULL = 5;
        public const int PS_INSIDEFRAME = 6;

        // Brush Styles
        public const int BS_SOLID = 0;
        public const int BS_HOLLOW = 1;
        public const int BS_NULL = 1;
        public const int BS_HATCHED = 2;
        public const int BS_PATTERN = 3;
        public const int BS_DIBPATTERN = 5;

        public const int HS_HORIZONTAL = 0;
        public const int HS_VERTICAL = 1;
        public const int HS_FDIAGONAL = 2;
        public const int HS_BDIAGONAL = 3;
        public const int HS_CROSS = 4;
        public const int HS_DIAGCROSS = 5;
        public const int DIB_RGB_COLORS = 0;
        public const int DIB_PAL_COLORS = 1;
        public const int TRANSPARENT = 1;
        public const int OPAQUE = 2;
        public const int FLOODFILLBORDER = 0;
        public const int FLOODFILLSURFACE = 1;
        public const int MM_TEXT = 1;
        public const int MM_LOMETRIC = 2;
        public const int MM_HIMETRIC = 3;
        public const int MM_LOENGLISH = 4;
        public const int MM_HIENGLISH = 5;
        public const int MM_HITWIPS = 6;
        public const int MM_ISOTROPIC = 7;
        public const int MM_ANISOTROPIC = 8;
        public const int ALTERNATE = 1;
        public const int WINDING = 2;
        public const int STRETCH_ANDSCANS = 1;
        public const int STRETCH_ORSCANS = 2;
        public const int STRETCH_DELETESCANS = 3;
        public const int TA_TOP = 0;
        public const int TA_BOTTOM = 8;
        public const int TA_BASELINE = 24;
        public const int TA_LEFT = 0;
        public const int TA_RIGHT = 2;
        public const int TA_CENTER = 6;
        public const int TA_NOUPDATECP = 0;
        public const int TA_UPDATECP = 1;

        /// <summary>
        /// Pixel is always 0.
        /// </summary>
        public const int R2_BLACK = 1;
        /// <summary>
        /// Pixel is the inverse of the R2_MERGEPEN color.
        /// </summary>
        public const int R2_NOTMERGEPEN = 2;
        /// <summary>
        /// Pixel is a combination of the colors common to both the screen and the inverse of the pen.
        /// </summary>
        public const int R2_MASKNOTPEN = 3;
        /// <summary>
        /// Pixel is the inverse of the pen color.
        /// </summary>
        public const int R2_NOTCOPYPEN = 4;
        /// <summary>
        /// Pixel is a combination of the colors common to both the pen and the inverse of the screen.
        /// </summary>
        public const int R2_MASKPENNOT = 5;
        /// <summary>
        /// Pixel is the inverse of the screen color.
        /// </summary>
        public const int R2_NOT = 6;
        /// <summary>
        /// Pixel is a combination of the colors in the pen and in the screen, but not in both.
        /// </summary>
        public const int R2_XORPEN = 7;
        /// <summary>
        /// Pixel is the inverse of the R2_MASKPEN color.
        /// </summary>
        public const int R2_NOTMASKPEN = 8;
        /// <summary>
        /// Pixel is a combination of the colors common to both the pen and the screen.
        /// </summary>
        public const int R2_MASKPEN = 9;
        /// <summary>
        /// Pixel is the inverse of the R2_XORPEN color.
        /// </summary>
        public const int R2_NOTXORPEN = 10;
        /// <summary>
        /// Pixel remains unchanged.
        /// </summary>
        public const int R2_NOP = 11;
        /// <summary>
        /// Pixel is a combination of the screen color and the inverse of the pen color.
        /// </summary>
        public const int R2_MERGENOTPEN = 12;
        /// <summary>
        /// Pixel is the pen color.
        /// </summary>
        public const int R2_COPYPEN = 13;
        /// <summary>
        /// Pixel is a combination of the pen color and the inverse of the screen color.
        /// </summary>
        public const int R2_MERGEPENNOT = 14;
        /// <summary>
        /// Pixel is a combination of the pen color and the screen color.
        /// </summary>
        public const int R2_MERGEPEN = 15;
        /// <summary>
        /// Pixel is always 1.
        /// </summary>
        public const int R2_WHITE = 16;

        public const int ETO_OPAQUE = 2;
        public const int ETO_CLIPPED = 4;
        public const int BLACKNESS = 66;
        public const int NOTSRCERASE = 0x1100a6;
        public const int NOTSRCCOPY = 0x330008;
        public const int SRCERASE = 0x440328;
        public const int DSTINVERT = 0x550009;
        public const int PATINVERT = 0x5a0049;
        public const int SRCINVERT = 0x660046;
        public const int SRCAND = 0x8800c6;
        public const int MERGEPAINT = 0xbb0226;
        public const int SRCCOPY = 0xcc0020;
        public const int SRCPAINT = 0xee0086;
        public const int PATCOPY = 0xf00021;
        public const int PATPAINT = 0xfb0a09;
        public const int WHITENESS = 0xff0062;

        // Константы для функции DrawText, даются в uFormat по принципу OR (|)
        public const int DT_TOP = 0x00000000;
        public const int DT_LEFT = 0x00000000;
        public const int DT_CENTER = 0x00000001;
        public const int DT_RIGHT = 0x00000002;
        public const int DT_VCENTER = 0x00000004;
        public const int DT_BOTTOM = 0x00000008;
        public const int DT_WORDBREAK = 0x00000010;
        public const int DT_SINGLELINE = 0x00000020;
        public const int DT_EXPANDTABS = 0x00000040;
        public const int DT_TABSTOP = 0x00000080;
        public const int DT_NOCLIP = 0x00000100;
        public const int DT_EXTERNALLEADING = 0x00000200;
        public const int DT_CALCRECT = 0x00000400;
        public const int DT_NOPREFIX = 0x00000800;
        public const int DT_INTERNAL = 0x00001000;

        // Для шрифтов. Weight:
        public const int FW_DONTCARE = 0;
        public const int FW_THIN = 100;
        public const int FW_EXTRALIGHT = 200;
        public const int FW_ULTRALIGHT = 200;
        public const int FW_LIGHT = 300;
        public const int FW_NORMAL = 400;
        public const int FW_REGULAR = 400;
        public const int FW_MEDIUM = 500;
        public const int FW_SEMIBOLD = 600;
        public const int FW_DEMIBOLD = 600;
        public const int FW_BOLD = 700;
        public const int FW_EXTRABOLD = 800;
        public const int FW_ULTRABOLD = 800;
        public const int FW_HEAVY = 900;
        public const int FW_BLACK = 900;

        public const int ANSI_CHARSET = 0;
        public const int ARABIC_CHARSET = 178;
        public const int BALTIC_CHARSET = 186;
        public const int CHINESEBIG5_CHARSET = 136;
        public const int DEFAULT_CHARSET = 1;
        public const int EASTEUROPE_CHARSET = 238;
        public const int GB2312_CHARSET = 134;
        public const int GREEK_CHARSET = 161;
        public const int HANGEUL_CHARSET = 129;
        public const int HEBREW_CHARSET = 177;
        public const int JOHAB_CHARSET = 130;
        public const int MAC_CHARSET = 77;
        public const int OEM_CHARSET = 255;
        public const int RUSSIAN_CHARSET = 204;
        public const int SHIFTJIS_CHARSET = 128;
        public const int SYMBOL_CHARSET = 2;
        public const int THAI_CHARSET = 222;
        public const int TURKISH_CHARSET = 162;
        public const int OUT_DEFAULT_PRECIS = 0;
        public const int OUT_DEVICE_PRECIS = 5;
        public const int OUT_OUTLINE_PRECIS = 8;
        public const int OUT_RASTER_PRECIS = 6;
        public const int OUT_STRING_PRECIS = 1;
        public const int OUT_STROKE_PRECIS = 3;
        public const int OUT_TT_ONLY_PRECIS = 7;
        public const int OUT_TT_PRECIS = 4;
        public const int CLIP_CHARACTER_PRECIS = 1;
        public const int CLIP_MASK = 15;
        public const int CLIP_TT_ALWAYS = 32;
        public const int CLIP_DEFAULT_PRECIS = 0;
        public const int CLIP_EMBEDDED = 128;
        public const int CLIP_LH_ANGLES = 16;
        public const int CLIP_STROKE_PRECIS = 2;
        public const int ANTIALIASED_QUALITY = 4;
        public const int DEFAULT_QUALITY = 0;
        public const int DRAFT_QUALITY = 1;
        public const int NONANTIALIASED_QUALITY = 3;
        public const int PROOF_QUALITY = 2;
        public const int DEFAULT_PITCH = 0;
        public const int FIXED_PITCH = 1;
        public const int VARIABLE_PITCH = 2;
        public const int FF_DECORATIVE = 80;
        public const int FF_DONTCARE = 0;
        public const int FF_ROMAN = 16;
        public const int FF_SCRIPT = 64;
        public const int FF_SWISS = 32;
        public const int FF_MODERN = 48;

        public const int WS_BORDER = 0x800000;
        public const int WS_CAPTION = 0xc00000;
        public const int WS_CHILD = 0x40000000;
        public const int WS_CLIPCHILDREN = 0x2000000;
        public const int WS_CLIPSIBLINGS = 0x4000000;
        public const int WS_DISABLED = 0x8000000;
        public const int WS_DLGFRAME = 0x400000;
        public const int WS_EX_APPWINDOW = 0x40000;
        public const int WS_EX_CLIENTEDGE = 0x200;
        public const int WS_EX_CONTEXTHELP = 0x400;
        public const int WS_EX_CONTROLPARENT = 0x10000;
        public const int WS_EX_DLGMODALFRAME = 1;
        public const int WS_EX_LAYERED = 0x80000;
        public const int WS_EX_LAYOUTRTL = 0x400000;
        public const int WS_EX_LEFT = 0;
        public const int WS_EX_LEFTSCROLLBAR = 0x4000;
        public const int WS_EX_MDICHILD = 0x40;
        public const int WS_EX_NOINHERITLAYOUT = 0x100000;
        public const int WS_EX_RIGHT = 0x1000;
        public const int WS_EX_RTLREADING = 0x2000;
        public const int WS_EX_STATICEDGE = 0x20000;
        public const int WS_EX_TOOLWINDOW = 0x80;
        public const int WS_EX_TOPMOST = 8;
        public const int WS_EX_TRANSPARENT = 0x20;
        public const int WS_HSCROLL = 0x100000;
        public const int WS_MAXIMIZE = 0x1000000;
        public const int WS_MAXIMIZEBOX = 0x10000;
        public const int WS_MINIMIZE = 0x20000000;
        public const int WS_MINIMIZEBOX = 0x20000;
        public const int WS_OVERLAPPED = 0;
        public const int WS_POPUP = -2147483648;
        public const int WS_SYSMENU = 0x80000;
        public const int WS_TABSTOP = 0x10000;
        public const int WS_THICKFRAME = 0x40000;
        public const int WS_VISIBLE = 0x10000000;
        public const int WS_VSCROLL = 0x200000;

        public const int SWP_NOACTIVATE = 0x0010;
        public const int SWP_SHOWWINDOW = 0x0040;

        public const int ERROR_CLASS_ALREADY_EXISTS = 1410;

        public enum ShowWindowCommands
        {
            /// <summary>
            /// Hides the window and activates another window.
            /// </summary>
            Hide = 0,
            /// <summary>
            /// Activates and displays a window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position.
            /// An application should specify this flag when displaying the window 
            /// for the first time.
            /// </summary>
            Normal = 1,
            /// <summary>
            /// Activates the window and displays it as a minimized window.
            /// </summary>
            ShowMinimized = 2,
            /// <summary>
            /// Maximizes the specified window.
            /// </summary>
            Maximize = 3, // is this the right value?
            /// <summary>
            /// Activates the window and displays it as a maximized window.
            /// </summary>       
            ShowMaximized = 3,
            /// <summary>
            /// Displays a window in its most recent size and position. This value 
            /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except 
            /// the window is not activated.
            /// </summary>
            ShowNoActivate = 4,
            /// <summary>
            /// Activates the window and displays it in its current size and position. 
            /// </summary>
            Show = 5,
            /// <summary>
            /// Minimizes the specified window and activates the next top-level 
            /// window in the Z order.
            /// </summary>
            Minimize = 6,
            /// <summary>
            /// Displays the window as a minimized window. This value is similar to
            /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the 
            /// window is not activated.
            /// </summary>
            ShowMinNoActive = 7,
            /// <summary>
            /// Displays the window in its current size and position. This value is 
            /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the 
            /// window is not activated.
            /// </summary>
            ShowNA = 8,
            /// <summary>
            /// Activates and displays the window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position. 
            /// An application should specify this flag when restoring a minimized window.
            /// </summary>
            Restore = 9,
            /// <summary>
            /// Sets the show state based on the SW_* value specified in the 
            /// STARTUPINFO structure passed to the CreateProcess function by the 
            /// program that started the application.
            /// </summary>
            ShowDefault = 10,
            /// <summary>
            ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread 
            /// that owns the window is not responding. This flag should only be 
            /// used when minimizing windows from a different thread.
            /// </summary>
            ForceMinimize = 11
        }

        // Messages.
        public const int WM_NCHITTEST = 0x0084;
        public const int HTTRANSPARENT = -1;
        public const int WM_PAINT = 15;
        public const int WM_NCPAINT = 0x0085;
        public const int WM_ERASEBKGND = 20;
        public const int WM_DESTROY = 2;

        public const uint BDR_INNER = 12;
        public const uint BDR_OUTER = 3;
        public const uint BDR_RAISED = 5;
        public const uint BDR_RAISEDINNER = 4;
        public const uint BDR_RAISEDOUTER = 1;
        public const uint BDR_SUNKEN = 10;
        public const uint BDR_SUNKENINNER = 8;
        public const uint BDR_SUNKENOUTER = 2;
        public const uint BF_ADJUST = 0x2000;
        public const uint BF_BOTTOM = 8;
        public const uint BF_BOTTOMLEFT = 9;
        public const uint BF_BOTTOMRIGHT = 12;
        public const uint BF_DIAGONAL = 0x10;
        public const uint BF_DIAGONAL_ENDBOTTOMLEFT = 0x19;
        public const uint BF_DIAGONAL_ENDBOTTOMRIGHT = 0x1c;
        public const uint BF_DIAGONAL_ENDTOPLEFT = 0x13;
        public const uint BF_DIAGONAL_ENDTOPRIGHT = 0x16;
        public const uint BF_FLAT = 0x4000;
        public const uint BF_LEFT = 1;
        public const uint BF_MIDDLE = 0x800;
        public const uint BF_MONO = 0x8000;
        public const uint BF_RECT = 15;
        public const uint BF_RIGHT = 4;
        public const uint BF_SOFT = 0x1000;
        public const uint BF_TOP = 2;
        public const uint BF_TOPLEFT = 3;
        public const uint BF_TOPRIGHT = 6;

        public const byte AC_SRC_ALPHA = 1;
        public const byte AC_SRC_OVER = 0;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class SIZE
        {
            public int cx;
            public int cy;
            public SIZE()
            {
            }

            public SIZE(int cx, int cy)
            {
                this.cx = cx;
                this.cy = cy;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public RECT(RECT rcSrc)
            {
                this.left = rcSrc.left;
                this.top = rcSrc.top;
                this.right = rcSrc.right;
                this.bottom = rcSrc.bottom;
            }

            public int Width
            {
                get
                {
                    return (this.right - this.left);
                }
                set
                {
                    this.right = this.left + value;
                }
            }
            public int Height
            {
                get
                {
                    return (this.bottom - this.top);
                }
                set
                {
                    this.bottom = this.top + value;
                }
            }
            public static RECT FromXYWH(int x, int y, int width, int height)
            {
                return new RECT(x, y, x + width, y + height);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class BITMAPINFO
        {
            public Int32 biSize;
            public Int32 biWidth;
            public Int32 biHeight;
            public Int16 biPlanes;
            public Int16 biBitCount;
            public Int32 biCompression;
            public Int32 biSizeImage;
            public Int32 biXPelsPerMeter;
            public Int32 biYPelsPerMeter;
            public Int32 biClrUsed;
            public Int32 biClrImportant;
            public Int32 colors;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PAINTSTRUCT
        {
            public IntPtr hdc;
            public bool fErase;
            public RECT rcPaint;
            public bool fRestore;
            public bool fIncUpdate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] rgbReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAP
        {
            public int bmType;
            public int bmWidth;
            public int bmHeight;
            public int bmWidthBytes;
            public ushort bmPlanes;
            public ushort bmBitsPixel;
            public int bmBits;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WNDCLASS
        {
            public uint style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszClassName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr HDC);

        /// <summary>
        /// The CreateBitmap function creates a bitmap with the specified width, height, and color format (color planes and bits-per-pixel).
        /// 
        /// The CreateBitmap function creates a device-dependent bitmap. 
        /// After a bitmap is created, it can be selected into a device context by calling the SelectObject function. 
        /// However, the bitmap can only be selected into a device context if the bitmap and the DC have the same format. 
        /// The CreateBitmap function can be used to create color bitmaps. 
        /// However, for performance reasons applications should use CreateBitmap to create monochrome bitmaps and CreateCompatibleBitmap 
        /// to create color bitmaps. 
        /// Whenever a color bitmap returned from CreateBitmap is selected into a device context, 
        /// the system checks that the bitmap matches the format of the device context it is being selected into. 
        /// Because CreateCompatibleBitmap takes a device context, it returns a bitmap that has the same format as the specified device context. 
        /// Thus, subsequent calls to SelectObject are faster with a color bitmap from 
        /// CreateCompatibleBitmap than with a color bitmap returned from CreateBitmap. If the bitmap is monochrome, 
        /// zeros represent the foreground color and ones represent the background color for the destination device context. 
        /// If an application sets the nWidth or nHeight parameters to zero, CreateBitmap returns the handle to a 1-by-1 pixel, monochrome bitmap. 
        /// When you no longer need the bitmap, call the DeleteObject function to delete it.
        /// </summary>
        /// <param name="Width">nWidth [in] The bitmap width, in pixels.</param>
        /// <param name="Height">nHeight [in] The bitmap height, in pixels.</param>
        /// <param name="Planes">cPlanes [in] The number of color planes used by the device.</param>
        /// <param name="BitCount">cBitsPerPel [in] The number of bits required to identify the color of a single pixel.</param>
        /// <param name="Bits">lpvBits [in] A pointer to an array of color data used to set the colors in a rectangle of pixels. 
        /// Each scan line in the rectangle must be word aligned (scan lines that are not word aligned must be padded with zeros). 
        /// If this parameter is NULL, the contents of the new bitmap is undefined.</param>
        /// <returns>If the function succeeds, the return value is a handle to a bitmap. 
        /// If the function fails, the return value is NULL.
        /// This function can return the following value.
        /// ERROR_INVALID_BITMAP - The calculated size of the bitmap is less than zero.</returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr CreateBitmap(int Width, int Height, int Planes, int BitCount, IntPtr Bits);

        /// <summary>
        /// The CreateCompatibleBitmap function creates a bitmap compatible with the device that is associated with the specified device context.
        /// 
        /// Remarks:
        /// The color format of the bitmap created by the CreateCompatibleBitmap function matches the color format of the device identified by 
        /// the hdc parameter. 
        /// This bitmap can be selected into any memory device context that is compatible with the original device.
        /// Because memory device contexts allow both color and monochrome bitmaps, 
        /// the format of the bitmap returned by the CreateCompatibleBitmap function differs when the specified device context is a memory device context. 
        /// However, a compatible bitmap that was created for a nonmemory device context always possesses the same color format 
        /// and uses the same color palette as the specified device context. Note: When a memory device context is created, 
        /// it initially has a 1-by-1 monochrome bitmap selected into it. If this memory device context is used in CreateCompatibleBitmap, 
        /// the bitmap that is created is a monochrome bitmap. To create a color bitmap, 
        /// use the HDC that was used to create the memory device context, as shown in the following code:
        /// HDC memDC = CreateCompatibleDC ( hDC ); HBITMAP memBM = CreateCompatibleBitmap ( hDC, nWidth, nHeight );SelectObject ( memDC, memBM );
        /// If an application sets the nWidth or nHeight parameters to zero, CreateCompatibleBitmap returns the handle to a 1-by-1 pixel, monochrome bitmap. 
        /// If a DIB section, which is a bitmap created by the CreateDIBSection function, 
        /// is selected into the device context identified by the hdc parameter, CreateCompatibleBitmap creates a DIB section. 
        /// When you no longer need the bitmap, call the DeleteObject function to delete it.
        /// </summary>
        /// <param name="HDC">hdc [in] A handle to a device context.</param>
        /// <param name="Width">nWidth [in] The bitmap width, in pixels.</param>
        /// <param name="Height">nHeight [in] The bitmap height, in pixels.</param>
        /// <returns>Return Value If the function succeeds, the return value is a handle to the compatible bitmap (DDB). 
        /// If the function fails, the return value is NULL.</returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr HDC, int Width, int Height);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr CreatePen(int Style, int Width, uint color);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool DeleteDC(IntPtr HDC);

        /// <summary>
        /// The DeleteObject function deletes a logical pen, brush, font, bitmap, region, or palette, 
        /// freeing all system resources associated with the object. After the object is deleted, the specified handle is no longer valid.
        /// 
        /// Do not delete a drawing object (pen or brush) while it is still selected into a DC.
        /// When a pattern brush is deleted, the bitmap associated with the brush is not deleted. The bitmap must be deleted independently.
        /// </summary>
        /// <param name="p1">hObject [in] A handle to a logical pen, brush, font, bitmap, region, or palette.</param>
        /// <returns>If the function succeeds, the return value is nonzero.
        /// If the specified handle is not valid or is currently selected into a DC, the return value is zero.</returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool DeleteObject(IntPtr p1);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr SelectObject(IntPtr HDC, IntPtr p2);

        public enum StockObjects
        {
            WHITE_BRUSH = 0,
            LTGRAY_BRUSH = 1,
            GRAY_BRUSH = 2,
            DKGRAY_BRUSH = 3,
            BLACK_BRUSH = 4,
            NULL_BRUSH = 5,
            HOLLOW_BRUSH = NULL_BRUSH,
            WHITE_PEN = 6,
            BLACK_PEN = 7,
            NULL_PEN = 8,
            OEM_FIXED_FONT = 10,
            ANSI_FIXED_FONT = 11,
            ANSI_VAR_FONT = 12,
            SYSTEM_FONT = 13,
            DEVICE_DEFAULT_FONT = 14,
            DEFAULT_PALETTE = 15,
            SYSTEM_FIXED_FONT = 16,
            DEFAULT_GUI_FONT = 17,
            DC_BRUSH = 18,
            DC_PEN = 19,
        }

        /// <summary>
        /// The GetStockObject function retrieves a handle to one of the stock pens, brushes, fonts, or palettes.
        /// It is not necessary (but it is not harmful) to delete stock objects by calling DeleteObject.
        /// </summary>
        /// <param name="fnObject"></param>
        /// <returns></returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr GetStockObject(StockObjects fnObject);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool LineTo(IntPtr DC, int X, int Y);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool Rectangle(IntPtr DC, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern uint GetDCPenColor(IntPtr hdc);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern uint SetDCPenColor(IntPtr DC, uint crColor);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool StretchBlt(IntPtr DestDC, int X, int Y, int Width, int Height, IntPtr SrcDC, int XSrc, int YSrc, int SrcWidth, int SrcHeight, uint Rop);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr CreateDIBSection(IntPtr hdc, [In] ref BITMAPINFO pbmi,
           uint pila, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);

        [DllImport("gdi32.dll", ExactSpelling = true)]
        public static extern int SetTextColor(IntPtr hdc, int crColor);

        [DllImport("gdi32.dll", ExactSpelling = true)]
        public static extern int SetBkColor(IntPtr hdc, int crColor);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool TextOutW(IntPtr hdc, int nXStart, int nYStart, string lpString, int cbString);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool TextOutW(IntPtr hdc, int nXStart, int nYStart, StringBuilder lpString, int cbString);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int DrawTextW(IntPtr hDC, string lpString, int nCount, ref RECT lpRect, uint uFormat);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr CreateFontW(int nHeight, int nWidth, int nEscapement,
           int nOrientation, int fnWeight, uint fdwItalic, uint fdwUnderline, uint
           fdwStrikeOut, uint fdwCharSet, uint fdwOutputPrecision, uint
           fdwClipPrecision, uint fdwQuality, uint fdwPitchAndFamily, string lpszFace);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr CreateSolidBrush(uint crColor);

        [DllImport("gdi32.dll", ExactSpelling = true)]
        public static extern IntPtr CreateHatchBrush(int fnStyle, uint clrref);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int SetBkMode(IntPtr hdc, BkMode iBkMode);

        public enum BkMode
        {
            TRANSPARENT = 1,
            OPAQUE = 2
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool MoveToEx(IntPtr hdc, int X, int Y, IntPtr lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int IntersectClipRect(IntPtr hdc, int nLeftRect, int nTopRect,
           int nRightRect, int nBottomRect);

        /// <summary>
        /// Выбор ClipRgn. Если hrgn = IntPtr.Zero, то это сброс всех клипов.
        /// </summary>
        /// <param name="hdc"></param>
        /// <param name="hrgn"></param>
        /// <returns></returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int SelectClipRgn(IntPtr hdc, IntPtr hrgn);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr GetCurrentObject(IntPtr hdc, uint uiObjectType);

        /// <summary>
        /// Создание прямоугольного региона (RectRgn). После создания его можно выбрать через SelectClipRgn, и он будет как ClipRegion у указанной DC.
        /// После выбора в DC, объект RectRgn можно удалить по DeleteObject. Удалить потом надо обязательно. DC можно использовать и после удаления RectRgn.
        /// </summary>
        /// <param name="nLeftRect"></param>
        /// <param name="nTopRect"></param>
        /// <param name="nRightRect"></param>
        /// <param name="nBottomRect"></param>
        /// <returns></returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("Msimg32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool TransparentBlt(IntPtr hdcDest, int xoriginDest, int yoriginDest, int wDest, int hDest, IntPtr hdcSrc, int xoriginSrc, int yoriginSrc, int wSrc, int hSrc, uint crTransparent);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr BeginPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool GetClientRect(IntPtr hWnd, IntPtr lpRect);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int Chord(IntPtr hDC, int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4);

        /// <summary>
        /// The Polygon function draws a polygon consisting of two or more vertices connected by straight lines. 
        /// The polygon is outlined by using the current pen and filled by using the current brush and polygon fill mode.
        /// The polygon is closed automatically by drawing a line from the last vertex to the first.
        /// The current position is neither used nor updated by the Polygon function.
        /// Any extra points are ignored. 
        /// To draw a line with more points, divide your data into groups, each of which have less than the maximum number of points, 
        /// and call the function for each group of points. Remember to connect the line segments.
        /// </summary>
        /// <param name="hDC">hdc [in] A handle to the device context.</param>
        /// <param name="lpPoint">lpPoints [in] A pointer to an array of POINT structures that specify the vertices of the polygon, in logical coordinates.</param>
        /// <param name="nCount">nCount [in] The number of vertices in the array. This value must be greater than or equal to 2.</param>
        /// <returns>Return Value
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero.</returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int Polygon(IntPtr hDC, ref POINT lpPoint, int nCount);

        /// <summary>
        /// The PolyPolygon function draws a series of closed polygons. 
        /// Each polygon is outlined by using the current pen and filled by using the current brush and polygon fill mode. 
        /// The polygons drawn by this function can overlap.
        /// The current position is neither used nor updated by this function.
        /// Any extra points are ignored. 
        /// To draw the polygons with more points, divide your data into groups, each of which have less than the maximum number of points, 
        /// and call the function for each group of points. Note, it is best to have a polygon in only one of the groups.
        /// </summary>
        /// <param name="hdc">hdc [in]  A handle to the device context.</param>
        /// <param name="lpPoint">lpPoints [in] A pointer to an array of POINT structures that define the vertices of the polygons, in logical coordinates. 
        /// The polygons are specified consecutively. Each polygon is closed automatically by drawing a line from the last vertex to the first. 
        /// Each vertex should be specified once.</param>
        /// <param name="lpPolyCounts">pPolyCounts [in] A pointer to an array of integers, each of which specifies the number of points 
        /// in the corresponding polygon. Each integer must be greater than or equal to 2.</param>
        /// <param name="nCount">nCount [in] The total number of polygons.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.</returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int PolyPolygon(IntPtr hdc, ref POINT lpPoint, ref int lpPolyCounts, int nCount);

        /// <summary>
        /// The GetROP2 function retrieves the foreground mix mode of the specified device context. 
        /// The mix mode specifies how the pen or interior color and the color already on the screen are combined to yield a new color.
        /// </summary>
        /// <param name="hDC">hdc [in] Handle to the device context.</param>
        /// <returns>If the function succeeds, the return value specifies the foreground mix mode. (Параметры начинающиеся с R2 в списке констант.)
        /// If the function fails, the return value is zero.</returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetROP2(IntPtr hDC);

        /// <summary>
        /// The SetROP2 function sets the current foreground mix mode. 
        /// GDI uses the foreground mix mode to combine pens and interiors of filled objects with the colors already on the screen. 
        /// The foreground mix mode defines how colors from the brush or pen and the colors in the existing image are to be combined.
        /// Mix modes define how GDI combines source and destination colors when drawing with the current pen. 
        /// The mix modes are binary raster operation codes, representing all possible Boolean functions of two variables, 
        /// using the binary operations AND, OR, and XOR (exclusive OR), and the unary operation NOT. 
        /// The mix mode is for raster devices only; it is not available for vector devices.
        /// </summary>
        /// <param name="hDC">hdc [in] A handle to the device context.</param>
        /// <param name="nDrawMode">fnDrawMode [in] The mix mode. (Параметры начинающиеся с R2 в списке констант.)</param>
        /// <returns>If the function succeeds, the return value specifies the previous mix mode.
        /// If the function fails, the return value is zero.</returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int SetROP2(IntPtr hDC, int nDrawMode);

        /// <summary>
        /// The SetPolyFillMode function sets the polygon fill mode for functions that fill polygons.
        /// In general, the modes differ only in cases where a complex, overlapping polygon must be filled 
        /// (for example, a five-sided polygon that forms a five-pointed star with a pentagon in the center). 
        /// In such cases, ALTERNATE mode fills every other enclosed region within the polygon (that is, the points of the star), 
        /// but WINDING mode fills all regions (that is, the points and the pentagon).
        /// When the fill mode is ALTERNATE, GDI fills the area between odd-numbered and even-numbered polygon sides on each scan line. 
        /// That is, GDI fills the area between the first and second side, between the third and fourth side, and so on.
        /// When the fill mode is WINDING, GDI fills any region that has a nonzero winding value. 
        /// This value is defined as the number of times a pen used to draw the polygon would go around the region. 
        /// The direction of each edge of the polygon is important.
        /// </summary>
        /// <param name="hdc">hdc [in] A handle to the device context.</param>
        /// <param name="nPolyFillMode">iPolyFillMode [in] The new fill mode. This parameter can be one of the following values:
        /// ALTERNATE - Selects alternate mode (fills the area between odd-numbered and even-numbered polygon sides on each scan line).
        /// WINDING	- Selects winding mode (fills any region with a nonzero winding value).</param>
        /// <returns>The return value specifies the previous filling mode. If an error occurs, the return value is zero.</returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int SetPolyFillMode(IntPtr hdc, int nPolyFillMode);

        /// <summary>
        /// The GetPolyFillMode function retrieves the current polygon fill mode.
        /// The current position is neither used nor updated by Ellipse.
        /// </summary>
        /// <param name="hdc">hdc [in] Handle to the device context.</param>
        /// <returns>If the function succeeds, the return value specifies the polygon fill mode, which can be one of the following values.
        /// ALTERNATE - Selects alternate mode (fills area between odd-numbered and even-numbered polygon sides on each scan line).
        /// WINDING - Selects winding mode (fills any region with a nonzero winding value).
        /// If an error occurs, the return value is zero.
        /// </returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetPolyFillMode(IntPtr hdc);

        /// <summary>
        /// The Ellipse function draws an ellipse. The center of the ellipse is the center of the specified bounding rectangle. 
        /// The ellipse is outlined by using the current pen and is filled by using the current brush.
        /// </summary>
        /// <param name="hdc">hdc [in] A handle to the device context.</param>
        /// <param name="nLeftRect">nLeftRect [in] The x-coordinate, in logical coordinates, of the upper-left corner of the bounding rectangle.</param>
        /// <param name="nTopRect">nTopRect [in] The y-coordinate, in logical coordinates, of the upper-left corner of the bounding rectangle.</param>
        /// <param name="nRightRect">nRightRect [in] The x-coordinate, in logical coordinates, of the lower-right corner of the bounding rectangle.</param>
        /// <param name="nBottomRect">nBottomRect [in] The y-coordinate, in logical coordinates, of the lower-right corner of the bounding rectangle.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.</returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public extern static int Ellipse(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern bool GetUpdateRect(IntPtr hWnd, ref WinApi.RECT lpRect, bool bErase);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern bool DrawEdge(IntPtr hdc, ref WinApi.RECT qrc, uint edge, uint grfFlags);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern void PostQuitMessage(int nExitCode);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize,
            IntPtr hdcSrc, ref POINT pptSrc, int crKey, ref BLENDFUNCTION pblend, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern UInt16 RegisterClassW([In] ref WNDCLASS lpWndClass);

        [DllImport("user32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr CreateWindowExW(
           UInt32 dwExStyle,
           [MarshalAs(UnmanagedType.LPWStr)]
           string lpClassName,
           [MarshalAs(UnmanagedType.LPWStr)]
           string lpWindowName,
           Int32 dwStyle,
           Int32 x,
           Int32 y,
           Int32 nWidth,
           Int32 nHeight,
           IntPtr hWndParent,
           IntPtr hMenu,
           IntPtr hInstance,
           IntPtr lpParam
        );

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }


        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        /*delegate to handle EnumChildWindows*/
        public delegate bool EnumProc(IntPtr hWnd, ref IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int EnumChildWindows(IntPtr hWndParent, EnumProc lpEnumFunc, ref IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern IntPtr GetWindowTextW(IntPtr hWnd, StringBuilder lpString, IntPtr nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int GetClassNameW(IntPtr hwnd, StringBuilder lpClassName, IntPtr nMaxCount);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int SetDCBrushColor(IntPtr hdc, int color);
    }
}
