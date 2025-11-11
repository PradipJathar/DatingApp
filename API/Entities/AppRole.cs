using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppRole : IdentityUserRole<int>
    {
        public ICollection<AppUserRole> UserRoles { get; set; }
    }
}