using DocPlannerEntry.SlotManagement.Model.Availability;

namespace DocPlannerEntry.UI;
public static class SlotExtensions
{
    public static SlotUI SlotToSlotUI(this Slot slot)
    {
        var dayOfWeek = slot.Start.DayOfWeek;

        var slotUI = new SlotUI()
        {
            DayOfWeek = dayOfWeek.ToString(),
            StartDate = slot.Start.ToString("f"),
            EndDate = slot.End.ToString("f")
        };

        return slotUI;
    }

    public static List<SlotUI> SlotToSlotUI(this List<Slot> slots)
    {
        var slotUIs = new List<SlotUI>();
        foreach (var slot in slots)
            slotUIs.Add(SlotToSlotUI(slot));

        return slotUIs;
    }
}
