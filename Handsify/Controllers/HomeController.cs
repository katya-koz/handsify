using Handsify.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Linq;
//using System.Text.Json;
using System.Reflection.Metadata.Ecma335;
using Handsify.utils;

namespace Handsify.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string podsRoot;
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly APIClient _apiClient;
        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment appEnvironment, APIClient apiClient)
        {
            _appEnvironment = appEnvironment;
            podsRoot = appEnvironment.WebRootPath + "\\Maps";
            _apiClient = apiClient;
            _logger = logger;
        }

        public IActionResult Home()
        {

            NavbarModel navbar = new NavbarModel();
            navbar.AvailableUnitsForFloor = GetAvailableUnitsByFloor(NavbarModel.Floors[navbar.SelectedFloorIndex]);
            var jwtToken = Request.Cookies["jwtToken"];
            if (jwtToken == null)
            {
                //Console.WriteLine(("jwtToken cookie not found!");
                throw new Exception("Token isn't properly assigned");
            }

            LoggedInUserModel user = new LoggedInUserModel(jwtToken);

            // NavbarModel navbarModel = new NavbarModel();
            PageInfoModel pageInfoModel = new PageInfoModel();
            pageInfoModel.Navbar = navbar;
            pageInfoModel.LoggedInUser= user;
            HttpContext.Session.SetObjectAsJson("LoggedInUser", user);

            //Console.WriteLine(("in the home controller the user is: "+user.ToString());
            HttpContext.Session.SetObjectAsJson("NavbarModel", navbar);

            return View(pageInfoModel);
        }




        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost("update-selected-drop-menu-index")]
        public IActionResult UpdateSelectedIndex([FromBody] UpdateDropdownRequest request)
        {
            ////Console.WriteLine(("request item: " + request.Item);
            // Retrieve the current Navbar model from the session
            NavbarModel navbar = HttpContext.Session.GetObjectFromJson<NavbarModel>("NavbarModel");

            switch (request.MenuIdentity)
            {
                case "mode":
                    navbar.SelectedModeIndex = NavbarModel.Modes.FirstOrDefault(n => n.Value == request.Item).Key;
                    /*return RedirectToAction(navbar.Modes[request.Index] + "ModeIndex", navbar.Modes[request.Index]+"Mode");*/
                    break;
                case "floor":
                    navbar.SelectedFloorIndex = NavbarModel.Floors.FirstOrDefault(n => n.Value == request.Item).Key;
                    List<string> availableUnits = GetAvailableUnitsByFloor(request.Item);
                    navbar.AvailableUnitsForFloor = availableUnits;
                   // switch unit if its not available anymore
                    if (!(navbar.SelectedUnitIndex > -1 && navbar.AvailableUnitsForFloor.Contains(NavbarModel.Units[navbar.SelectedUnitIndex])))
                    {
                        navbar.AvailableUnitsForFloor.Sort();
                        string firstUnit = navbar.AvailableUnitsForFloor.First();
                        navbar.SelectedUnitIndex = NavbarModel.Units.FirstOrDefault(x => x.Value == firstUnit).Key;
                    }
                    break;
                case "unit":
                    navbar.SelectedUnitIndex = NavbarModel.Units.FirstOrDefault(n => n.Value == request.Item).Key;
                    break;
                default:
                    return BadRequest(new { error = "The menu identity defined is not in the list of correct identites: mode, floor, or unit." }); 
            }
            HttpContext.Session.SetObjectAsJson("NavbarModel", navbar);
            return Ok(new { message = $"Successfully updated {request.MenuIdentity} to {request.Item}. Here is the updated NavBar model: {navbar.ToString()}", updatedUnit = NavbarModel.Units[navbar.SelectedUnitIndex] });
        }

        [HttpGet("get-units")]
        public JsonResult GetAvailibleUnits()
        {
            NavbarModel navbar = HttpContext.Session.GetObjectFromJson<NavbarModel>("NavbarModel");
            return Json(navbar.AvailableUnitsForFloor);
        }

        [HttpGet("get-pod-map")]
        public JsonResult GetPodMap()
        {
            NavbarModel navbar = HttpContext.Session.GetObjectFromJson<NavbarModel>("NavbarModel");
            string podMapFileLocation = podsRoot + "L" + NavbarModel.Floors[navbar.SelectedFloorIndex] + "\\P" + NavbarModel.Units[navbar.SelectedUnitIndex] + ".svg"; 

            return Json(new
            {
                podMapLocation = podMapFileLocation,
                pod = NavbarModel.Units[navbar.SelectedUnitIndex],
                floor = NavbarModel.Floors[navbar.SelectedFloorIndex],
                message = $"Pod map: Level {NavbarModel.Floors[navbar.SelectedFloorIndex]}, Unit {NavbarModel.Units[navbar.SelectedUnitIndex]}"
            });
        }


        /* [HttpGet("get-units")]*/
        public List<string> GetAvailableUnitsByFloor(string floor)
        {
            List<string> availableUnits = new List<string>();

            string levelFolderPath = Path.Combine(podsRoot, "L" + floor);
            
            if (Directory.Exists(levelFolderPath))
            {
                string[] files = Directory.GetFiles(levelFolderPath, "*.svg");

                foreach (var file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);

                    
                    if (fileName.Length > 1)
                    {
                        string letter = fileName.Substring(1, 1);
                        availableUnits.Add(letter);
                    }
                }
            }


            return availableUnits;
        }

        [HttpGet("get-pod")]
        public async Task<string> GetPod()
        {
            NavbarModel navbar = HttpContext.Session.GetObjectFromJson<NavbarModel>("NavbarModel");
            string podKey = NavbarModel.Floors[navbar.SelectedFloorIndex] + NavbarModel.Units[navbar.SelectedUnitIndex];
            //Pod pod = HttpContext.Session.GetObjectFromJson<Pod>(podKey);

            //if ( pod == null) // if not cached, get from API
            //{
            Pod pod = await _apiClient.GetPod(NavbarModel.Floors[navbar.SelectedFloorIndex], NavbarModel.Units[navbar.SelectedUnitIndex]);

            //}

            HttpContext.Session.SetObjectAsJson(podKey, pod);
            HttpContext.Session.SetObjectAsJson("CurrentPod", pod);
            ////Console.WriteLine(("pod for floor: " + NavbarModel.Floors[navbar.SelectedFloorIndex] + " for unit: " + NavbarModel.Units[navbar.SelectedUnitIndex]);
            //Console.WriteLine((JsonConvert.SerializeObject(pod));

            return JsonConvert.SerializeObject(pod);
        }

        [HttpPost("load-station-info-partial")]
        public IActionResult LoadStationInfoPartialView([FromBody] int stationKey)
        {

            bool editing = false;

            if (Request.Headers.TryGetValue("editing", out var editingHeaderValue))
            {
                bool.TryParse(editingHeaderValue.ToString(), out editing); // editing stays false if parsing fails
            }

            Console.WriteLine(editing);
            NavbarModel navbar = HttpContext.Session.GetObjectFromJson<NavbarModel>("NavbarModel"); // check which mode 

             if(NavbarModel.Modes[navbar.SelectedModeIndex] == "Edit")
            {
                if (!editing)
                {
                    return RedirectToAction("EditStationInfo", "EditMode", new { key = stationKey });
                }
                else
                {
                    return NoContent(); // if the user is currently editing, we dotn wnat to update the view
                }
            }

             else return RedirectToAction("ViewStationInfo", "ViewMode", new { key = stationKey });
            
        }

        [HttpGet("get-current-navigation")]
        public JsonResult GetNavigation()
        {
            NavbarModel navbar = HttpContext.Session.GetObjectFromJson<NavbarModel>("NavbarModel"); // check which mode 
            if(navbar.SelectedModeIndex > 0)
            {
                return Json(new
                            {
                                selectedMode = NavbarModel.Modes[navbar.SelectedModeIndex],
                                selectedFloor = NavbarModel.Floors[navbar.SelectedFloorIndex],
                                selectedUnit = NavbarModel.Units[navbar.SelectedUnitIndex]
                            }
                );
            }
            else
            {
                return Json(new
                {
                    selectedMode = "Default",
                    selectedFloor = NavbarModel.Floors[navbar.SelectedFloorIndex],
                    selectedUnit = NavbarModel.Units[navbar.SelectedUnitIndex]
                });
            }
            
            
        }
    }


    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }

    public class UpdateDropdownRequest
    {
        public string Item { get; set; }
        public string MenuIdentity { get; set; }
    }




}