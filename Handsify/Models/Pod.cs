using System;
using System.Drawing;

namespace Handsify.Models
{

    public class Pod
    {
        //public string PodMapLocation { get; set; }
        // public List<HHStation> HHStations { get; set; } = new List<HHStation>();

        public Dictionary<int, HHStation> HHStations { get; set; } = new Dictionary<int, HHStation>();

        //public Pod(string podMapLocation, List<HHStation> hhStations)
        //{
        //   // PodMapLocation = podMapLocation;
        //    HHStations = hhStations;

        //}

        public Pod()
        {

        }
        public override string ToString()
        {
            string stations = "";

            foreach (HHStation station in HHStations.Values)
            {

                stations += "\n" + station.StationID;

            }

            return stations;

        }
    

    public HHStation GetSelectedHHStation(int selectedStationID)
        {
            //Console.WriteLine(("selected id: " + selectedStationID);

            foreach (HHStation hhStation in HHStations.Values)
            {
                //Console.WriteLine((hhStation.StationName);
            }

            //get the current selected station, otherwise return an empty one (id is invalid, and will not be rendered)

            HHStation station = HHStations.Values.Where(s => s.StationID == selectedStationID).FirstOrDefault();

            if (station == null)
            {
                station = new HHStation();
            }

            //Console.WriteLine(("returnign this station: " + station.StationName);
            return station;

           /* List<Note> notes = new List<Note>();

            notes.Add(new Note("This is a test note.", new DateTime(2025,02,11), "JSmith"));
            notes.Add(new Note("This is a test note.", new DateTime(2025, 02, 11), "JSmith"));
            notes.Add(new Note("This is a test note.", new DateTime(2025, 01, 21), "JApple"));
            notes.Add(new Note("This is a test note.", new DateTime(2025, 04, 6), "JSmith"));
            notes.Add(new Note("This is a test note.", new DateTime(2025, 02, 1), "BJones"));
            notes.Add(new Note("This is a test note.", new DateTime(2025, 01, 11), "JSmith"));
            notes.Add(new Note("This is a test note.", new DateTime(2025, 02, 11), "JSmith"));

            Random rand = new Random();

            return new HHStation("8.711","Inside","Soap",true, rand.Next(0,99999), notes);*/


            //return HHStations.FirstOrDefault(station => station.StationID == SelectedStationID);

        }


    }
}
