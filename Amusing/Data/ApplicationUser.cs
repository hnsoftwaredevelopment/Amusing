using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Amusing.Data
{
    [Table( "AH_Beheer" )]
    public class ApplicationUser : IdentityUser
    {
        [Key]
        [Column( "user_id" )]
        public override string Id { get; set; } = Guid.NewGuid().ToString();

        [Column( "username" )]
        public override string UserName { get; set; } = string.Empty;

        [Column( "password" )]
        public override string PasswordHash { get; set; } = string.Empty;

        [Column( "role" )]
        public string Role { get; set; } = string.Empty;

        // Deze properties worden genegeerd in EF maar Identity verwacht ze wel
        // We geven ze automatisch waarden gebaseerd op UserName
        public override string Email
        {
            get => UserName ?? string.Empty;
            set { } // Setter doet niets omdat we Email niet opslaan
        }

        public override string NormalizedUserName
        {
            get => UserName?.ToUpper() ?? string.Empty;
            set { }
        }

        public override string NormalizedEmail
        {
            get => UserName?.ToUpper() ?? string.Empty;
            set { }
        }

        public override string SecurityStamp
        {
            get => "default-stamp";
            set { }
        }

        public override string ConcurrencyStamp
        {
            get => "default-concurrency";
            set { }
        }

        // Andere Identity properties met defaults
        public override bool EmailConfirmed { get; set; } = true;
        public override string PhoneNumber { get; set; } = string.Empty;
        public override bool PhoneNumberConfirmed { get; set; } = false;
        public override bool TwoFactorEnabled { get; set; } = false;
        public override DateTimeOffset? LockoutEnd { get; set; } = null;
        public override bool LockoutEnabled { get; set; } = false;
        public override int AccessFailedCount { get; set; } = 0;
    }
}