using DocPlannerEntry.SlotManagement.Model.Availability;
using DocPlannerEntry.SlotManagement.Model.TakeSlot;
using DocPlannerEntry.SlotManagement.Service;

using Microsoft.AspNetCore.Mvc;

namespace DocPlannerEntry.API.Controllers;

[ApiController]
[Route("api/[controller]")]

/// <summary>
/// The main controller exposed for front-end to get available ones and reserve them
/// </summary>
public class SlotManagementController : ControllerBase
{
    private readonly ILogger<SlotManagementController> _logger;
    private readonly ISlotManager _slotManager;

    public SlotManagementController(ILogger<SlotManagementController> logger, ISlotManager slotManager)
    {
        _logger = logger;
        _slotManager = slotManager;
    }

    /// <summary>
    /// Method used to retrieve all currently available slots for whole week
    /// </summary>
    /// <param name="requestedDate">Date to pull weekly availability (Sunday being the first day of the week)</param>
    /// <returns>Returns all available slots for that week</returns>
    /// <response code="200">Returns all available slots for that week</response>
    [HttpGet(Name = "GetAvailability")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Slot>>> GetAvailableSlots(string requestedDate = null)
    {
        _logger.LogInformation("Started processing GetAvailableSlots");

        IEnumerable<Slot> availableSlots = Enumerable.Empty<Slot>();

        DateTimeOffset dto;
        if (!DateTimeOffset.TryParse(requestedDate, out dto))
            dto = DateTimeOffset.Now;

        try
        {
            availableSlots = await _slotManager.GetAvailableSlots(dto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogInformation("Authorization has not been set properly");

            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not retrieve available slots due to {0}", ex);

            return BadRequest("Could not authorize to slot management API");
        }

        _logger.LogInformation("Finished processing GetAvailableSlots");

        return Ok(availableSlots);
    }

    /// <summary>
    /// Method used to reserve a set slot
    /// </summary>
    /// <param name="startDate">Date of slot starting time point</param>
    /// <param name="endDate">Date of slot ending time point</param>
    /// <returns>Response if slot reservation was successful with errors describing the reason if not</returns>
    /// <response code="200">Slot has been reserved successfully</response>
    /// <response code="400">Slot could not be reserved due to a reason said in response</response>
    [HttpPost(Name = "TakeSlot")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<(bool, string)>> TakeSlot(SlotReservationRequest request)
    {
        _logger.LogInformation("Started processing TakeSlot");

        (bool, string) result;

        try
        {
            result = await _slotManager.TakeSlot(request.FacilityId, request.Start, request.End, request.Comments, request.Patient);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogInformation("Authorization has not been set properly");

            return BadRequest("Could not authorize to slot management API");
        }
        catch (Exception ex)
        {
            _logger.LogError("Slot could not be reserved due to {0}", ex);

            return BadRequest($"Slot could not be reserved {ex}");
        }

        _logger.LogInformation("Finished processing TakeSlot");

        return Ok(result.Item2);
    }
}
