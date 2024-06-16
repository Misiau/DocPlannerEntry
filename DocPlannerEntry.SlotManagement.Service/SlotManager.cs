using DocPlannerEntry.Shared;
using DocPlannerEntry.SlotManagement.Model.Availability;

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

    public async Task<bool> TakeSlot(string startHour)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var sb = new StringBuilder();

        sb.Append(_settings.BaseUrl);
        sb.Append(_settings.TakeSlotUrl);

        var request = new HttpRequestMessage(HttpMethod.Post, sb.ToString())
        {
            Content = new StringContent(sb.ToString())
        };

        request.Headers.Authorization = new BasicAuthenticationHeaderValue(_settings.UserName, _settings.Password);

        var response = await httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Slot reservation API returned {0}", response.StatusCode);
            return false;
        }

        return true;
    }

    public async Task<IEnumerable<Slot>> CalculateAvailableSlots(Dictionary<string, DayInfo> weeklySchedule, DateTime referenceDate, int slotDuration)
    {
        //This should be moved as a separate service

        var slots = new List<Slot>();

        foreach (var day in weeklySchedule.Values)
        {
            var workPeriod = day.WorkPeriod;

            //Is that approach efficient?
            //StartHour - LunchStartHour
            for (DateTime appointment = referenceDate.AddHours(workPeriod.StartHour); appointment < referenceDate.AddHours(workPeriod.LunchStartHour); appointment = appointment.AddMinutes(slotDuration))
                slots.Add(new Slot() { Start = appointment, End = appointment.AddMinutes(slotDuration) });

            //LunchEndHour - EndHour
            for (DateTime appointment = referenceDate.AddHours(workPeriod.LunchEndHour); appointment < referenceDate.AddHours(workPeriod.EndHour); appointment = appointment.AddMinutes(slotDuration))
                slots.Add(new Slot() { Start = appointment, End = appointment.AddMinutes(slotDuration) });

            referenceDate = referenceDate.AddDays(1);
        }

        throw new NotImplementedException();
    }

    public async Task<AvailabilityResponse> RetrieveAvailabilityAsync(DateTime? requestedDate = null)
    {
        //Will most likely cause some issues over time zones, look into this later to convert into DateTimeOffset
        var targetDate = requestedDate ?? DateTime.Now;

        //This behaves accordingly to week starting from Sunday, which some parts of the world consider as last day of week, other the first one. For sake of reduced complexity, Sunday is taken here as first.
        var mondayDate = targetDate.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);

        var httpClient = _httpClientFactory.CreateClient();

        var sb = new StringBuilder();

        sb.Append(_settings.BaseUrl);
        sb.Append(String.Format(_settings.AvailabilityUrl, mondayDate.ToString("yyyyMMdd")));

        var request = new HttpRequestMessage(HttpMethod.Get, sb.ToString());

        request.Headers.Authorization = new BasicAuthenticationHeaderValue(_settings.UserName, _settings.Password);

        var response = await httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        var availability = JsonSerializer.Deserialize<AvailabilityResponse>(responseBody);

        foreach (KeyValuePair<string, JsonElement> kvp in availability.Unserialized)
            availability.Days.Add(kvp.Key, JsonSerializer.Deserialize<DayInfo>(kvp.Value));

        await CalculateAvailableSlots(availability.Days, DateTime.Today, 10);

        return availability;
    }
}
