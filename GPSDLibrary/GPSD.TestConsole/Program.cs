using GPSD.Library;

namespace GPSD.TestConsole
{
    public class Program
    {
        static void Main(string[] args)
        {
            var gpsdService = new GpsdService("178.50.42.172", 80);
            //gpsdService.SetProxy("proxy", 80);
            //gpsdService.SetProxyAuthentication("EXJ508", "*****");
            gpsdService.StartService();

            //ListernerTest test = new ListernerTest();
            //test.createListener();
        }
    }
}
