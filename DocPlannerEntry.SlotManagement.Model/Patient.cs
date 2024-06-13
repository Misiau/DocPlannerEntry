using System.Text.Json.Serialization;

namespace DocPlannerEntry.SlotManagement.Model;

public class Patient
{
    [JsonPropertyName("Name")]
    public string Name { get; set; }

    [JsonPropertyName("SecondName")]
    public string SecondName { get; set; }

    [JsonPropertyName("Email")]
    public string Email { get; set; }

    [JsonPropertyName("Phone")]
    public string Phone { get; set; }
}

