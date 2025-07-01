namespace Handsify.Models
{
    public class Floor
    {
        public List<Pod> Pods { get; set; }
        public Floor() 
        {
            Pods = new List<Pod>();
        }
    }
}
