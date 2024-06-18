using System.Text.Json;
using System.Text.Json.Serialization;

namespace DocPlannerEntry.Shared;

//This class has been created due to System.Text.Json treating receiving DateTimes without Z symbol as ISO-compliant and parsing them to local time zone instead. To ensure DateTimeOffsets deserialized from set format this has to be used by default
//Slot API handles DateTimes with T format, this means without any additional information around that, an assumption is made for that to be treated as UTC (this is why this dirty workaround is set here, even though all hours and handled as DateTimeOffset in this demo)
public class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));
    }
}

