using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;

namespace SpectrumArchiveReader
{
    public class Driver
    {
        public const uint FILE_DEVICE_UNKNOWN = 0x00000022;
        public const uint FILE_READ_DATA = 0x1;
        public const uint FILE_WRITE_DATA = 0x2;
        public const uint METHOD_BUFFERED = 0;
        public const uint METHOD_OUT_DIRECT = 2;
        public const uint METHOD_IN_DIRECT = 1;


        // If you're not using C/C++, use the public uint IOCTL values below
        public static uint IOCTL_FDRAWCMD_GET_VERSION = FD_CTL_CODE(0x888, METHOD_BUFFERED);           // 0x0022e220

        public static uint IOCTL_FDCMD_READ_TRACK = FD_CTL_CODE(0x802, METHOD_OUT_DIRECT);             // 0x0022e00a
        public static uint IOCTL_FDCMD_SPECIFY = FD_CTL_CODE(0x803, METHOD_BUFFERED);                  // 0x0022e00c
        public static uint IOCTL_FDCMD_SENSE_DRIVE_STATUS = FD_CTL_CODE(0x804, METHOD_BUFFERED);       // 0x0022e010
        public static uint IOCTL_FDCMD_WRITE_DATA = FD_CTL_CODE(0x805, METHOD_IN_DIRECT);              // 0x0022e015
        public static uint IOCTL_FDCMD_READ_DATA = FD_CTL_CODE(0x806, METHOD_OUT_DIRECT);              // 0x0022e01a
        public static uint IOCTL_FDCMD_RECALIBRATE = FD_CTL_CODE(0x807, METHOD_BUFFERED);              // 0x0022e01c
        public static uint IOCTL_FDCMD_SENSE_INT_STATUS = FD_CTL_CODE(0x808, METHOD_BUFFERED);         // 0x0022e020   // added in 1.0.0.22
        public static uint IOCTL_FDCMD_WRITE_DELETED_DATA = FD_CTL_CODE(0x809, METHOD_IN_DIRECT);      // 0x0022e025
        public static uint IOCTL_FDCMD_READ_ID = FD_CTL_CODE(0x80a, METHOD_BUFFERED);                  // 0x0022e028
        public static uint IOCTL_FDCMD_READ_DELETED_DATA = FD_CTL_CODE(0x80c, METHOD_OUT_DIRECT);      // 0x0022e032
        public static uint IOCTL_FDCMD_FORMAT_TRACK = FD_CTL_CODE(0x80d, METHOD_BUFFERED);             // 0x0022e034
        public static uint IOCTL_FDCMD_DUMPREG = FD_CTL_CODE(0x80e, METHOD_BUFFERED);                  // 0x0022e038
        public static uint IOCTL_FDCMD_SEEK = FD_CTL_CODE(0x80f, METHOD_BUFFERED);                     // 0x0022e03c
        public static uint IOCTL_FDCMD_VERSION = FD_CTL_CODE(0x810, METHOD_BUFFERED);                  // 0x0022e040
        public static uint IOCTL_FDCMD_SCAN_EQUAL = FD_CTL_CODE(0x811, METHOD_IN_DIRECT);              // 0x0022e045   (not implemented yet);
        public static uint IOCTL_FDCMD_PERPENDICULAR_MODE = FD_CTL_CODE(0x812, METHOD_BUFFERED);       // 0x0022e048
        public static uint IOCTL_FDCMD_CONFIGURE = FD_CTL_CODE(0x813, METHOD_BUFFERED);                // 0x0022e04c
        public static uint IOCTL_FDCMD_LOCK = FD_CTL_CODE(0x814, METHOD_BUFFERED);                     // 0x0022e050
        public static uint IOCTL_FDCMD_VERIFY = FD_CTL_CODE(0x816, METHOD_BUFFERED);                   // 0x0022e058
        public static uint IOCTL_FDCMD_POWERDOWN_MODE = FD_CTL_CODE(0x817, METHOD_BUFFERED);           // 0x0022e05c   (not implemented yet);
        public static uint IOCTL_FDCMD_PART_ID = FD_CTL_CODE(0x818, METHOD_BUFFERED);                  // 0x0022e060
        public static uint IOCTL_FDCMD_SCAN_LOW_OR_EQUAL = FD_CTL_CODE(0x819, METHOD_IN_DIRECT);       // 0x0022e065   (not implemented yet);
        public static uint IOCTL_FDCMD_SCAN_HIGH_OR_EQUAL = FD_CTL_CODE(0x81d, METHOD_IN_DIRECT);      // 0x0022e075   (not implemented yet);
        public static uint IOCTL_FDCMD_SAVE = FD_CTL_CODE(0x82e, METHOD_BUFFERED);                     // 0x0022e0b8   (not implemented yet);
        public static uint IOCTL_FDCMD_OPTION = FD_CTL_CODE(0x833, METHOD_BUFFERED);                   // 0x0022e0cc   (not implemented yet);
        public static uint IOCTL_FDCMD_RESTORE = FD_CTL_CODE(0x84e, METHOD_BUFFERED);                  // 0x0022e138   (not implemented yet);
        public static uint IOCTL_FDCMD_DRIVE_SPEC_CMD = FD_CTL_CODE(0x88e, METHOD_BUFFERED);           // 0x0022e238   (not implemented yet);
        public static uint IOCTL_FDCMD_RELATIVE_SEEK = FD_CTL_CODE(0x88f, METHOD_BUFFERED);            // 0x0022e23c
        public static uint IOCTL_FDCMD_FORMAT_AND_WRITE = FD_CTL_CODE(0x8ef, METHOD_BUFFERED);         // 0x0022e3bc   // added in 1.0.1.10

