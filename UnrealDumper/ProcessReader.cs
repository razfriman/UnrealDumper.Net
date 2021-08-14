using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UnrealDumper.Net
{
    public class ProcessReader
    {
        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess,
            IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        public Process Process { get; }
        public DumperSettings Settings { get; }

        public ProcessReader(Process process, DumperSettings settings)
        {
            Process = process;
            Settings = settings;
        }

        public byte[] ReadMemory(long address, int size)
        {
            var buffer = new byte[size];
            ReadProcessMemory(Process.Handle, new IntPtr(address), buffer, size, out _);
            return buffer;
        }

        public MemoryReader GetMemoryReader(long address, int size)
        {
            var buffer = ReadMemory(address, size);
            return new MemoryReader(buffer);
        }
    }
}