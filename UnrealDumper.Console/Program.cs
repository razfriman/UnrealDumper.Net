using UnrealDumper.Net;
using UnrealDumper.Net.Models.Offsets;
using UnrealDumper.Net.Models.Unreal;

namespace UnrealDumper.Console
{
    public class Program
    {
        public static void Main()
        {
            var settings = SdkSettings.FortniteSettings;
            var dumper = new Dumper(settings);
            //dumper.Start();
            FindKey();
        }

        private static void FindKey()
        {
            // None
            var enc = new byte[] { 0xB4, 0x96, 0x66, 0xE6 };
            var dec = new byte[] { 0x4E, 0x6F, 0x6E, 0x65 };
        
        
            // ByteProperty
            var enc2 = new byte[] { 0xD3, 0xB0, 0x75, 0x5C, 0x21, 0xDB, 0x8E, 0x69, 0x34, 0xFB, 0xB5, 0x80 };
            var dec2 = new byte[] { 0x74, 0xf7, 0xc7, 0xe6, 0xf5, 0x67, 0x56, 0x17, 0xc6, 0x07, 0x87, 0xe7 };
        
            for (uint i = 0; i < uint.MaxValue; i++)
            {
                if (IsMatch(UeFNameEntry.DecryptRaw(enc2, i), dec2))
                {
                    System.Console.WriteLine($"Key: {i}");
                    break;
                }
            }
            System.Console.WriteLine("Done");
        }

        private static bool IsMatch(byte[] res, byte[] dec2)
        {
            for (var i = 0; i < res.Length; i++)
            {
                if (res[i] != dec2[i])
                {
                    return false;
                }
            }
        
            return true;
        }
    }
}