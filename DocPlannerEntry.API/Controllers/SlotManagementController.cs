using DocPlannerEntry.SlotManagement.Model.Availability;
using DocPlannerEntry.SlotManagement.Service;

using Microsoft.AspNetCore.Mvc;

namespace DocPlannerEntry.API.Controllers;

[ApiController]
[Route("[controller]")]
public class SlotManagementController : ControllerBase
{
    private readonly ILogger<SlotManagementController> _logger;
    private readonly ISlotManager _slotManager;

    public SlotManagementController(ILogger<SlotManagementController> logger, ISlotManager slotManager)
    {
        _logger = logger;
        _slotManager = slotManager;
    }

    [HttpGet(Name = "GetAvailability")]
    public async Task<IEnumerable<Slot>> Get()
    {
        var result = await _slotManager.RetrieveAvailabilityAsync(DateTime.Now);

        throw new NotImplementedException();
    }
}
