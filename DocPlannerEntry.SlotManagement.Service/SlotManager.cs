using DocPlannerEntry.Shared;
using DocPlannerEntry.SlotManagement.Model.Availability;
using DocPlannerEntry.SlotManagement.Model.TakeSlot;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Text;
using System.Text.Json;

namespace DocPlannerEntry.SlotManagement.Service;

public class SlotManager : ISlotManager
{
    private readonly SlotServiceSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SlotManager> _logger;

    public SlotManager(IOptions<SlotServiceSettings> settings, IHttpClientFactory httpClientFactory, ILogger<SlotManager> logger)
    {
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<(bool, string)> TakeSlot(Guid facilityId, DateTimeOffset startDate, DateTimeOffset endDate, string comments, Patient patientData)
    {
        if (startDate > endDate)
            return (false, "Slot has to start before ending");

        if (patientData is null)
            return (false, "Patient data cannot be empty");

        var patientValidator = new PatientValidator();

        var validationResult = await patientValidator.ValidateAsync(patientData);

        if (!validationResult.IsValid)
            return (false, string.Join(",", validationResult.Errors.Select(x => x.ErrorMessage)));

        //Might be worth checking, considering as an overkill for the purpose of this task
        //if (startDate.AddMinutes(slotDuration) != endDate)
        //    return (false, "Slot duration misaligned with start/date");

        var httpClient = _httpClientFactory.CreateClient();

        var sb = new StringBuilder();

        sb.Append(_settings.BaseUrl);
        sb.Append(_settings.TakeSlotUrl);


        var slotReservationRequest = new SlotReservationRequest()
        {
            FacilityId = facilityId,
            Start = startDate,
            End = endDate,
            Comments = comments,
            Patient = patientData
        };

        var request = new HttpRequestMessage(HttpMethod.Post, sb.ToString())
        {
            Content = new StringContent(JsonSerializer.Serialize(slotReservationRequest), Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new BasicAuthenticationHeaderValue(_settings.UserName, _settings.Password);

        var response = await httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogDebug("Slot reservation API returned {0}. Original response: {1}", response.StatusCode, responseBody);
            return (false, $"Slot reservation API returned {response.StatusCode}");
        }

        return (true, "Slot has been reserved successfully");
    }

    public async Task<IEnumerable<Slot>> GetAvailableSlots(DateTimeOffset? requestedDate = null)
    {
        var targetDate = requestedDate ?? DateTimeOffset.Now;

        var availability = await RetrieveAvailabilityAsync(targetDate);

        if (availability is null)
        {
            _logger.LogError("Response from third party API was not successful");
            return Enumerable.Empty<Slot>();
        }

        var possibleSlots = CalculateAvailableSlots(availability.Days, targetDate.Date, availability.SlotDurationMinutes).ToList();
        var busySlots = availability.Days.Where(x => x.Value.BusySlots != null).SelectMany(x => x.Value.BusySlots).ToList();

        var availableSlots = possibleSlots.Except(busySlots, new SlotEqualityComparer()).ToList();

        return availableSlots;
    }

    private IEnumerable<Slot> CalculateAvailableSlots(Dictionary<string, DayInfo> weeklySchedule, DateTime referenceDate, int slotDuration)
    {
        var slots = new List<Slot>();

        foreach (var day in weeklySchedule.Values)
        {
            //Edge-case due to API bug, which allows for slot reservation on a day that has not been listed before
            if (day.WorkPeriod is null)
                continue;

            var workPeriod = day.WorkPeriod;

            //StartHour - LunchStartHour
            for (DateTime appointment = referenceDate.AddHours(workPeriod.StartHour); appointment < referenceDate.AddHours(workPeriod.LunchStartHour); appointment = appointment.AddMinutes(slotDuration))
                slots.Add(new Slot() { Start = appointment, End = appointment.AddMinutes(slotDuration) });

            //LunchEndHour - EndHour
            for (DateTime appointment = referenceDate.AddHours(workPeriod.LunchEndHour); appointment < referenceDate.AddHours(workPeriod.EndHour); appointment = appointment.AddMinutes(slotDuration))
                slots.Add(new Slot() { Start = appointment, End = appointment.AddMinutes(slotDuration) });

            referenceDate = referenceDate.AddDays(1);
        }

        return slots;
    }

    private async Task<AvailabilityResponse> RetrieveAvailabilityAsync(DateTimeOffset requestedDate)
    {
        //This behaves accordingly to week starting from Sunday, which some parts of the world consider as last day of week, other the first one. For sake of reduced complexity, Sunday is taken here as first.
        var mondayDate = requestedDate.AddDays(-(int)requestedDate.DayOfWeek + (int)DayOfWeek.Monday);

        var httpClient = _httpClientFactory.CreateClient();

        var sb = new StringBuilder();

        sb.Append(_settings.BaseUrl);
        sb.Append(String.Format(_settings.AvailabilityUrl, mondayDate.ToString("yyyyMMdd")));

        var request = new HttpRequestMessage(HttpMethod.Get, sb.ToString());

        request.Headers.Authorization = new BasicAuthenticationHeaderValue(_settings.UserName, _settings.Password);

        try
        {

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
        }
        catch (JsonException ex)
        {
            _logger.LogError("Could not deserialize retrieved JSON {0}", ex);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error occured during sending payload", ex);
            return null;
        }

        if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(responseBody))
            return null;

        AvailabilityResponse availability = null;

        try
        {
            availability = JsonSerializer.Deserialize<AvailabilityResponse>(responseBody);
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not deserialize type: {0}, exception: {1}", typeof(AvailabilityResponse), ex);
            return null;
        }

        foreach (KeyValuePair<string, JsonElement> kvp in availability.Unserialized)
            availability.Days.Add(kvp.Key, JsonSerializer.Deserialize<DayInfo>(kvp.Value));

        return availability;
    }
}
