using System.Runtime.Serialization;

namespace GPSD.Library
{
    [DataContract]
    public class GpsdData
    {
        [DataMember(Name = "class")]
        public string Class { get; set; }

        [DataMember(Name = "release")]
        public string Release { get; set; }

        [DataMember(Name = "rev")]
        public string Rev { get; set; }

        [DataMember(Name = "proto_major")]
        public int ProtoMajor { get; set; }

        [DataMember(Name = "proto_minor")]
        public int ProtoMinor { get; set; }

        public override string ToString()
        {
            return $"{Class} - Release: {Release} - Revision: {Rev} - ProtoMajor: {ProtoMajor} - ProtoMinor: {ProtoMinor}";
        }
    }
}
