using System.Text.Json.Serialization;

namespace DocPlannerEntry.SlotManagement.Model;

public class BusySlot
{
    [JsonPropertyName("Start")]
    public DateTime Start { get; set; }

    [JsonPropertyName("End")]
    public DateTime End { get; set; }
}