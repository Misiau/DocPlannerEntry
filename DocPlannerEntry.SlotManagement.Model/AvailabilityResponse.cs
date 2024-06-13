using System.Text.Json.Serialization;

namespace DocPlannerEntry.SlotManagement.Model;

public class AvailabilityResponse
{
    [JsonPropertyName("Facility")]
    public Facility Facility { get; set; }

    [JsonPropertyName("SlotDurationMinutes")]
    public int SlotDurationMinutes { get; set; }

    public Dictionary<string, WorkPeriod> Days { get; set; }
}