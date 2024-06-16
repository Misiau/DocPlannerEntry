using System.Text.Json.Serialization;

namespace DocPlannerEntry.SlotManagement.Model.Availability;
public class WorkPeriod
{
    [JsonPropertyName("StartHour")]
    public int StartHour { get; set; }

    [JsonPropertyName("EndHour")]
    public int EndHour { get; set; }

    [JsonPropertyName("LunchStartHour")]
    public int LunchStartHour { get; set; }

    [JsonPropertyName("LunchEndHour")]
    public int LunchEndHour { get; set; }
}
