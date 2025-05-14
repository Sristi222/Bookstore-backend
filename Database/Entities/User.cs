using Microsoft.AspNetCore.Identity;

namespace Try_application.Database.Entities
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }
    }
}
