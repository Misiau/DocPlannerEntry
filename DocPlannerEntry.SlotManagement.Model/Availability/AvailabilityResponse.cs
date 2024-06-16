using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DocPlannerEntry.SlotManagement.Model.Availability;

public class AvailabilityResponse
{
    [JsonPropertyName("Facility")]
    public Facility Facility { get; set; }

    [JsonPropertyName("SlotDurationMinutes")]
    public int SlotDurationMinutes { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> Unserialized { get; set; }
    public Dictionary<string, DayInfo> Days { get; set; } = new Dictionary<string, DayInfo>();
}