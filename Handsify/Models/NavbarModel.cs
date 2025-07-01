namespace Handsify.Models
{
    /*    enum Modes {View, Edit, Operation};
        enum Floors {Level2, Level4, Level5, Level6, Level7, Level8};
        enum Units {A, B, C, D, E, F};*/

    // this is ridiculous. idk what i was thinking with these enums

    public class NavbarModel
    {
        public int SelectedModeIndex { get; set; } = -1;
        public int SelectedFloorIndex { get; set; } = 0;
        public int SelectedUnitIndex { get; set; } = 0;
        public List<string> AvailableUnitsForFloor { get; set; } = new List<string>();
        /*public List<string> Modes => Enum.GetNames(typeof(Modes)).ToList(); 
        public List<string> Floors => Enum.GetNames(typeof(Floors)).ToList();
        public List<string> Units => Enum.GetNames(typeof(Units)).ToList();*/

        public static Dictionary<int, string> Modes = new Dictionary<int, string>
        {
            { 0, "View" },
            { 1, "Edit" },
            { 2, "Operation" }
        };

        public static Dictionary<int, string> Floors = new Dictionary<int, string>
        {
            { 0, "2" },
            { 1, "4" },
            { 2, "5" },
            { 3, "6" },
            { 4, "7" },
            { 5, "8" }
        };

        public static Dictionary<int, string> Units = new Dictionary<int, string>
        {
            { 0, "A" },
            { 1, "B" },
            { 2, "C" },
            { 3, "D" },
            { 4, "E" },
            { 5, "F" }
        };

        public override string ToString()
        {
            return (
                "\nAvailableUnitsForFloor: " + AvailableUnitsForFloor.ToString()
                + "\nSelectedModeIndex: " + SelectedModeIndex 
                + "\nSelectedFloorIndex: " + SelectedFloorIndex
                + "\nSelectedUnitIndex: " + SelectedUnitIndex
                );
        }

    }
    
}
