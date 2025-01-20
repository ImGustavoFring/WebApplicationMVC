using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplicationMVC.Services;

namespace WebApplicationMVC.Controllers
{
    [Authorize(Policy = "AdminPolicy")]
    [Route("Visit")]
    public class VisitController : Controller
    {
        private readonly VisitTrackerService _visitTracker;

        public VisitController(VisitTrackerService visitTracker)
        {
            _visitTracker = visitTracker;
        }

        [HttpGet("GetVisits")]
        public IActionResult GetVisits()
        {
            return View(_visitTracker.GetMonthlyVisits());
        }
    }
}
