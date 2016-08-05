using GPSD.Library;

namespace GPSD.TestConsole
{
    public class Program
    {
        static void Main(string[] args)
        {
            var gpsdService = new GpsdService();
            gpsdService.StartService();
        }
    }
}
