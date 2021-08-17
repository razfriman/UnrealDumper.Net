using System.Linq;
using System.Text;
using System.Threading;
using UnrealDumper.Net;
using UnrealDumper.Net.Models;

namespace UnrealDumper.Console
{
    public class Program
    {
        public static void Main()
        {
            var settings = SdkSettings.FortniteSettings_1740;
            var dumper = new Dumper(settings);
            dumper.Start();
            // FindKey();
        }

        private static void FindKey()
        {
            // None
            var encxs = new byte[] { 0xB4 };
            var decxs = new byte[] { 0x4E };

            // None
            var enc = new byte[] { 0xB4, 0x96, 0x66, 0xE6 };
            var dec = new byte[] { 0x4E, 0x6F, 0x6E, 0x65 };

            // ByteProperty
            var enc2 = new byte[] { 0x74, 0xf7, 0xc7, 0xe6, 0xf5, 0x67, 0x56, 0x17, 0xc6, 0x07, 0x87, 0xe7 };
            var dec2 = new byte[] { 0x42, 0x79, 0x74, 0x65, 0x50, 0x72, 0x6F, 0x70, 0x65, 0x72, 0x74, 0x79 };

            var cts = new CancellationTokenSource();

            for (uint i = 160; i < uint.MaxValue; i++)
            {
                if (i % 100_000 == 0)
                {
                    System.Console.WriteLine(i);
                }

                var buffer = enc2.ToArray();
                EncryptionSettings.Decrypt1740(buffer, i);
                if (IsMatch(buffer, dec2))
                {
                    System.Console.WriteLine($"Key: {i}");
                    break;
                }
            }

            System.Console.WriteLine("Done");
        }

        private static bool IsMatch(byte[] res, byte[] dec2)
        {
            var s = Encoding.ASCII.GetString(dec2);
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