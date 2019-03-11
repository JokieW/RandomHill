using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

    public static class Scribe
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern int GetLastError();

        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_WM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;


        [DllImport("kernel32.dll", SetLastError = false)]
        static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, uint dSize, int flNewProtect, out int lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = false)]
        public static extern void GetSystemInfo(out SYSTEM_INFO Info);

        [StructLayout(LayoutKind.Explicit)]
        public struct SYSTEM_INFO_UNION
        {
            [FieldOffset(0)]
            public UInt32 OemId;
            [FieldOffset(0)]
            public UInt16 ProcessorArchitecture;
            [FieldOffset(2)]
            public UInt16 Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SYSTEM_INFO
        {
            public SYSTEM_INFO_UNION CpuInfo;
            public UInt32 PageSize;
            public UInt32 MinimumApplicationAddress;
            public UInt32 MaximumApplicationAddress;
            public UInt32 ActiveProcessorMask;
            public UInt32 NumberOfProcessors;
            public UInt32 ProcessorType;
            public UInt32 AllocationGranularity;
            public UInt16 ProcessorLevel;
            public UInt16 ProcessorRevision;
        }

        private static uint _pageSize;
        private static IntPtr handle = IntPtr.Zero;

        static Scribe()
        {
            SYSTEM_INFO si;
            GetSystemInfo(out si);
            _pageSize = si.PageSize;
        }

        public static void InitTo(Process memProcess)
        {
            handle = OpenProcess(PROCESS_VM_OPERATION | PROCESS_WM_READ | PROCESS_VM_WRITE, false, memProcess.Id);

            foreach (IntPtr intptr in _pages)
            {
                int oldsettings;
                VirtualProtectEx(handle, intptr, _pageSize, 0x40, out oldsettings);
            }
        }

        private static HashSet<IntPtr> _pages = new HashSet<IntPtr>();
        //https://msdn.microsoft.com/en-us/library/windows/desktop/aa366786(v=vs.85).aspx
        public static void RegisterForPage(IntPtr address)
        {
            IntPtr pageAddress = new IntPtr(((long)address / _pageSize) * _pageSize);
            _pages.Add(pageAddress);
        }

        //bytes
        public static byte[] ReadBytes(IntPtr address, int byteCount)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[byteCount];

            ReadProcessMemory(handle, address, buffer, buffer.Length, ref bytesRead);
            return buffer;
        }

        public static void WriteBytes(IntPtr address, byte[] data)
        {
            int bytesWrite = 0;
            if (!WriteProcessMemory(handle, address, data, data.Length, ref bytesWrite))
            {
                Console.WriteLine("Write error: " + GetLastError().ToString("X"));
            }
        }

        public static byte ReadByte(IntPtr address)
        {
            return ReadBytes(address, 1)[0];
        }

        public static void WriteByte(IntPtr address, byte data)
        {
            WriteBytes(address, new byte[] { data });
        }

        //bools
        public static bool ReadBool(IntPtr address)
        {
            return BitConverter.ToBoolean(ReadBytes(address, 1), 0);
        }

        public static void WriteBool(IntPtr address, bool data)
        {
            WriteBytes(address, BitConverter.GetBytes(data));
        }

        //shorts
        public static ushort ReadUInt16(IntPtr address)
        {
            return BitConverter.ToUInt16(ReadBytes(address, 2), 0);
        }

        public static void WriteUInt16(IntPtr address, ushort data)
        {
            WriteBytes(address, BitConverter.GetBytes(data));
        }

        public static short ReadInt16(IntPtr address)
        {
            return BitConverter.ToInt16(ReadBytes(address, 2), 0);
        }

        public static void WriteInt16(IntPtr address, short data)
        {
            WriteBytes(address, BitConverter.GetBytes(data));
        }

        //ints
        public static uint ReadUInt32(IntPtr address)
        {
            return BitConverter.ToUInt32(ReadBytes(address, 4), 0);
        }

        public static void WriteUInt32(IntPtr address, uint data)
        {
            WriteBytes(address, BitConverter.GetBytes(data));
        }

        public static int ReadInt32(IntPtr address)
        {
            return BitConverter.ToInt32(ReadBytes(address, 4), 0);
        }

        public static void WriteInt32(IntPtr address, int data)
        {
            WriteBytes(address, BitConverter.GetBytes(data));
        }

        //longs
        public static ulong ReadUInt64(IntPtr address)
        {
            return BitConverter.ToUInt64(ReadBytes(address, 8), 0);
        }

        public static void WriteUInt64(IntPtr address, ulong data)
        {
            WriteBytes(address, BitConverter.GetBytes(data));
        }

        public static long ReadInt64(IntPtr address)
        {
            return BitConverter.ToInt64(ReadBytes(address, 8), 0);
        }

        public static void WriteInt64(IntPtr address, long data)
        {
            WriteBytes(address, BitConverter.GetBytes(data));
        }

        //floats
        public static float ReadSingle(IntPtr address)
        {
            return BitConverter.ToSingle(ReadBytes(address, 4), 0);
        }

        public static void WriteSingle(IntPtr address, float data)
        {
            WriteBytes(address, BitConverter.GetBytes(data));
        }

        //doubles
        public static double ReadDouble(IntPtr address)
        {
            return BitConverter.ToDouble(ReadBytes(address, 8), 0);
        }

        public static void WriteDouble(IntPtr address, double data)
        {
            WriteBytes(address, BitConverter.GetBytes(data));
        }

        //T for Trouble
        //Don't use if you can avoid it
        public static T Read<T>(IntPtr address)
        {
            if (typeof(T) == typeof(byte)) return (T)(object)ReadByte(address);
            if (typeof(T) == typeof(bool)) return (T)(object)ReadBool(address);
            if (typeof(T) == typeof(ushort)) return (T)(object)ReadUInt16(address);
            if (typeof(T) == typeof(short)) return (T)(object)ReadInt16(address);
            if (typeof(T) == typeof(uint)) return (T)(object)ReadUInt32(address);
            if (typeof(T) == typeof(int)) return (T)(object)ReadInt32(address);
            if (typeof(T) == typeof(ulong)) return (T)(object)ReadUInt64(address);
            if (typeof(T) == typeof(long)) return (T)(object)ReadInt64(address);
            if (typeof(T) == typeof(float)) return (T)(object)ReadSingle(address);
            if (typeof(T) == typeof(double)) return (T)(object)ReadDouble(address);
            throw new InvalidOperationException("Trying to read unregistered T");
        }

        public static Enum ReadEnum(IntPtr address, Type actualType)
        {
            if (Enum.GetUnderlyingType(actualType) == typeof(byte)) return (Enum)Enum.ToObject(actualType, ReadByte(address));
            if (Enum.GetUnderlyingType(actualType) == typeof(int)) return (Enum)Enum.ToObject(actualType, ReadInt32(address));
            throw new InvalidOperationException("Trying to read unregistered Enum T");
        }

        //Don't use if you can avoid it
        public static void Write<T>(IntPtr address, T data)
        {
            if (typeof(T) == typeof(byte)) { WriteByte(address, (byte)(object)data); return; }
            if (typeof(T) == typeof(bool)) { WriteBool(address, (bool)(object)data); return; }
            if (typeof(T) == typeof(ushort)) { WriteUInt16(address, (ushort)(object)data); return; }
            if (typeof(T) == typeof(short)) { WriteInt16(address, (short)(object)data); return; }
            if (typeof(T) == typeof(uint)) { WriteUInt32(address, (uint)(object)data); return; }
            if (typeof(T) == typeof(int)) { WriteInt32(address, (int)(object)data); return; }
            if (typeof(T) == typeof(ulong)) { WriteUInt64(address, (ulong)(object)data); return; }
            if (typeof(T) == typeof(long)) { WriteInt64(address, (long)(object)data); return; }
            if (typeof(T) == typeof(float)) { WriteSingle(address, (float)(object)data); return; }
            if (typeof(T) == typeof(double)) { WriteDouble(address, (double)(object)data); return; }
            if (typeof(Enum).IsAssignableFrom(typeof(T)))
            {
                if (Enum.GetUnderlyingType(typeof(T)) == typeof(byte)) { WriteByte(address, (byte)(object)data); return; }
                if (Enum.GetUnderlyingType(typeof(T)) == typeof(int)) { WriteInt32(address, (int)(object)data); return; }
                throw new InvalidOperationException("Trying to write unregistered Enum T");
            }
            throw new InvalidOperationException("Trying to write unregistered T");
        }

        public static void WriteEnum(IntPtr address, Enum data, Type actualType)
        {
            if (Enum.GetUnderlyingType(actualType) == typeof(byte)) { WriteByte(address, (byte)(object)data); return; }
            if (Enum.GetUnderlyingType(actualType) == typeof(int)) { WriteInt32(address, (int)(object)data); return; }
            throw new InvalidOperationException("Trying to write unregistered Enum T");
        }
    }