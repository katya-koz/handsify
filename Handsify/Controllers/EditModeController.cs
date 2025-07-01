using Handsify.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Newtonsoft;
using Newtonsoft.Json;
using Handsify.utils;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace Handsify.Controllers
{
    public class EditModeController : Controller
    {
        private readonly APIClient _apiClient;
        public EditModeController(APIClient apiClient)
        {
            _apiClient = apiClient;
        }
        [HttpGet("edit-mode")]
        public IActionResult EditModeIndex()
        {
            PageInfoModel pageInfoModel = new PageInfoModel();
            NavbarModel navbar = HttpContext.Session.GetObjectFromJson<NavbarModel>("NavbarModel");
            navbar.SelectedModeIndex = 1; // this is the index representing edit mode
            LoggedInUserModel user = HttpContext.Session.GetObjectFromJson<LoggedInUserModel>("LoggedInUser");
            pageInfoModel.LoggedInUser = user;

            pageInfoModel.Navbar = navbar;
            /*Pod initialPod = new Pod(
               "test",  
               new List<HHStation>()
            );*/
            // pageInfoModel.Pod = initialPod;
            pageInfoModel.Pod = new Pod();
            return View("EditMode",pageInfoModel);
        }



        public IActionResult EditStationInfo(int key)
        {

            Pod currentPod = HttpContext.Session.GetObjectFromJson<Pod>("CurrentPod");
            if (currentPod.HHStations.ContainsKey(key))
            {
                HHStation selectedStation = currentPod.HHStations[key];
                return PartialView("_StationEditPartial", selectedStation);
            }
            else
            {
                return RedirectToAction("ShowAddStation");
            }
            

            /*//Console.WriteLine(("1. This the le station requested on partial view: " + selectedStation.StationName);*/

          

        }

        [HttpPost("update-station")]
        public async Task<IActionResult> UpdateStation([FromBody] UpdateStationRequest request)
        {

            ////Console.WriteLine((request);
            NavbarModel navbar = HttpContext.Session.GetObjectFromJson<NavbarModel>("NavbarModel");
            // Retrieve the current Pod from the session
            Pod currentPod = HttpContext.Session.GetObjectFromJson<Pod>("CurrentPod");

            
            // Update the Pod's stations
            currentPod.HHStations = request.Stations;

            LoggedInUserModel user = HttpContext.Session.GetObjectFromJson<LoggedInUserModel>("LoggedInUser");

            foreach (Note note in request.NewNotes)
            {
                note.Author = user.ADUser;
            }


            await _apiClient.UpsertStation(currentPod.HHStations[request.StationKey], request.NewNotes,request.ArchivedNotes, int.Parse(NavbarModel.Floors[navbar.SelectedFloorIndex]), NavbarModel.Units[navbar.SelectedUnitIndex]);

            //get notes after new notes have been assigned keys by db in sql server
          /*  currentPod.HHStations[request.StationKey].Notes = await _apiClient.GetNotesByStationKey(request.StationKey + "");*/

            currentPod = await _apiClient.GetPod(NavbarModel.Floors[navbar.SelectedFloorIndex], NavbarModel.Units[navbar.SelectedUnitIndex]);

            HttpContext.Session.SetObjectAsJson("CurrentPod", currentPod);


            if (request.Mode == "operation") return PartialView("/Views/OperationMode/_OperationEditPartial.cshtml", currentPod.HHStations[request.StationKey]);
            //Console.WriteLine(("here");

            return PartialView("_StationEditPartial", currentPod.HHStations[request.StationKey]);

        }

        [HttpPost("add-station")]
        public async Task<IActionResult> AddStation([FromBody] HHStation NewStation)
        {
            

            NavbarModel navbar = HttpContext.Session.GetObjectFromJson<NavbarModel>("NavbarModel");

            await _apiClient.AddStation(NewStation, int.Parse(NavbarModel.Floors[navbar.SelectedFloorIndex]), NavbarModel.Units[navbar.SelectedUnitIndex]);

            var currentPod = await _apiClient.GetPod(NavbarModel.Floors[navbar.SelectedFloorIndex], NavbarModel.Units[navbar.SelectedUnitIndex]);

            HttpContext.Session.SetObjectAsJson("CurrentPod", currentPod);

            


            return PartialView("_StationEditPartial", currentPod.HHStations.Values.Where(n => n.StationID == NewStation.StationID).First());

        }
        [HttpPost("archive-station")]
        public async Task<string> ArchiveStation([FromBody] string stationKey)
        {
            //Console.WriteLine(("in archive statio in edit mode controlelr");
            var response = await _apiClient.ArchiveStation(stationKey);
            return response;

        }
        [HttpGet("add-station-partial")]
        public async Task<IActionResult> ShowAddStation()
        {
            Console.WriteLine("show add station");
            NavbarModel navbar = HttpContext.Session.GetObjectFromJson<NavbarModel>("NavbarModel"); // check which mode 

            if (NavbarModel.Modes[navbar.SelectedModeIndex] == "Edit")
            { return PartialView("_AddStationPartial"); }

            return NoContent();

            
        }
    }
    public class UpdateStationRequest
    {
        public string? Mode {get;set;}
        public Dictionary<int, HHStation> Stations  { get; set; }
        public int StationKey { get; set; }
        public List<Note> NewNotes { get; set; }
        public List<int> ArchivedNotes { get; set; }
    }
/*
    public class AddStationRequest
    {
        public HHStation NewStation { get; set; }
    }
*/

}
