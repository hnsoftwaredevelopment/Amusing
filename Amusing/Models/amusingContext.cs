using Microsoft.EntityFrameworkCore;

using Pomelo.EntityFrameworkCore.MySql;
namespace Amusing.Models;

public partial class amusingContext : DbContext
{
    public amusingContext()
    {
    }

    public amusingContext( DbContextOptions<amusingContext> options )
        : base( options )
    {
    }

    public virtual DbSet<UserModel> AhBeheers { get; set; } = null!;
    public virtual DbSet<AhBeheerLog> AhBeheerLogs { get; set; } = null!;
    public virtual DbSet<AhContactgegeven> AhContactgegevens { get; set; } = null!;
    public virtual DbSet<AhFestival> AhFestivals { get; set; } = null!;
    public virtual DbSet<AhGenre> AhGenres { get; set; } = null!;
    public virtual DbSet<AhInschrijvingen> AhInschrijvingens { get; set; } = null!;
    public virtual DbSet<AhLanden> AhLandens { get; set; } = null!;
    public virtual DbSet<AhMailingTemplate> AhMailingTemplates { get; set; } = null!;
    public virtual DbSet<AhPersonen> AhPersonens { get; set; } = null!;
    public virtual DbSet<AhPersonenRollen> AhPersonenRollens { get; set; } = null!;
    public virtual DbSet<AhPersonenWachtwoorden> AhPersonenWachtwoordens { get; set; } = null!;
    public virtual DbSet<AhPodiaTypen> AhPodiaTypens { get; set; } = null!;
    public virtual DbSet<AhPodium> AhPodia { get; set; } = null!;
    public virtual DbSet<AhProfielbeheerLog> AhProfielbeheerLogs { get; set; } = null!;
    public virtual DbSet<AhProfielen> AhProfielens { get; set; } = null!;
    public virtual DbSet<AhRecipientList> AhRecipientLists { get; set; } = null!;
    public virtual DbSet<AhTaken> AhTakens { get; set; } = null!;
    public virtual DbSet<AhVrijwilliger> AhVrijwilligers { get; set; } = null!;
    public virtual DbSet<AhWenssoorten> AhWenssoortens { get; set; } = null!;
    public virtual DbSet<AhZanggroepDetail> AhZanggroepDetails { get; set; } = null!;
    public virtual DbSet<AhZanggroepen> AhZanggroepens { get; set; } = null!;
    public virtual DbSet<PlannerOptreden> PlannerOptredens { get; set; } = null!;
    public virtual DbSet<PlannerVoorwaarden> PlannerVoorwaardens { get; set; } = null!;
    public virtual DbSet<PlannerVrijwilligersdiensten> PlannerVrijwilligersdienstens { get; set; } = null!;
    public virtual DbSet<Temp> Temps { get; set; } = null!;
    public virtual DbSet<Token> Tokens { get; set; } = null!;

    protected override void OnConfiguring( DbContextOptionsBuilder optionsBuilder )
    {
        if ( !optionsBuilder.IsConfigured )
        {
            //Do nothing additional
        }
    }

