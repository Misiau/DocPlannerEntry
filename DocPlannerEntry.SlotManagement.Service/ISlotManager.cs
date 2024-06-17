using DocPlannerEntry.SlotManagement.Model.Availability;

namespace DocPlannerEntry.SlotManagement.Service;
public interface ISlotManager
{
    public Task<IEnumerable<Slot>> GetAvailableSlots(DateTimeOffset? requestedDate = null);
    public Task<(bool, string)> TakeSlot(DateTimeOffset startDate, DateTimeOffset endDate);
}
