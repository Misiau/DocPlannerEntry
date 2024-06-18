using DocPlannerEntry.SlotManagement.Model.Availability;

using System.Collections.ObjectModel;

namespace DocPlannerEntry.UI;
public static class ExtensionMethods
{
    public static void AddRange(this ObservableCollection<SlotUI> destination, List<Slot> source)
    {
        foreach (var value in source)
            destination.Add(value.SlotToSlotUI());
    }
}
