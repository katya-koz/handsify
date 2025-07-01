namespace Handsify.Models
{
    // need to make a composite model to contain 'pod' and 'navbarmodel' 
    public class PageInfoModel
    {
        public Pod Pod;
        public NavbarModel Navbar;
        public LoggedInUserModel LoggedInUser;
    }
}
