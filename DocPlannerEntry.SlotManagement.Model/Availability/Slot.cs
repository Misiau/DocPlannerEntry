using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace DocPlannerEntry.SlotManagement.Model.Availability;

public class Slot
{
    [JsonPropertyName("Start")]
    public DateTimeOffset Start { get; set; }

    [JsonPropertyName("End")]
    public DateTimeOffset End { get; set; }
}

public class SlotEqualityComparer : IEqualityComparer<Slot>
{
    public bool Equals(Slot? x, Slot? y)
    {
        if (x.Start == y.Start &
            x.End == y.End)
            return true;
        return false;
    }

    public int GetHashCode([DisallowNull] Slot obj)
    {
        unchecked
        {
            if (obj == null)
                return 0;
            int hashCode = obj.Start.GetHashCode();
            hashCode = (hashCode * 397) ^ obj.Start.GetHashCode();
            return hashCode;
        }
    }
}