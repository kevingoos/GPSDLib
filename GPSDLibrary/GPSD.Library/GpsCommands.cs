using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSD.Library
{
    public static class GpsCommands
    {
        public const string EnableCommand = "?WATCH={\"enable\":true,\"json\":true,\"nmea\":false}";
        public const string DisableCommand = "?WATCH={\"enable\":false}";
    }
}
