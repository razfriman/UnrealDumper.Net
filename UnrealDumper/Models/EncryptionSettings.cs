namespace UnrealDumper.Net.Models
{
    public class EncryptionSettings
    {
        public static void Decrypt1730(byte[] buffer, uint key)
        {
            var currentCipherValue = (key >> 8) | (key << 8);
            uint keyBump = (key >> 3) & (0x00_00_00_00_FF_FF_FF_FF);
            for (var i = 0; i < buffer.Length; i++)
            {
                currentCipherValue += keyBump;
                buffer[i] = (byte)(buffer[i] ^ ((byte)(currentCipherValue & 0x00_00_00_FF)));
            }
        }
        
        public static void Decrypt1740(byte[] buffer, uint key)
        {
            var len = buffer.Length;
            key >>= 0x05;

            var curKey = key;
            for (uint i = 0; i < len; i++)
            {
                var edx = buffer[i];
                var ecx = buffer[i];
                curKey += 0x20;
                curKey += i;
                ecx >>= 4;
                var cl = (byte)((ecx & 0xFF) ^ (curKey & 0xFF));
                var dl = (byte)(edx & 0xFF);
                dl <<= 4;
                cl &= 0xF;
                cl |= dl;
                buffer[i] = cl;
            }
        }
    }
}