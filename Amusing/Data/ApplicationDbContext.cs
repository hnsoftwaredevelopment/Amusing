using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Amusing.Data;

public class ApplicationDbContext( DbContextOptions<ApplicationDbContext> options )
    : IdentityDbContext<ApplicationUser, IdentityRole<uint>, uint>( options )
{
    protected override void OnModelCreating( ModelBuilder builder )
    {
        base.OnModelCreating( builder );

        // Map ApplicationUser naar bestaande tabel
        builder.Entity<ApplicationUser>( entity =>
        {
            entity.ToTable( "ah_beheer" );

            entity.Property( e => e.Id )
                  .HasColumnName( "user_id" );

            entity.Property( e => e.UserName )
                  .HasColumnName( "username" );

            entity.Property( e => e.LegacyPassword )
          .HasColumnName( "password" ); // legacy MD5

            entity.Property( e => e.PasswordHash )
                  .HasColumnName( "PasswordHash" ); // Identity hash

            entity.Property( e => e.Role )
                  .HasColumnName( "role" );

            entity.Property( e => e.SecurityStamp )
          .HasColumnName( "SecurityStamp" );

            entity.Property( e => e.ConcurrencyStamp )
                  .HasColumnName( "ConcurrencyStamp" );
        } );
    }
}
