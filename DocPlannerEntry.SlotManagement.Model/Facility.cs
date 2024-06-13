using System.Text.Json.Serialization;

namespace DocPlannerEntry.SlotManagement.Model;

public class Facility
{
    [JsonPropertyName("FacilityId")]
    public string FacilityId { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; }

    [JsonPropertyName("Address")]
    public string Address { get; set; }
}