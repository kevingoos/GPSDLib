using GPSD.Library;

namespace GPSD.TestConsole
{
    public class Program
    {
        static void Main(string[] args)
        {
            var gpsdService = new GpsdService("178.50.89.134", 80);
            gpsdService.SetProxy("proxy", 80);
            gpsdService.StartService();
        }
    }
}