    protected override void OnModelCreating( ModelBuilder modelBuilder )
    {
        modelBuilder.UseCollation( "utf8mb3_general_ci" )
            .HasCharSet( "utf8mb3" );

        modelBuilder.Entity<UserModel>( entity =>
        {
            entity.HasKey( e => e.UserId )
                .HasName( "PRIMARY" );

            entity.ToTable( t => t.HasComment( "ah_beheer" ) );
            entity.ToTable( "ah_beheer" );

            entity.Property( e => e.UserId )
                .HasColumnType( "int(10) unsigned" )
                .HasColumnName( "user_id" );

            entity.Property( e => e.Password )
                .HasMaxLength( 32 )
                .HasColumnName( "password" )
                .HasDefaultValueSql( "''" );

            entity.Property( e => e.Username )
                .HasMaxLength( 100 )
                .HasColumnName( "username" )
                .HasDefaultValueSql( "''" );
        } );

        modelBuilder.Entity<AhBeheerLog>( entity =>
        {
            entity.HasKey( e => e.LogId )
                .HasName( "PRIMARY" );

            entity.ToTable( t => t.HasComment( "ah_beheer_log" ) );

            entity.Property( e => e.LogId )
                .HasColumnType( "int(10) unsigned" )
                .HasColumnName( "log_id" );

            entity.Property( e => e.Action )
                .HasMaxLength( 255 )
                .HasColumnName( "action" )
                .HasDefaultValueSql( "''" );

            entity.Property( e => e.Date )
                .HasColumnType( "datetime" )
                .HasColumnName( "date" );

            entity.Property( e => e.IpAddress )
                .HasMaxLength( 40 )
                .HasColumnName( "ip_address" )
                .HasDefaultValueSql( "''" );

            entity.Property( e => e.Report )
                .HasColumnType( "text" )
                .HasColumnName( "report" );

            entity.Property( e => e.UserId )
                .HasColumnType( "int(10) unsigned" )
                .HasColumnName( "user_id" );
        } );

        modelBuilder.Entity<AhContactgegeven>()
        .HasKey( c => c.PersoonId );

        modelBuilder.Entity<AhContactgegeven>()
            .HasOne( c => c.Persoon )
            .WithOne( p => p.AhContactgegeven )
            .HasForeignKey<AhContactgegeven>( c => c.PersoonId );

        modelBuilder.Entity<AhFestival>()
            .HasOne( f => f.PlannerVoorwaarden )
            .WithOne( v => v.Festival )
            .HasForeignKey<PlannerVoorwaarden>( v => v.FestivalId );

        modelBuilder.Entity<AhPersonenWachtwoorden>()
            .HasOne( w => w.IdNavigation )
            .WithOne()
            .HasForeignKey<AhPersonenWachtwoorden>( w => w.Id );

        modelBuilder.Entity<AhZanggroepDetail>()
            .HasOne( z => z.IdNavigation )
            .WithOne()
            .HasForeignKey<AhZanggroepDetail>( z => z.Id );

        modelBuilder.Entity<AhProfielen>()
            .HasKey( p => p.ZanggroepId );

        modelBuilder.Entity<AhProfielen>()
            .HasOne( p => p.Zanggroep )
            .WithOne( z => z.AhProfielen )
            .HasForeignKey<AhProfielen>( p => p.ZanggroepId );

        modelBuilder.Entity<AhFestival>( entity =>
        {
            entity.HasKey( e => e.FestivalId ).HasName( "PRIMARY" );
        } );

        modelBuilder.Entity<AhGenre>( entity =>
        {
            entity.HasKey( e => e.GenreId ).HasName( "PRIMARY" );
        } );

        modelBuilder.Entity<AhInschrijvingen>( entity =>
        {
            entity.HasKey( e => e.FestivalId ).HasName( "PRIMARY" );
            entity.HasKey( e => e.ZanggroepId ).HasName( "PRIMARY" );
        } );

        modelBuilder.Entity<AhLanden>( entity =>
        {
            entity.HasKey( e => e.Code ).HasName( "PRIMARY" );
        } );

        modelBuilder.Entity<AhMailingTemplate>( entity =>
        {
            entity.HasKey( e => e.Id ).HasName( "PRIMARY" );
        } );

        modelBuilder.Entity<AhPersonen>( entity =>
        {
            entity.HasKey( e => e.PersoonId ).HasName( "PRIMARY" );
        } );

        modelBuilder.Entity<AhPersonenRollen>( entity =>
        {
            entity.HasKey( e => new { e.PersoonId, e.ZanggroepId, e.Rol } ).HasName( "PRIMARY" );
            entity.ToTable( "ah_personen_rollen" );
            entity.Property( e => e.PersoonId ).HasColumnName( "Persoon_Id" );
            entity.Property( e => e.ZanggroepId ).HasColumnName( "Zanggroep_Id" );
            entity.Property( e => e.Rol ).HasColumnName( "Rol" );
        } );

        modelBuilder.Entity<AhPodiaTypen>( entity =>
        {
            entity.HasKey( e => e.Type ).HasName( "PRIMARY" );
            entity.HasKey( e => e.Versie ).HasName( "PRIMARY" );
        } );

        modelBuilder.Entity<AhPodium>( entity =>
        {
            entity.HasKey( e => e.PodiumId ).HasName( "PRIMARY" );
        } );

        modelBuilder.Entity<AhProfielbeheerLog>( entity =>
        {
            entity.HasKey( e => e.LogId ).HasName( "PRIMARY" );
        } );

        modelBuilder.Entity<AhTaken>( entity =>
        {
            entity.HasKey( e => e.TaakId ).HasName( "PRIMARY" );
        } );

        modelBuilder.Entity<AhWenssoorten>( entity =>
        {
            entity.HasKey( e => e.WenssoortId ).HasName( "PRIMARY" );
        } );

        modelBuilder.Entity<AhZanggroepen>( entity =>
        {
            entity.HasKey( e => e.ZanggroepId ).HasName( "PRIMARY" );
        } );

        modelBuilder.Entity<PlannerOptreden>( entity =>
        {
            entity.HasKey( e => e.FestivalId ).HasName( "PRIMARY" );
            entity.HasKey( e => e.ZanggroepId ).HasName( "PRIMARY" );
            entity.HasKey( e => e.Tijdvak ).HasName( "PRIMARY" );
        } );

        modelBuilder.Entity<PlannerVoorwaarden>( entity =>
        {
            entity.HasKey( e => e.FestivalId ).HasName( "PRIMARY" );
        } );

        modelBuilder.Entity<PlannerVrijwilligersdiensten>( entity =>
        {
            entity.HasKey( e => e.FestivalId ).HasName( "PRIMARY" );
            entity.HasKey( e => e.PersoonId ).HasName( "PRIMARY" );
            entity.HasKey( e => e.Van ).HasName( "PRIMARY" );
        } );

        modelBuilder.Entity<Temp>( entity =>
        {
            entity.HasKey( e => e.PersoonId ).HasName( "PRIMARY" );
            entity.HasKey( e => e.ZanggroepId ).HasName( "PRIMARY" );
        } );

        modelBuilder.Entity<Token>( entity =>
        {
            entity.HasKey( e => e.Id ).HasName( "PRIMARY" );
        } );


        // Repeat similar changes for other entities where `HasComment` is used.
        OnModelCreatingPartial( modelBuilder );
    }

    partial void OnModelCreatingPartial( ModelBuilder modelBuilder );
}
