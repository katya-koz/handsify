using Handsify.Models;
using Microsoft.AspNetCore.Mvc;

namespace Handsify.Controllers
{
    public class ViewModeController : Controller
    {
        [HttpGet("view-mode")]
        public IActionResult ViewModeIndex()
        {

            PageInfoModel pageInfoModel = new PageInfoModel();
            NavbarModel navbar = HttpContext.Session.GetObjectFromJson<NavbarModel>("NavbarModel");
            navbar.SelectedModeIndex = 0; // this is the index representing view mode
            pageInfoModel.Navbar = navbar;
            LoggedInUserModel user = HttpContext.Session.GetObjectFromJson<LoggedInUserModel>("LoggedInUser");
            pageInfoModel.LoggedInUser = user;

            /*  Pod initialPod = new Pod(
                 "test",  // Use double quotes for strings
                 new List<HHStation>()
              );
              pageInfoModel.Pod = initialPod;*/
            //HttpContext.Session.SetObjectAsJson("
            pageInfoModel.Pod = new Pod();
            return View("ViewMode", pageInfoModel);
        }



        public IActionResult ViewStationInfo(int key)
        {
            Pod currentPod = HttpContext.Session.GetObjectFromJson<Pod>("CurrentPod");
            //Console.WriteLine(("we are in the view mode controller");
            if (currentPod.HHStations.ContainsKey(key))
            {
               
                HHStation selectedStation = currentPod.HHStations[key];
                return PartialView("_StationViewPartial", selectedStation);
            }
            else
            {
                return NoContent();
            }
            
            
        }
    }
}
