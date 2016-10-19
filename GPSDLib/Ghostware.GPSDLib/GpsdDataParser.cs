using Ghostware.GPSDLib.Models;
using Newtonsoft.Json;

namespace Ghostware.GPSDLib
{
    public class GpsdDataParser
    {
        public object GetGpsData(string gpsData)
        {
            try
            {
                var classType = JsonConvert.DeserializeObject<DataClassType>(gpsData);
                return JsonConvert.DeserializeObject(gpsData, classType.GetClassType());
            }
            catch (JsonReaderException ex)
            {
                return null;
            }
            catch (JsonSerializationException ex)
            {
                return null;
            }
        }
    }
}
