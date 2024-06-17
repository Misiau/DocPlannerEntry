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

    public async Task<(bool, string)> TakeSlot(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        if (startDate > endDate)
            return (false, "Slot has to start before ending");

        //Might be worth checking, considering as an overkill for the purpose of this task
        //if (startDate.AddMinutes(slotDuration) != endDate)
        //    return (false, "Slot duration misaligned with start/date");

        var httpClient = _httpClientFactory.CreateClient();

        var sb = new StringBuilder();

        sb.Append(_settings.BaseUrl);
        sb.Append(_settings.TakeSlotUrl);

        var slotReservationRequest = new SlotReservationRequest()
        {
            FacilityId = new Guid("90c9f71c-685f-48e7-a6d5-7898775209ce"),
            Start = startDate,
            End = endDate,
            Comments = "Awesome Patient incoming",
            Patient = new Patient()
            {
                Email = "testPatient@gmail.com",
                Name = "John",
                SecondName = "Connor",
                Phone = "000111222"
            }
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
            return (false, $"Slot reservation API returned {response.StatusCode}. Original response: {responseBody}");
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
        var busySlots = availability.Days.SelectMany(x => x.Value.BusySlots).ToList();

        var availableSlots = possibleSlots.Except(busySlots, new SlotEqualityComparer()).ToList();

        return availableSlots;
    }

    private IEnumerable<Slot> CalculateAvailableSlots(Dictionary<string, DayInfo> weeklySchedule, DateTime referenceDate, int slotDuration)
    {
        var slots = new List<Slot>();

        foreach (var day in weeklySchedule.Values)
        {
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
        var mondayDate = requestedDate.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);

        var httpClient = _httpClientFactory.CreateClient();

        var sb = new StringBuilder();

        sb.Append(_settings.BaseUrl);
        sb.Append(String.Format(_settings.AvailabilityUrl, mondayDate.ToString("yyyyMMdd")));

        var request = new HttpRequestMessage(HttpMethod.Get, sb.ToString());

        request.Headers.Authorization = new BasicAuthenticationHeaderValue(_settings.UserName, _settings.Password);

        var response = await httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

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
