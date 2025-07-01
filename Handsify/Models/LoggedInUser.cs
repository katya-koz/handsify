using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;

namespace Handsify.Models
{
    internal enum Role
    {
        CE, 
        Analyst,
        Supervisor
    
    }

    public class LoggedInUserModel
    {
        public string Name { get; set; }
        public string ADUser { get; set; }

         readonly Role _role;
       public List<string> Roles { get; set; }
        public string ToString()
        {
            //return ("Name: " + Name + "\nADUser: " + ADUser + "\n ProfileImage: " + ProfileImage.ToString());
            return ("Name: " + Name + "\nADUser: " + ADUser + "\nRole " + _role.ToString() + "\n Roles Number " + Roles.Count);

        }
        public LoggedInUserModel(string name, string user)// for testing
        {
            Name = name; 
            ADUser = user;
            Roles = new List<string>();

            Roles.Add("Supervisor");

        }

            public LoggedInUserModel(string token)
        {

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);  // This reads the JWT token



            // Extract claims from the token
            Name = jwtToken.Claims.FirstOrDefault(c => c.Type == "Name")?.Value;
            ADUser = jwtToken.Claims.FirstOrDefault(c => c.Type == "ADUser")?.Value;
            try
            {
                _role = ParseRole(jwtToken.Claims.FirstOrDefault(c => c.Type == "role").Value);
                Roles = jwtToken.Claims
                .Where(c => c.Type == "role")
                    .Select(c => c.Value)
                    .ToList();

            }
            catch (Exception ex)
            {
                //Console.WriteLine(("Role not found in token, defaulting to CE role: " +ex);
                _role = Role.CE;
            }

           // var profileImageUrl = jwtToken.Claims.FirstOrDefault(c => c.Type == "ProfileImageUrl")?.Value;

        }
        
        internal Role ParseRole(string role)
        {
            switch (role)
            {
                case ("CE"):
                    return Role.CE;
                case ("Analyst"):
                    return Role.Analyst;
                case ("Supervisor"):
                    return Role.Supervisor;
                default:
                    throw new NotImplementedException();
            }
        }

        public LoggedInUserModel()        
        { 
        }
    }

    
}
