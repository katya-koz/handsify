using System.ComponentModel.DataAnnotations;

namespace Handsify.Models
{
    public class LoginModel
    {
        public string Message { get; set; }
        public bool IsError { get; set; }

        [Required]

        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }


    }


}