        public static uint IOCTL_FD_SCAN_TRACK = FD_CTL_CODE(0x900, METHOD_BUFFERED);                  // 0x0022e400
        public static uint IOCTL_FD_GET_RESULT = FD_CTL_CODE(0x901, METHOD_BUFFERED);                  // 0x0022e404
        public static uint IOCTL_FD_RESET = FD_CTL_CODE(0x902, METHOD_BUFFERED);                       // 0x0022e408
        public static uint IOCTL_FD_SET_MOTOR_TIMEOUT = FD_CTL_CODE(0x903, METHOD_BUFFERED);           // 0x0022e40c
        public static uint IOCTL_FD_SET_DATA_RATE = FD_CTL_CODE(0x904, METHOD_BUFFERED);               // 0x0022e410
        public static uint IOCTL_FD_GET_FDC_INFO = FD_CTL_CODE(0x905, METHOD_BUFFERED);                // 0x0022e414
        public static uint IOCTL_FD_GET_REMAIN_COUNT = FD_CTL_CODE(0x906, METHOD_BUFFERED);            // 0x0022e418   // added in 1.0.0.22
        public static uint IOCTL_FD_SET_DISK_CHECK = FD_CTL_CODE(0x908, METHOD_BUFFERED);              // 0x0022e420
        public static uint IOCTL_FD_SET_SHORT_WRITE = FD_CTL_CODE(0x909, METHOD_BUFFERED);             // 0x0022e424   // added in 1.0.0.22
        public static uint IOCTL_FD_SET_SECTOR_OFFSET = FD_CTL_CODE(0x90a, METHOD_BUFFERED);           // 0x0022e428   // added in 1.0.0.22
        public static uint IOCTL_FD_SET_HEAD_SETTLE_TIME = FD_CTL_CODE(0x90b, METHOD_BUFFERED);        // 0x0022e42c   // added in 1.0.0.22
        public static uint IOCTL_FD_LOCK_FDC = FD_CTL_CODE(0x910, METHOD_BUFFERED);                    // 0x0022e440   // obsolete from 1.0.1.0
        public static uint IOCTL_FD_UNLOCK_FDC = FD_CTL_CODE(0x911, METHOD_BUFFERED);                  // 0x0022e444   // obsolete from 1.0.1.0
        public static uint IOCTL_FD_MOTOR_ON = FD_CTL_CODE(0x912, METHOD_BUFFERED);                    // 0x0022e448
        public static uint IOCTL_FD_MOTOR_OFF = FD_CTL_CODE(0x913, METHOD_BUFFERED);                   // 0x0022e44c
        public static uint IOCTL_FD_WAIT_INDEX = FD_CTL_CODE(0x914, METHOD_BUFFERED);                  // 0x0022e450   // added in 1.0.0.22
        public static uint IOCTL_FD_TIMED_SCAN_TRACK = FD_CTL_CODE(0x915, METHOD_BUFFERED);            // 0x0022e454   // added in 1.0.0.22
        public static uint IOCTL_FD_RAW_READ_TRACK = FD_CTL_CODE(0x916, METHOD_OUT_DIRECT);            // 0x0022e45a   // added in 1.0.1.4
        public static uint IOCTL_FD_CHECK_DISK = FD_CTL_CODE(0x917, METHOD_BUFFERED);                  // 0x0022e45c   // added in 1.0.1.10
        public static uint IOCTL_FD_GET_TRACK_TIME = FD_CTL_CODE(0x918, METHOD_BUFFERED);              // 0x0022e460   // added in 1.0.1.10


        // Command flags: multi-track, MFM, sector skip, relative seek direction, verify enable count
        public const byte FD_OPTION_MT = 0x80;
        public const byte FD_OPTION_MFM = 0x40;
        public const byte FD_OPTION_SK = 0x20;
        public const byte FD_OPTION_DIR = 0x40;
        public const byte FD_OPTION_EC = 0x01;
        public const byte FD_OPTION_FM = 0x00;
        public const byte FD_ENCODING_MASK = FD_OPTION_MFM;

        // Controller data rates, for use with IOCTL_FD_SET_DATA_RATE
        public const byte FD_RATE_MASK = 3;
        public const byte FD_RATE_500K = 0;
        public const byte FD_RATE_300K = 1;
        public const byte FD_RATE_250K = 2;
        public const byte FD_RATE_1M = 3;

        // FD_FDC_INFO controller types      
        public const byte FDC_TYPE_UNKNOWN = 0;
        public const byte FDC_TYPE_UNKNOWN2 = 1;
        public const byte FDC_TYPE_NORMAL = 2;
        public const byte FDC_TYPE_ENHANCED = 3;
        public const byte FDC_TYPE_82077 = 4;
        public const byte FDC_TYPE_82077AA = 5;
        public const byte FDC_TYPE_82078_44 = 6;
        public const byte FDC_TYPE_82078_64 = 7;
        public const byte FDC_TYPE_NATIONAL = 8;

