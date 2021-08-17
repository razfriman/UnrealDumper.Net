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
        
        public static byte[] DecryptRaw1750(byte[] enc, uint key)
        {
            // 0:  00 00                   add    BYTE PTR [rax],al
            // 2:  00 0f                   add    BYTE PTR [rdi],cl
            // 4:  b7 1b                   mov    bh,0x1b
            // 6:  ba e4 00 00 00          mov    edx,0xe4
            // b:  c1 eb 06                shr    ebx,0x6
            // e:  48 8b 08                mov    rcx,QWORD PTR [rax]
            // 11: 8b 04 0a                mov    eax,DWORD PTR [rdx+rcx*1]
            // 14: 39 05 7c 7a d6 08       cmp    DWORD PTR [rip+0x8d67a7c],eax        # 0x8d67a96
            // 1a: 0f 8f e6 fe 2e 02       jg     0x22eff06
            // 20: 44 8b 05 9f b1 af 08    mov    r8d,DWORD PTR [rip+0x8afb19f]        # 0x8afb1c6
            // 27: 33 c0                   xor    eax,eax
            // 29: 41 c1 e8 05             shr    r8d,0x5
            // 2d: 85 db                   test   ebx,ebx
            // 2f: 74 2c                   je     0x5d
            
            // 36: 0f be 17           /----movsx  edx,BYTE PTR [rdi]  
            // 39: 48 8d 7f 01        |    lea    rdi,[rdi+0x1]
            // 3d: 8b ca              |    mov    ecx,edx
            // 3f: 41 83 c0 20        |    add    r8d,0x20
            // 43: c1 e9 04           |    shr    ecx,0x4
            // 46: 44 03 c0           |    add    r8d,eax
            // 49: 41 32 c8           |    xor    cl,r8b
            // 4c: c0 e2 04           |    shl    dl,0x4
            // 4f: 80 e1 0f           |    and    cl,0xf
            // 52: ff c0              |    inc    eax
            // 54: 0a ca              |    or     cl,dl
            // 56: 88 4f ff           |    mov    BYTE PTR [rdi-0x1],cl
            // 59: 3b c3              |    cmp    eax,ebx
            // 5b: 7c d9              \----jl     0x36
            // 5d: 48 8b 5c 24 30          mov    rbx,QWORD PTR [rsp+0x30]
            // 62: 48 83 c4 20             add    rsp,0x20
            // 66: 5f                      pop    rdi
            // 67: c3                      ret
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
                var edx = enc[i];
                var ecx = edx;
                r8d += 0x20;
                ecx >> 4;
                r8d += eax;
                cl ^= r8b;
                dl << 4;
                cl &= 0xF;
                result[i] = cl;
            }
        
            return result;
        }
    }
}