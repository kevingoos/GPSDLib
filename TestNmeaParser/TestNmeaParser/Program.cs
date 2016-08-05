namespace TestNmeaParser
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var gpsService = new GpsService();
            gpsService.StartService();
        }
    }
}
