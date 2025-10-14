using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Amusing.Data;

[Table( "ah_beheer" )]
public class ApplicationUser : IdentityUser<uint>
{
    [Key]
    [Column( "user_id" )]
    public override uint Id { get; set; }

    [Column( "username" )]
    public override string? UserName { get; set; }

    // Laat deze kolom ongemoeid, alleen legacy MD5
    [Column( "password" )]
    public string LegacyPassword { get; set; } = "";

    // Nieuwe Identity-hash
    [Column( "PasswordHash" )]
    public override string? PasswordHash { get; set; }

    [Column( "role" )]
    public string Role { get; set; } = "";

    [Column( "SecurityStamp" )]
    public override string SecurityStamp { get; set; } = Guid.NewGuid().ToString();

    [Column( "ConcurrencyStamp" )]
    public override string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
}
