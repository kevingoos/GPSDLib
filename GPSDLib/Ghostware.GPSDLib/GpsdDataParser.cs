using Ghostware.GPSDLib.Models;
using Newtonsoft.Json;

namespace Ghostware.GPSDLib
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
