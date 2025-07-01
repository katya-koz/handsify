using Handsify.Models;
using Handsify.utils;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Handsify.Controllers
{
    public class OperationModeController : Controller
    {
        private readonly ILogger<OperationModeController> _logger;
        private readonly APIClient _apiClient;

        public OperationModeController(ILogger<OperationModeController> logger, APIClient apiClient)
        {

            _apiClient = apiClient;
            _logger = logger;

        }
        [HttpGet("operation-mode")]
        public IActionResult OperationModeIndex()
        {

            PageInfoModel pageInfoModel = new PageInfoModel();
            NavbarModel navbar = HttpContext.Session.GetObjectFromJson<NavbarModel>("NavbarModel");
            LoggedInUserModel user = HttpContext.Session.GetObjectFromJson<LoggedInUserModel>("LoggedInUser");

            navbar.SelectedModeIndex = 2; // this is the index representing operation mode
            pageInfoModel.Navbar = navbar;
   
            pageInfoModel.Pod = new Pod();
            pageInfoModel.LoggedInUser = user;
            return View("OperationMode", pageInfoModel);
        }
        [HttpGet("load-operation-station-partial")]
        public IActionResult LoadStationPartial(int stationKey)
        {
            Pod currentPod = HttpContext.Session.GetObjectFromJson<Pod>("CurrentPod");
            HHStation station = currentPod.HHStations[stationKey]; // however you retrieve it
            
            return PartialView("_OperationEditPartial", station);
        }

        [HttpGet("get-operational-pod")]
        public async Task<string> GetPod()
        {
            NavbarModel navbar = HttpContext.Session.GetObjectFromJson<NavbarModel>("NavbarModel");
            string podKey = NavbarModel.Floors[navbar.SelectedFloorIndex] + NavbarModel.Units[navbar.SelectedUnitIndex];
           
            Pod pod = await _apiClient.GetOperationalPod(NavbarModel.Floors[navbar.SelectedFloorIndex], NavbarModel.Units[navbar.SelectedUnitIndex]);

            HttpContext.Session.SetObjectAsJson(podKey, pod);
            HttpContext.Session.SetObjectAsJson("CurrentPod", pod);
         

            return JsonConvert.SerializeObject(pod);
        }
    }
}

