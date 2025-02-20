using Microsoft.AspNetCore.Identity;

namespace RestoreApiV2.Entities
{
    public class User : IdentityUser
    {
        public int? AddressId { get; set; }
        public Address? Address { get; set; }
    }
}
