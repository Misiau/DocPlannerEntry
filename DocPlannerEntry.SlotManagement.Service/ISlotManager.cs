using DocPlannerEntry.SlotManagement.Model.Availability;
using DocPlannerEntry.SlotManagement.Model.TakeSlot;

namespace DocPlannerEntry.SlotManagement.Service;
public interface ISlotManager
{
    public Task<IEnumerable<Slot>> GetAvailableSlots(DateTimeOffset? requestedDate = null);
    public Task<(bool, string)> TakeSlot(Guid facilityId, DateTimeOffset startDate, DateTimeOffset endDate, string comments, Patient patientData);
}
