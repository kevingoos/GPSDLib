using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GPSD.Library.Exceptions;

namespace GPSD.Library.Models
{
    [DataContract]
    public class DataClassType
    {
        public static Dictionary<string, Type> TypeDictionary = new Dictionary<string, Type>
        {
            {"VERSION", typeof(GpsdVersion)},
            {"DEVICES", typeof(GpsDevice)},
            {"WATCH", typeof(GpsOptions)},
            {"TVP", typeof(GpsLocation)}
        };

        [DataMember(Name = "class")]
        public string Class { get; set; }

        public Type GetClassType()
        {
            Type result;
            TypeDictionary.TryGetValue(Class, out result);

            if (result == null)
            {
                throw new UnknownTypeException();
            }

            return result;
        }
    }
}
