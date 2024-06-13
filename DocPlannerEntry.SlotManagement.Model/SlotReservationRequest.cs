using System.Text.Json.Serialization;

namespace DocPlannerEntry.SlotManagement.Model;

internal class SlotReservationRequest
{
    [JsonPropertyName("Start")]
    public DateTimeOffset Start { get; set; }

    [JsonPropertyName("End")]
    public DateTimeOffset End { get; set; }

    [JsonPropertyName("Comments")]
    public string Comments { get; set; }

    [JsonPropertyName("Patient")]
    public Patient Patient { get; set; }
}