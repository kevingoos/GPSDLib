using System.Runtime.InteropServices;
using GPSD.Library;

namespace GPSD.TestConsole
{
    public class Program
    {
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler();
        static EventHandler _handler;

        private static GpsdService gpsdService;

        static void Main(string[] args)
        {
            _handler += Handler;
            SetConsoleCtrlHandler(_handler, true);

            gpsdService = new GpsdService("178.50.89.134", 80);
            gpsdService.SetProxy("proxy", 80);
            gpsdService.StartService();
        }

        private static bool Handler()
        {
            gpsdService?.StopService();
            return true;
        }
    }
}
