using System.Text.Json.Serialization;

namespace DocPlannerEntry.SlotManagement.Model.Availability;

public class Slot
{
    [JsonPropertyName("Start")]
    public DateTime Start { get; set; }

    [JsonPropertyName("End")]
    public DateTime End { get; set; }
}