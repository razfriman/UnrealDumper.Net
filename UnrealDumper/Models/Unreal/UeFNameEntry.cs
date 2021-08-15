using System;
using System.Text;

namespace UnrealDumper.Net.Models.Unreal
{
    public class UeFNameEntry
    {
        public long Address { get; init; }
        public ProcessReader Reader { get; init; }

        public UeFNameEntry(ProcessReader reader, long address)
        {
            Reader = reader;
            Address = address;
        }

        public (bool wide, ushort len) Info()
        {
            var reader = Reader.GetMemoryReader(Address + Reader.Settings.StructOffsets.FNameEntry.Info, 2);
            var info = reader.ReadUInt16();
            var len = (ushort)(info >> Reader.Settings.StructOffsets.FNameEntry.LenBit);
            var wide = ((info >> Reader.Settings.StructOffsets.FNameEntry.WideBit) & 1) > 0;
            return (wide, len);
        }

        public string String()
        {
            var (wide, len) = Info();
            return String(wide, len);
        }

        public ushort Size(bool wide, ushort len)
        {
            var bytes = (ushort)(Reader.Settings.StructOffsets.FNameEntry.HeaderSize + len * (wide ? 2 : 1));
            return (ushort)((bytes + Reader.Settings.StructOffsets.Stride - 1u) &
                            ~(Reader.Settings.StructOffsets.Stride - 1u));
        }

        public string String(bool wide, ushort len)
        {
            if (wide)
            {
                var reader = Reader.GetMemoryReader(Address + Reader.Settings.StructOffsets.FNameEntry.HeaderSize,
                    len * 2);
                var bytes = reader.ReadBytes(len);
                return Encoding.Unicode.GetString(bytes);
            }
            else
            {
                var reader = Reader.GetMemoryReader(Address + Reader.Settings.StructOffsets.FNameEntry.HeaderSize, len);
                var bytes = reader.ReadBytes(len);
                Decrypt(bytes, 22976);
                return Encoding.ASCII.GetString(bytes);
            }
        }
        
        public static void Decrypt(byte[] buffer, uint key)
        {
            var currentCipherValue = (key >> 8) | (key << 8);
            uint keyBump = (key >> 3) & (0x00_00_00_00_FF_FF_FF_FF);
            for (var i = 0; i < buffer.Length; i++)
            {
                currentCipherValue += keyBump;
                buffer[i] = (byte)(buffer[i] ^ ((byte)(currentCipherValue & 0x00_00_00_FF)));
            }
        }

        public static byte[] DecryptRaw(byte[] enc, uint key)
        {
            var len = enc.Length;
            var result = new byte[len];
        
            uint eax = key;
            uint ecx = eax;
            uint edx = eax;
            edx >>= 8;
            ecx <<= 8;
            edx |= ecx;
            ulong r8 = eax;
            r8 >>= 3;
        
            for (int i = 0; i < len; i++)
            {
                edx += (uint)(r8 & 0x00_00_00_00_FF_FF_FF_FF);
                result[i] = (byte)(enc[i] ^ ((byte)(edx & 0x00_00_00_FF)));
            }
        
            return result;
        }
    }
}