        // Bits representing supported data rates, for the FD_FDC_INFO structure below
        public const byte FDC_SPEED_250K = 0x01;
        public const byte FDC_SPEED_300K = 0x02;
        public const byte FDC_SPEED_500K = 0x04;
        public const byte FDC_SPEED_1M = 0x08;
        public const byte FDC_SPEED_2M = 0x10;


        // WinAPI constants:

        public const int INVALID_HANDLE_VALUE = -1;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string filename,
            [MarshalAs(UnmanagedType.U4)] EFileAccess access,
            [MarshalAs(UnmanagedType.U4)] EFileShare share,
            IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
            [MarshalAs(UnmanagedType.U4)] ECreationDisposition creationDisposition,
            [MarshalAs(UnmanagedType.U4)] EFileAttributes flagsAndAttributes,
            IntPtr templateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateFileW(
             [MarshalAs(UnmanagedType.LPWStr)] string filename,
             [MarshalAs(UnmanagedType.U4)] EFileAccess access,
             [MarshalAs(UnmanagedType.U4)] EFileShare share,
             IntPtr securityAttributes,
             [MarshalAs(UnmanagedType.U4)] ECreationDisposition creationDisposition,
             [MarshalAs(UnmanagedType.U4)] EFileAttributes flagsAndAttributes,
             IntPtr templateFile);

        [Flags]
        public enum EFileAccess : uint
        {
            //
            // Standart Section
            //

            AccessSystemSecurity = 0x1000000,   // AccessSystemAcl access type
            MaximumAllowed = 0x2000000,         // MaximumAllowed access type

            Delete = 0x10000,
            ReadControl = 0x20000,
            WriteDAC = 0x40000,
            WriteOwner = 0x80000,
            Synchronize = 0x100000,

            StandardRightsRequired = 0xF0000,
            StandardRightsRead = ReadControl,
            StandardRightsWrite = ReadControl,
            StandardRightsExecute = ReadControl,
            StandardRightsAll = 0x1F0000,
            SpecificRightsAll = 0xFFFF,

            FILE_READ_DATA = 0x0001,            // file & pipe
            FILE_LIST_DIRECTORY = 0x0001,       // directory
            FILE_WRITE_DATA = 0x0002,           // file & pipe
            FILE_ADD_FILE = 0x0002,             // directory
            FILE_APPEND_DATA = 0x0004,          // file
            FILE_ADD_SUBDIRECTORY = 0x0004,     // directory
            FILE_CREATE_PIPE_INSTANCE = 0x0004, // named pipe
            FILE_READ_EA = 0x0008,              // file & directory
            FILE_WRITE_EA = 0x0010,             // file & directory
            FILE_EXECUTE = 0x0020,              // file
            FILE_TRAVERSE = 0x0020,             // directory
            FILE_DELETE_CHILD = 0x0040,         // directory
            FILE_READ_ATTRIBUTES = 0x0080,      // all
            FILE_WRITE_ATTRIBUTES = 0x0100,     // all

            //
            // Generic Section
            //

            GenericRead = 0x80000000,
            GenericWrite = 0x40000000,
            GenericExecute = 0x20000000,
            GenericAll = 0x10000000,

            SPECIFIC_RIGHTS_ALL = 0x00FFFF,
            FILE_ALL_ACCESS =
            StandardRightsRequired |
            Synchronize |
            0x1FF,

            FILE_GENERIC_READ =
                StandardRightsRead |
                FILE_READ_DATA |
                FILE_READ_ATTRIBUTES |
                FILE_READ_EA |
                Synchronize,

            FILE_GENERIC_WRITE =
                StandardRightsWrite |
                FILE_WRITE_DATA |
                FILE_WRITE_ATTRIBUTES |
                FILE_WRITE_EA |
                FILE_APPEND_DATA |
                Synchronize,

            FILE_GENERIC_EXECUTE =
                StandardRightsExecute |
                FILE_READ_ATTRIBUTES |
                FILE_EXECUTE |
                Synchronize
        }

        [Flags]
        public enum EFileShare : uint
        {
            /// <summary>
            ///
            /// </summary>
            None = 0x00000000,
            /// <summary>
            /// Enables subsequent open operations on an object to request read access.
            /// Otherwise, other processes cannot open the object if they request read access.
            /// If this flag is not specified, but the object has been opened for read access, the function fails.
            /// </summary>
            Read = 0x00000001,
            /// <summary>
            /// Enables subsequent open operations on an object to request write access.
            /// Otherwise, other processes cannot open the object if they request write access.
            /// If this flag is not specified, but the object has been opened for write access, the function fails.
            /// </summary>
            Write = 0x00000002,
            /// <summary>
            /// Enables subsequent open operations on an object to request delete access.
            /// Otherwise, other processes cannot open the object if they request delete access.
            /// If this flag is not specified, but the object has been opened for delete access, the function fails.
            /// </summary>
            Delete = 0x00000004
        }

        public enum ECreationDisposition : uint
        {
            /// <summary>
            /// Creates a new file. The function fails if a specified file exists.
            /// </summary>
            New = 1,
            /// <summary>
            /// Creates a new file, always.
            /// If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file attributes,
            /// and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES structure specifies.
            /// </summary>
            CreateAlways = 2,
            /// <summary>
            /// Opens a file. The function fails if the file does not exist.
            /// </summary>
            OpenExisting = 3,
            /// <summary>
            /// Opens a file, always.
            /// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
            /// </summary>
            OpenAlways = 4,
            /// <summary>
            /// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
            /// The calling process must open the file with the GENERIC_WRITE access right.
            /// </summary>
            TruncateExisting = 5
        }

