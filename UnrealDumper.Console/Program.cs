using UnrealDumper.Net;
using UnrealDumper.Net.Models.Offsets;

namespace UnrealDumper.Console
{
    public class Program
    {
        public static void Main()
        {
            var settings = SdkSettings.FortniteSettings;
            var dumper = new Dumper(settings);
            dumper.Start();
        }
    }
}