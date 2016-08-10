using System;
using System.Runtime.InteropServices;
using GPSD.Library;
using GPSD.Library.Models;

namespace GPSD.TestConsole
{
    public class Program
    {
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler();
        static EventHandler _handler;

        private static GpsdService _gpsdService;

        static void Main(string[] args)
        {
            _handler += Handler;
            SetConsoleCtrlHandler(_handler, true);

            _gpsdService = new GpsdService("178.50.89.134", 80);
            _gpsdService.SetProxy("proxy", 80);
            _gpsdService.SetProxyAuthentication("*****", "*****");

            _gpsdService.OnLocationChanged += GpsdServiceOnOnLocationChanged;
            _gpsdService.StartService();
            
        }

        private static void GpsdServiceOnOnLocationChanged(object source, GpsLocation e)
        {
            Console.WriteLine(e.ToString());
        }

        private static bool Handler()
        {
            _gpsdService?.StopService();
            return true;
        }
    }
}
