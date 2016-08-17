using System;
using System.Runtime.Serialization;

namespace Ghostware.GPSDLib.Models
{
    [DataContract]
    public class GpsLocation
    {
        [DataMember(Name = "tag")]
        public string Tag { get; set; }

        [DataMember(Name = "device")]
        public string Device { get; set; }

        [DataMember(Name = "mode")]
        public int Mode { get; set; }

        [DataMember(Name = "time")]
        public DateTime Time { get; set; }

        [DataMember(Name = "ept")]
        public float Ept { get; set; }

        [DataMember(Name = "lat")]
        public double Latitude { get; set; }

        [DataMember(Name = "lon")]
        public double Longitude { get; set; }

        [DataMember(Name = "track")]
        public float Track { get; set; }

        [DataMember(Name = "speed")]
        public float Speed { get; set; }

        public override string ToString()
        {
            return $"Tag: {Tag} - Device: {Device} - Mode: {Mode} - Time: {Time} - Latitude: {Latitude} - Longitude: {Longitude} - Track: {Track} - Speed: {Speed}";
        }
    }
}
