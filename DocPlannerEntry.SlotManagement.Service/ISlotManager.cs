using DocPlannerEntry.SlotManagement.Model.Availability;

namespace DocPlannerEntry.SlotManagement.Service;
public interface ISlotManager
{
    public Task<AvailabilityResponse> RetrieveAvailabilityAsync(DateTime? requestedDate);
}