        [Flags]
        public enum EFileAttributes : uint
        {
            Readonly = 0x00000001,
            Hidden = 0x00000002,
            System = 0x00000004,
            Directory = 0x00000010,
            Archive = 0x00000020,
            Device = 0x00000040,
            Normal = 0x00000080,
            Temporary = 0x00000100,
            SparseFile = 0x00000200,
            ReparsePoint = 0x00000400,
            Compressed = 0x00000800,
            Offline = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted = 0x00004000,
            Write_Through = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x08000000,
            DeleteOnClose = 0x04000000,
            BackupSemantics = 0x02000000,
            PosixSemantics = 0x01000000,
            OpenReparsePoint = 0x00200000,
            OpenNoRecall = 0x00100000,
            FirstPipeInstance = 0x00080000
        }

        public static uint CTL_CODE(uint deviceType, uint function, uint method, uint access)
        {
            return ((deviceType) << 16) | ((access) << 14) | ((function) << 2) | (method);
        }

        public static uint FD_CTL_CODE(uint i, uint m)
        {
            return CTL_CODE(FILE_DEVICE_UNKNOWN, i, m, FILE_READ_DATA | FILE_WRITE_DATA);
        }

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode,
            IntPtr lpInBuffer, uint nInBufferSize,
            IntPtr lpOutBuffer, uint nOutBufferSize,
            IntPtr lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode,
            IntPtr lpInBuffer, uint nInBufferSize,
            IntPtr lpOutBuffer, uint nOutBufferSize,
            out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr InBuffer,
            uint nInBufferSize,
            ref long OutBuffer,
            int nOutBufferSize,
            ref int pBytesReturned,
            [In] ref NativeOverlapped lpOverlapped);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            ref int InBuffer,
            int nInBufferSize,
            out int OutBuffer,
            int nOutBufferSize,
            out int pBytesReturned,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, AllocationType lAllocationType, MemoryProtection flProtect);

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        [DllImport("kernel32", ExactSpelling = true)]
        public static extern bool VirtualFree(IntPtr lpAddress, uint dwSize, AllocationType dwFreeType);


        public static int GetVersion()
        {
            int inBuffer = 0;
            int dwVersion;
            int dwRead;
            IntPtr h = CreateFile("\\\\.\\fdrawcmd", EFileAccess.GenericRead | EFileAccess.GenericWrite, 0, IntPtr.Zero, ECreationDisposition.OpenExisting, 0, IntPtr.Zero);
            if ((int)h == INVALID_HANDLE_VALUE) return -1;
            bool result = DeviceIoControl(h, IOCTL_FDRAWCMD_GET_VERSION, ref inBuffer, 0, out dwVersion, 4, out dwRead, IntPtr.Zero);
            CloseHandle(h);
            return result ? dwVersion : 0;
        }

        public static string GetVersionStr()
        {
            int version = GetVersion();
            if (version == -1) return "Driver is not accessible";
            int v0 = (int)((version & 0xFF000000) >> 24);
            int v1 = (version & 0x00FF0000) >> 16;
            int v2 = (version & 0x0000FF00) >> 8;
            int v3 = version & 0x000000FF;
            return $"{v0}.{v1}.{v2}.{v3}";
        }

        public unsafe static bool GetFdcInfo(IntPtr driverHandle, out tagFD_FDC_INFO info)
        {
            tagFD_FDC_INFO infox;
            int inBuffer;
            uint dwRead;
            bool r = DeviceIoControl(driverHandle, IOCTL_FD_GET_FDC_INFO, (IntPtr)(&inBuffer), 0, (IntPtr)(&infox), (uint)sizeof(tagFD_FDC_INFO), out dwRead, IntPtr.Zero);
            info = infox;
            return r;
        }

        public static unsafe bool GetTrackTime(IntPtr driverHandle, out int time)
        {
            uint dwRead;
            int time0 = 200000;
            bool r = DeviceIoControl(driverHandle, IOCTL_FD_GET_TRACK_TIME, IntPtr.Zero, 0, (IntPtr)(&time0), sizeof(int), out dwRead, IntPtr.Zero);
            time = time0;
            return r;
        }

        public static bool MotorOn(IntPtr driverHandle)
        {
            uint ret;
            return DeviceIoControl(driverHandle, IOCTL_FD_MOTOR_ON, IntPtr.Zero, 0, IntPtr.Zero, 0, out ret, IntPtr.Zero);
        }

        public static bool WaitIndex(IntPtr driverHandle)
        {
            uint ret;
            return DeviceIoControl(driverHandle, IOCTL_FD_WAIT_INDEX, IntPtr.Zero, 0, IntPtr.Zero, 0, out ret, IntPtr.Zero);
        }

        public static unsafe bool DumpRegister(IntPtr driverHandle, out tagFD_DUMPREG_RESULT dumpResult)
        {
            tagFD_DUMPREG_RESULT dumpRes;
            int inBuffer;
            uint dwRead;
            bool r = DeviceIoControl(driverHandle, IOCTL_FDCMD_DUMPREG, (IntPtr)(&inBuffer), 0, (IntPtr)(&dumpRes), (uint)sizeof(tagFD_DUMPREG_RESULT), out dwRead, IntPtr.Zero);
            dumpResult = dumpRes;
            return r;
        }

        public static unsafe bool GetPartId(IntPtr driverHandle, out byte partId)
        {
            int inBuffer = 0;
            int dwRead;
            int result;
            bool r = DeviceIoControl(driverHandle, IOCTL_FDCMD_PART_ID, ref inBuffer, sizeof(int), out result, sizeof(int), out dwRead, IntPtr.Zero);
            partId = (byte)result;
            return r;
        }

        public static unsafe bool SenseDriveStatus(IntPtr driverHandle, tagFD_SENSE_PARAMS senseParams, out tagFD_DRIVE_STATUS driveStatus)
        {
            tagFD_DRIVE_STATUS status;
            uint dwRead;
            bool r = DeviceIoControl(driverHandle, IOCTL_FDCMD_SENSE_DRIVE_STATUS, (IntPtr)(&senseParams), (uint)sizeof(tagFD_SENSE_PARAMS), (IntPtr)(&status), (uint)sizeof(tagFD_DRIVE_STATUS), out dwRead, IntPtr.Zero);
            driveStatus = status;
            return r;
        }

        public static unsafe bool GetResult(IntPtr driverHandle, out tagFD_CMD_RESULT cmdResult)
        {
            tagFD_CMD_RESULT result;
            int inBuffer;
            uint dwRead;
            bool r = DeviceIoControl(driverHandle, IOCTL_FD_GET_RESULT, (IntPtr)(&inBuffer), 0, (IntPtr)(&result), (uint)sizeof(tagFD_CMD_RESULT), out dwRead, IntPtr.Zero);
            cmdResult = result;
            return r;
        }

        public static unsafe bool SetHeadSettleTime(IntPtr driverHandle, byte timeMs)
        {
            uint dwRead;
            bool r = DeviceIoControl(driverHandle, IOCTL_FD_SET_HEAD_SETTLE_TIME, (IntPtr)(&timeMs), sizeof(byte), IntPtr.Zero, 0, out dwRead, IntPtr.Zero);
            return r;
        }

        public static unsafe bool Recalibrate(IntPtr driverHandle, out tagFD_INTERRUPT_STATUS interruptStatus)
        {
            tagFD_INTERRUPT_STATUS result;
            uint dwRead;
            bool r = DeviceIoControl(driverHandle, IOCTL_FDCMD_RECALIBRATE, IntPtr.Zero, 0, (IntPtr)(&result), (uint)sizeof(tagFD_INTERRUPT_STATUS), out dwRead, IntPtr.Zero);
            interruptStatus = result;
            return r;
        }

        public static unsafe bool Recalibrate(IntPtr driverHandle)
        {
            tagFD_INTERRUPT_STATUS result;
            uint dwRead;
            bool r = DeviceIoControl(driverHandle, IOCTL_FDCMD_RECALIBRATE, IntPtr.Zero, 0, (IntPtr)(&result), (uint)sizeof(tagFD_INTERRUPT_STATUS), out dwRead, IntPtr.Zero);
            return r;
        }

        public static unsafe bool Reset(IntPtr driverHandle)
        {
            uint dwRead;
            return DeviceIoControl(driverHandle, IOCTL_FD_RESET, IntPtr.Zero, 0, IntPtr.Zero, 0, out dwRead, IntPtr.Zero);
        }

        public static unsafe bool ReadId(IntPtr driverHandle, tagFD_READ_ID_PARAMS pars, out tagFD_CMD_RESULT cmdResult)
        {
            tagFD_CMD_RESULT result;
            uint dwRead;
            bool r = DeviceIoControl(driverHandle, IOCTL_FDCMD_READ_ID, (IntPtr)(&pars), (uint)sizeof(tagFD_READ_ID_PARAMS), (IntPtr)(&result), (uint)sizeof(tagFD_CMD_RESULT), out dwRead, IntPtr.Zero);
            cmdResult = result;
            return r;
        }

        public static unsafe IntPtr Open(DataRate dataRate, Drive drive)
        {
            string fileName = drive == Drive.A ? "\\\\.\\fdraw0" : "\\\\.\\fdraw1";
            IntPtr handle = CreateFile(fileName, EFileAccess.GenericRead | EFileAccess.GenericWrite, 0, IntPtr.Zero, ECreationDisposition.OpenExisting, 0, IntPtr.Zero);
            if ((int)handle == INVALID_HANDLE_VALUE)
            {
                int lastError = Marshal.GetLastWin32Error();
                Log.Error?.Out($"Не удалось открыть драйвер: {lastError} {WinErrors.GetSystemMessage(lastError)} {(lastError == 2 ? $"(Drive {drive}: is not installed)" : "")}");
                return handle;
            }
            uint dwRet;
            bool r = DeviceIoControl(handle, IOCTL_FD_SET_DATA_RATE, (IntPtr)(&dataRate), sizeof(byte), IntPtr.Zero, 0, out dwRet, IntPtr.Zero);
            if (!r)
            {
                int lastError = Marshal.GetLastWin32Error();
                Log.Error?.Out($"Ошибка при установке DataRate: {lastError} {WinErrors.GetSystemMessage(lastError)}");
            }
            return handle;
        }

        public static IntPtr Open()
        {
            IntPtr handle = CreateFile("\\\\.\\fdraw0", EFileAccess.GenericRead | EFileAccess.GenericWrite, 0, IntPtr.Zero, ECreationDisposition.OpenExisting, 0, IntPtr.Zero);
            if ((int)handle == INVALID_HANDLE_VALUE)
            {
                int lastError = Marshal.GetLastWin32Error();
                Log.Error?.Out($"Не удалось открыть драйвер: {lastError} {WinErrors.GetSystemMessage(lastError)}");
                return handle;
            }
            return handle;
        }

        public static void Close(IntPtr handle)
        {
            if (handle != IntPtr.Zero) CloseHandle(handle);
        }

        public static IntPtr VirtualAlloc(int size)
        {
            IntPtr memoryHandle = VirtualAlloc(IntPtr.Zero, (UIntPtr)size, AllocationType.Commit, MemoryProtection.ReadWrite);
            if (memoryHandle == IntPtr.Zero)
            {
                int lastError = Marshal.GetLastWin32Error();
                Log.Error?.Out($"Не удалось выделить память: {lastError} {WinErrors.GetSystemMessage(lastError)}");
            }
            return memoryHandle;
        }

        public static unsafe bool Seek(IntPtr driverHandle, int track)
        {
            tagFD_SEEK_PARAMS seekParams = new tagFD_SEEK_PARAMS()
            {
                cyl = (byte)(track >> 1),
                head = 0
            };
            uint dwRet;
            return DeviceIoControl(driverHandle, IOCTL_FDCMD_SEEK, (IntPtr)(&seekParams), (uint)sizeof(tagFD_SEEK_PARAMS), IntPtr.Zero, 0, out dwRet, IntPtr.Zero);
        }

        public static unsafe int ReadSector(IntPtr driverHandle, IntPtr memoryHandle, int track, int sector, UpperSideHead head, int sizeCode)
        {            
            uint dwRet;
            tagFD_READ_WRITE_PARAMS readParams = new tagFD_READ_WRITE_PARAMS()
            {
                flags = FD_OPTION_MFM,
                phead = (byte)(track & 1),
                cyl = (byte)(track >> 1),
                head = head == UpperSideHead.Head1 ? (byte)(track & 1) : (byte)0,
                sector = (byte)sector,
                size = (byte)sizeCode,
                eot = (byte)(sector + 1),
                gap = 0x0a,
                datalen = 0xff,
            };
            bool r = DeviceIoControl(driverHandle, IOCTL_FDCMD_READ_DATA, (IntPtr)(&readParams), (uint)sizeof(tagFD_READ_WRITE_PARAMS), memoryHandle, (uint)SectorInfo.GetSizeBytes(sizeCode), out dwRet, IntPtr.Zero);
            int error = !r ? Marshal.GetLastWin32Error() : 0;
            Log.Trace?.Out($"Track: {track} | Sector: {sector} | Error: {error} | Bytes Read: {dwRet}");
            return error;
        }

        public static unsafe int ReadSectorF(IntPtr driverHandle, IntPtr memoryHandle, int cyl, int sector, int sizeCode, int phead, int head, int gap, int datalen)
        {
            uint dwRet;
            tagFD_READ_WRITE_PARAMS readParams = new tagFD_READ_WRITE_PARAMS()
            {
                flags = FD_OPTION_MFM,
                phead = (byte)phead,
                cyl = (byte)cyl,
                head = (byte)head,
                sector = (byte)sector,
                size = (byte)sizeCode,
                eot = (byte)(sector + 1),
                gap = (byte)gap,
                datalen = (byte)datalen,
            };
            bool r = DeviceIoControl(driverHandle, IOCTL_FDCMD_READ_DATA, (IntPtr)(&readParams), (uint)sizeof(tagFD_READ_WRITE_PARAMS), memoryHandle, (uint)SectorInfo.GetSizeBytes(sizeCode), out dwRet, IntPtr.Zero);
            int error = !r ? Marshal.GetLastWin32Error() : 0;
            Log.Trace?.Out($"Cyl: {cyl} | PHead: {phead} | Head: {head} | Sector: {sector} | Gap: {gap} | DataLen: {datalen} | Error: {error} | Bytes Read: {dwRet}");
            return error;
        }

        public static unsafe int ReadTrack(IntPtr driverHandle, IntPtr memoryHandle, int track, int sector)
        {
            uint dwRet;
            //tagFD_READ_WRITE_PARAMS readParams = new tagFD_READ_WRITE_PARAMS()
            //{
            //    flags = FD_OPTION_MFM,
            //    phead = (byte)(track & 1),
            //    cyl = (byte)(track >> 1),
            //    head = upperSideHead == UpperSideHead.Head1 ? (byte)(track & 1) : (byte)0,
            //    //sector = 1,
            //    //size = 1,
            //    //eot = 1 + 16,
            //    gap = 0x0a,
            //    datalen = 0xff,
            //};
            tagFD_READ_WRITE_PARAMS readParams = new tagFD_READ_WRITE_PARAMS()
            {
                flags = FD_OPTION_MFM,
                phead = (byte)(track & 1),
                cyl = (byte)(track / 2),
                head = 0,
                sector = 0,
                size = 7,
                eot = 0,
                gap = 0,
                datalen = 0,
            };
            bool r = DeviceIoControl(driverHandle, IOCTL_FDCMD_READ_TRACK, (IntPtr)(&readParams), (uint)sizeof(tagFD_READ_WRITE_PARAMS), memoryHandle, 65536, out dwRet, IntPtr.Zero);
            int error = !r ? Marshal.GetLastWin32Error() : 0;
            Log.Trace?.Out($"Track: {track} | Error: {error} | Bytes Read: {dwRet}");
            return error;
        }

        public static unsafe int ScanTrack(IntPtr driverHandle, int track, UpperSideHead upperSideHead)
        {
            tagFD_SEEK_PARAMS seekParams = new tagFD_SEEK_PARAMS()
            {
                cyl = (byte)(track >> 1),
                head = upperSideHead == UpperSideHead.Head1 ? (byte)(track & 1) : (byte)0
            };
            uint dwRet;
            DeviceIoControl(driverHandle, IOCTL_FDCMD_SEEK, (IntPtr)(&seekParams), (uint)sizeof(tagFD_SEEK_PARAMS), IntPtr.Zero, 0, out dwRet, IntPtr.Zero);
            tagFD_SCAN_PARAMS scanParams = new tagFD_SCAN_PARAMS()
            {
                Flags = 0,
                Head = (byte)(track & 1)
            };
            tagFD_SCAN_RESULT scanResult = new tagFD_SCAN_RESULT();
            bool r = DeviceIoControl(driverHandle, IOCTL_FD_SCAN_TRACK, (IntPtr)(&scanParams), (uint)sizeof(tagFD_SCAN_PARAMS), 
                (IntPtr)(&scanResult), (uint)sizeof(tagFD_SCAN_RESULT), out dwRet, IntPtr.Zero);
            int error = !r ? Marshal.GetLastWin32Error() : 0;
            ScanResult scanResultNew = new ScanResult(scanResult);
            return error;
        }

        public static unsafe bool CheckDisk(IntPtr driverHandle)
        {
            //int checkd = 0;
            //int dwRet;
            //int result;
            //return DeviceIoControl(driverHandle, IOCTL_FD_CHECK_DISK, ref checkd, sizeof(int), out result, 0, out dwRet, IntPtr.Zero);

            int checkd = 1;
            int dwRet = 0;
            bool r = DeviceIoControl(driverHandle, IOCTL_FD_CHECK_DISK, (IntPtr)(&checkd), sizeof(int), IntPtr.Zero, 0, (IntPtr)(&dwRet), IntPtr.Zero);
            Log.Trace?.Out($"r={r} | checkd={checkd} | dwRet={dwRet}");
            return r;
        }
    }

    public struct tagFD_READ_WRITE_PARAMS
    {
        public byte flags;                         // MT MFM SK
        public byte phead;
        public byte cyl, head, sector, size;
        public byte eot, gap, datalen;
    }

    public struct tagFD_SEEK_PARAMS
    {
        public byte cyl;
        public byte head;
    }

    public struct tagFD_RELATIVE_SEEK_PARAMS
    {
        public byte flags;
        public byte head;
        public byte offset;
    }

    public struct tagFD_RAW_READ_PARAMS
    {
        public byte flags;
        public byte head, size;
    }

    public struct tagFD_FDC_INFO
    {
        public FdcControllerType ControllerType;
        public FdcSpeedRate SpeedsAvailable;
        public byte BusType;
        public ulong BusNumber;
        public ulong ControllerNumber;
        public ulong PeripheralNumber;
    }

    public struct tagFD_SCAN_PARAMS
    {
        public byte Flags;
        public byte Head;
    }

    public struct tagFD_ID_HEADER
    {
        public byte cyl, head, sector, size;
    }

    public unsafe struct tagFD_SCAN_RESULT
    {
        public byte count;
        public fixed byte Header[256];
    }

    public struct tagFD_DUMPREG_RESULT
    {
        public byte pcn0, pcn1, pcn2, pcn3;
        public byte srt_hut;
        public byte hlt_nd;
        public byte sceot;
        public byte lock_d0123_gap_wgate;
        public byte eis_efifo_poll_fifothr;
        public byte pretrk;
    }

    /*
    Контроллер НГМД i8272.

    ¦====¦=============¦======¦========================================================================¦
    ¦                                       Регистр состояния ST3                                      ¦
    ¦====T=============T======T========================================================================¦
    ¦ D7 ¦  Fault      ¦ FT   ¦ Отражает состояние сигнала Fault с накопителя.                         ¦
    ¦====+=============+======+========================================================================¦
    ¦ D6 ¦ Write       ¦ WP   ¦ Отражает состояние сигнала Write Protect (защита от записи)            ¦
    ¦    ¦ Protect     ¦      ¦ с накопителя.                                                          ¦
    ¦====+=============+======+========================================================================¦
    ¦ D5 ¦ Ready       ¦ RDY  ¦ Отражает состояние сигнала Ready с накопителя.                         ¦
    ¦====+=============+======+========================================================================¦
    ¦ D4 ¦ Track 00    ¦ T0   ¦ Отражает состояние сигнала Track 00 с накопителя.                      ¦
    ¦====+=============+======+========================================================================¦
    ¦ D3 ¦ Two Side    ¦ TS   ¦ Отражает состояние сигнала Two Side с накопителя.                      ¦
    ¦====+=============+======+========================================================================¦
    ¦ D2 ¦ Head Address¦ HD   ¦ Указывает на выбранную поверхность диска.                              ¦
    ¦====+=============+======+========================================================================¦
    ¦ D1 ¦ Unit        ¦ US1  ¦ Два бита показывают выбранный накопитель.                              ¦
    ¦ D0 ¦ Select      ¦ US0  ¦                                                                        ¦
    L====¦=============¦======¦========================================================================-


    --------------------------------

        82078 64 PIN
        CHMOS SINGLE-CHIP FLOPPY DISK CONTROLLER

        7.4 Status Register 3
        Bit
        Symbol Name Description
        No.
        7 — — Unused. This bit is always ‘‘0’’.
        6 WP Write Protected Indicates the status of the WP pin.
        5 — — Unused. This bit is always ‘‘1’’.
        4 T0 TRACK 0 Indicates the status of the TRK0 pin.
        3 — — Unused. This bit is always ‘‘1’’.
        2 HD Head Address Indicates the status of the HDSEL pin.
        1, 0 DS1, 0 Drive Select Indicates the status of the DS1, DS0 pins.
    */
    public struct tagFD_DRIVE_STATUS
    {
        /// <summary>
        ///D6 - Write Protect (1 - true)
        ///D4 - TR00 (1 - true)
        ///D2 - HEAD. Возвращает то же значение что передано в head в структуре tagFD_SENSE_PARAMS.
        /// </summary>
        public byte st3;
    }

    public struct tagFD_SENSE_PARAMS
    {
        public byte head;
    }

    public struct tagFD_CMD_RESULT
    {
        public byte st0, st1, st2;
        public byte cyl, head, sector, size;
    }

    public struct tagFD_INTERRUPT_STATUS
    {
        public byte st0;
        public byte pcn;
    }

    public struct tagFD_READ_ID_PARAMS
    {
        /// <summary>
        /// Имеет значение только флаг FD_OPTION_MFM.
        /// </summary>
        public byte flags;
        /// <summary>
        /// D0 задает физическую головку.
        /// </summary>
        public byte head;
    }

    public struct ScanResult
    {
        public int Count;
        public tagFD_ID_HEADER[] Headers;

        public unsafe ScanResult(tagFD_SCAN_RESULT source)
        {
            Count = source.count;
            Headers = new tagFD_ID_HEADER[Count];
            byte* ptr = source.Header;
            for (int i = 0; i < Count; i++)
            {
                Headers[i] = *(tagFD_ID_HEADER*)ptr;
                ptr += sizeof(tagFD_ID_HEADER);
            }
        }
    }

    public enum FdcControllerType : byte
    {
        FDC_TYPE_UNKNOWN = 0,
        FDC_TYPE_UNKNOWN2 = 1,
        FDC_TYPE_NORMAL = 2,
        FDC_TYPE_ENHANCED = 3,
        FDC_TYPE_82077 = 4,
        FDC_TYPE_82077AA = 5,
        FDC_TYPE_82078_44 = 6,
        FDC_TYPE_82078_64 = 7,
        FDC_TYPE_NATIONAL = 8
    }

    [Flags]
    public enum FdcSpeedRate : byte
    {
        FDC_SPEED_250K = 0x01,
        FDC_SPEED_300K = 0x02,
        FDC_SPEED_500K = 0x04,
        FDC_SPEED_1M = 0x08,
        FDC_SPEED_2M = 0x10
    }

    public enum UpperSideHead
    {
        Head0,
        Head1
    }

    public enum DiskSide
    {
        Side0,
        Side1,
        Both
    }

    public enum ScanMode
    {
        None,
        UnscannedOnly,
        Once,
        EachTrackRead
    }

    public enum DataRate : byte
    {
        FD_RATE_500K = 0,
        FD_RATE_300K = 1,
        FD_RATE_250K = 2,
        FD_RATE_1M = 3
    }

    public enum Drive
    {
        A,
        B
    }

    public static class WinErrors
    {
        #region definitions
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int FormatMessage(FormatMessageFlags dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, ref IntPtr lpBuffer, uint nSize, IntPtr Arguments);

        [Flags]
        private enum FormatMessageFlags : uint
        {
            FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100,
            FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200,
            FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000,
            FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000,
            FORMAT_MESSAGE_FROM_HMODULE = 0x00000800,
            FORMAT_MESSAGE_FROM_STRING = 0x00000400,
        }
        #endregion

        /// <summary>
        /// Gets a user friendly string message for a system error code
        /// </summary>
        /// <param name="errorCode">System error code</param>
        /// <returns>Error string</returns>
        public static string GetSystemMessage(int errorCode)
        {
            try
            {
                IntPtr lpMsgBuf = IntPtr.Zero;

                int dwChars = FormatMessage(
                    FormatMessageFlags.FORMAT_MESSAGE_ALLOCATE_BUFFER | FormatMessageFlags.FORMAT_MESSAGE_FROM_SYSTEM | FormatMessageFlags.FORMAT_MESSAGE_IGNORE_INSERTS,
                    IntPtr.Zero,
                    (uint)errorCode,
                    0, // Default language
                    ref lpMsgBuf,
                    0,
                    IntPtr.Zero);
                if (dwChars == 0)
                {
                    // Handle the error.
                    int le = Marshal.GetLastWin32Error();
                    return "Unable to get error code string from System - Error " + le.ToString();
                }

                string sRet = Marshal.PtrToStringAnsi(lpMsgBuf);

                // Free the buffer.
                lpMsgBuf = LocalFree(lpMsgBuf);
                return sRet.Trim('\r', '\n');
            }
            catch (Exception e)
            {
                return "Unable to get error code string from System -> " + e.ToString();
            }
        }
    }
}
