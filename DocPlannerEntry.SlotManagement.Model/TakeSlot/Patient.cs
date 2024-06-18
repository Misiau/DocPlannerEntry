using FluentValidation;

using System.Text.Json.Serialization;

namespace DocPlannerEntry.SlotManagement.Model.TakeSlot;

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

public class PatientValidator : AbstractValidator<Patient>
{
    public PatientValidator()
    {
        RuleFor(p => p.Name).NotEmpty();
        RuleFor(p => p.SecondName).NotEmpty();
        RuleFor(p => p.Email).NotEmpty().EmailAddress();
        RuleFor(p => p.Phone).NotEmpty();
    }
}

