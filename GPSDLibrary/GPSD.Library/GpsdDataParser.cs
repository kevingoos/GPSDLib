using GPSD.Library.Models;
using Newtonsoft.Json;

namespace GPSD.Library
{
    public class GpsdDataParser
    {
        public object GetGpsData(string gpsData)
        {
            var classType = JsonConvert.DeserializeObject<DataClassType>(gpsData);
            return JsonConvert.DeserializeObject(gpsData, classType.GetClassType());
        }
    }
}
