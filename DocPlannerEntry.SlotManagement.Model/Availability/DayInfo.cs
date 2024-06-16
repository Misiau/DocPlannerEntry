namespace DocPlannerEntry.SlotManagement.Model.Availability;
public class DayInfo
{
    public WorkPeriod WorkPeriod { get; set; }
    public List<Slot> BusySlots { get; set; }
}
