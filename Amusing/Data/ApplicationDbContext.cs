using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Amusing.Data;

public class ApplicationDbContext( DbContextOptions<ApplicationDbContext> options ) : IdentityDbContext<ApplicationUser>( options )
{
    protected override void OnModelCreating( ModelBuilder builder )
    {
        base.OnModelCreating( builder );

        // Configure de ApplicationUser entity - alleen map wat echt bestaat
        builder.Entity<ApplicationUser>( entity =>
        {
            entity.ToTable( "ah_beheer" );

            // Primaire sleutel
            entity.HasKey( e => e.Id );
            entity.Property( e => e.Id ).HasColumnName( "user_id" ).HasMaxLength( 255 );

            // Bestaande kolommen
            entity.Property( e => e.UserName ).HasColumnName( "username" ).HasMaxLength( 255 );
            entity.Property( e => e.PasswordHash ).HasColumnName( "password" ).HasMaxLength( 255 );
            entity.Property( e => e.Role ).HasColumnName( "role" ).HasMaxLength( 50 );

            // Ignore ALLE andere Identity properties
            entity.Ignore( e => e.Email );
            entity.Ignore( e => e.NormalizedUserName );
            entity.Ignore( e => e.NormalizedEmail );
            entity.Ignore( e => e.SecurityStamp );
            entity.Ignore( e => e.ConcurrencyStamp );
            entity.Ignore( e => e.EmailConfirmed );
            entity.Ignore( e => e.PhoneNumber );
            entity.Ignore( e => e.PhoneNumberConfirmed );
            entity.Ignore( e => e.TwoFactorEnabled );
            entity.Ignore( e => e.LockoutEnd );
            entity.Ignore( e => e.LockoutEnabled );
            entity.Ignore( e => e.AccessFailedCount );
        } );

        // Ignore andere Identity tabellen volledig
        builder.Ignore<Microsoft.AspNetCore.Identity.IdentityRole>();
        builder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>();
        builder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>();
        builder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>();
        builder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>();
        builder.Ignore<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>();
    }
}