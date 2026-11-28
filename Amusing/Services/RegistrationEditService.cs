using Amusing.Helpers;
using MySqlConnector;
using Amusing.Shared;
namespace Amusing.Services;

// Price-bearing fields and waitlist placement deliberately have no writable properties.
public enum RegistrationEditAction { Details, Payment, Confirmation, Cancellation }
public sealed record RegistrationEdit(uint FestivalId,uint GroupId,string Name,DateTime? Registered,
 DateTime? Paid,DateTime? Confirmed,DateTime? Cancelled,int Inside,int Outside,bool Battle,bool Review,bool Waitlisted)
{
 public decimal? PaidAmount {get;init;}
 public TimeSpan From {get;init;}
 public TimeSpan Until {get;init;}
 public TimeSpan First {get;init;}
 public TimeSpan Last {get;init;}
 public FestivalAvailability Window=>new(First,Last);
}

public sealed class RegistrationEditService(IConfiguration config,UserContextHelper users)
{
 public static RegistrationEdit CreateDraft(RegistrationEdit current,RegistrationEditAction action,DateOnly today)=>action switch
 {
  RegistrationEditAction.Payment=>current with{Paid=current.Paid??today.ToDateTime(TimeOnly.MinValue)},
  RegistrationEditAction.Confirmation=>current with{Confirmed=current.Confirmed??today.ToDateTime(TimeOnly.MinValue)},
  RegistrationEditAction.Cancellation=>current with{Cancelled=current.Cancelled??today.ToDateTime(TimeOnly.MinValue)},
  _=>current
 };
 private MySqlConnection Connection()=>new(config.GetConnectionString("DefaultConnection"));
 private async Task<int> AuthorizeAsync(MySqlConnection connection,MySqlTransaction? tx)
 {
  var actor=await users.GetUserIdAsync();if(actor<=0)throw new UnauthorizedAccessException();
  await using var cmd=new MySqlCommand("SELECT COUNT(*) FROM ah_beheer WHERE user_id=@actor AND NULLIF(TRIM(role),'') IS NOT NULL",connection,tx);
  cmd.Parameters.AddWithValue("@actor",actor);
  if(Convert.ToInt32(await cmd.ExecuteScalarAsync())!=1)throw new UnauthorizedAccessException();return actor;
 }
 public async Task<RegistrationEdit?> ReadAsync(uint festival,uint group)
 {
  await using var connection=Connection();await connection.OpenAsync();await AuthorizeAsync(connection,null);
  return await ReadAsync(connection,null,festival,group);
 }
 private static async Task<RegistrationEdit?> ReadAsync(MySqlConnection connection,MySqlTransaction? tx,uint festival,uint group)
 {
  await using var cmd=new MySqlCommand("""
   SELECT g.naam,
    CASE WHEN CAST(i.ingeschreven AS CHAR) LIKE '0000-%' THEN NULL ELSE i.ingeschreven END,
    CASE WHEN CAST(i.betaald AS CHAR) LIKE '0000-%' THEN NULL ELSE i.betaald END,
    CASE WHEN CAST(i.bevestigd AS CHAR) LIKE '0000-%' THEN NULL ELSE i.bevestigd END,
    CASE WHEN CAST(i.afgehaakt AS CHAR) LIKE '0000-%' THEN NULL ELSE i.afgehaakt END,
    i.binnenoptredens,i.buitenoptredens,LOWER(COALESCE(i.wens_3,''))='ja',LOWER(COALESCE(i.wens_4,''))='ja',COALESCE(i.wachtlijst,0),
    i.betaald_bedrag,IF(i.beschikbaarheid_eigen=0,f.eerste_optreden,i.beschikbaar_van),
    IF(i.beschikbaarheid_eigen=0,f.laatste_optreden,i.beschikbaar_tot),f.eerste_optreden,f.laatste_optreden
   FROM ah_inschrijvingen i JOIN ah_zanggroepen g ON g.zanggroep_id=i.zanggroep_id
   JOIN ah_festivals f ON f.festival_id=i.festival_id
   WHERE i.festival_id=@festival AND i.zanggroep_id=@group
   """+(tx is null?"":" FOR UPDATE"),connection,tx);
  cmd.Parameters.AddWithValue("@festival",festival);cmd.Parameters.AddWithValue("@group",group);
  await using var r=await cmd.ExecuteReaderAsync();
  if(!await r.ReadAsync())return null;
  DateTime? Date(int column)=>r.IsDBNull(column)?null:r.GetDateTime(column);
  return new(festival,group,r.GetString(0),Date(1),Date(2),Date(3),Date(4),r.GetInt32(5),r.GetInt32(6),r.GetBoolean(7),r.GetBoolean(8),r.GetBoolean(9))
   {PaidAmount=r.IsDBNull(10)?null:r.GetDecimal(10),From=r.GetTimeSpan(11),Until=r.GetTimeSpan(12),First=r.GetTimeSpan(13),Last=r.GetTimeSpan(14)};
 }
 public static bool AllowsAction(RegistrationEdit original,RegistrationEdit edited,RegistrationEditAction action)=>action switch
 {
  RegistrationEditAction.Payment=>original with{Paid=edited.Paid,PaidAmount=edited.PaidAmount}==edited,
  RegistrationEditAction.Confirmation=>original with{Confirmed=edited.Confirmed}==edited,
  RegistrationEditAction.Cancellation=>original with{Cancelled=edited.Cancelled}==edited,
  RegistrationEditAction.Details=>original with{From=edited.From,Until=edited.Until,Inside=edited.Inside,Outside=edited.Outside,Battle=edited.Battle}==edited,
  _=>false
 };
 public async Task<string?> SaveAsync(RegistrationEdit original,RegistrationEdit edited,RegistrationEditAction action=RegistrationEditAction.Details)
 {
  await using var connection=Connection();await connection.OpenAsync();await using var tx=await connection.BeginTransactionAsync();
  var actor=await AuthorizeAsync(connection,tx);
  var current=await ReadAsync(connection,tx,original.FestivalId,original.GroupId);
  if(current is null || current!=original)return "De inschrijving is inmiddels gewijzigd. Open deze opnieuw.";
  if(edited.FestivalId!=original.FestivalId || edited.GroupId!=original.GroupId || edited.Waitlisted!=original.Waitlisted || edited.Name!=original.Name)
   return "Gebruik de bestaande beheerprocedure voor deze wijziging.";
  if(edited==current)return null;
  if(!AllowsAction(current,edited,action))return "Deze wijziging hoort niet bij de gekozen actie. Open de juiste actie opnieuw.";
  if(edited.PaidAmount is <0 or >99999999.99m || (edited.PaidAmount is {} amount && decimal.Round(amount,2)!=amount))return "Vul een geldig betaald bedrag in met maximaal twee decimalen.";
  if(action==RegistrationEditAction.Payment && edited.Paid is not null && edited.PaidAmount is null)return "Vul het betaalde bedrag in, of maak de betaaldatum leeg voor niet betaald.";
  if((edited.Inside!=current.Inside || edited.Outside!=current.Outside) &&
    (edited.Inside<0 || edited.Outside<0 || edited.Inside>2 || edited.Outside>2 || edited.Inside+edited.Outside!=2))
   return "Kies samen twee binnen- en buitenoptredens.";
  if(current.Waitlisted && (edited.Paid!=current.Paid || edited.PaidAmount!=current.PaidAmount || edited.Confirmed!=current.Confirmed))
   return "Plaats dit koor eerst via Wachtlijst beheren. Daar worden het plaatsingsbericht en de nota verstuurd.";
  if(edited.Cancelled is not null && edited.Confirmed!=current.Confirmed && edited.Confirmed is not null)
   return "Een afgehaakt koor kan niet worden bevestigd.";
  var availabilityChanged=edited.From!=current.From || edited.Until!=current.Until;
  if(availabilityChanged && (current.Cancelled is not null || !current.Window.Allows(edited.From,edited.Until)))return "Kies geldige tijden binnen het festivalvenster. De beschikbaarheid van een afgehaakt koor kan niet worden gewijzigd.";
  var changes=new List<string>();
  if(edited.Paid!=current.Paid)changes.Add("PaidAt");
  if(edited.PaidAmount!=current.PaidAmount)changes.Add("PaidAmount");
  if(availabilityChanged){changes.Add("AvailableFrom");changes.Add("AvailableUntil");}
  if(edited.Confirmed!=current.Confirmed)changes.Add("ConfirmedAt");
  if(edited.Cancelled!=current.Cancelled)changes.Add("CancelledAt");
  if(edited.Inside!=current.Inside || edited.Outside!=current.Outside)changes.Add("Preference");
  if(edited.Battle!=current.Battle)changes.Add("AcapellaBattle");
  var assignments=action switch
  {
   RegistrationEditAction.Payment=>"betaald=@paid,betaald_bedrag=@amount",
   RegistrationEditAction.Confirmation=>"bevestigd=@confirmed",
   RegistrationEditAction.Cancellation=>"afgehaakt=@cancelled",
   RegistrationEditAction.Details=>"""
    binnenoptredens=@inside,buitenoptredens=@outside,wens_3=@battle,
    beschikbaar_van=IF(@availabilityChanged,@from,beschikbaar_van),beschikbaar_tot=IF(@availabilityChanged,@until,beschikbaar_tot),
    beschikbaarheid_eigen=IF(@availabilityChanged,@custom,beschikbaarheid_eigen)
    """,
   _=>throw new ArgumentOutOfRangeException(nameof(action))
  };
  await using var update=new MySqlCommand($"UPDATE ah_inschrijvingen SET {assignments} WHERE festival_id=@festival AND zanggroep_id=@group",connection,tx);
  update.Parameters.AddWithValue("@amount",(object?)edited.PaidAmount??DBNull.Value);
  update.Parameters.AddWithValue("@availabilityChanged",availabilityChanged);update.Parameters.AddWithValue("@from",edited.From);update.Parameters.AddWithValue("@until",edited.Until);
  update.Parameters.AddWithValue("@custom",edited.From!=current.First || edited.Until!=current.Last);
  update.Parameters.AddWithValue("@paid",(object?)edited.Paid??DBNull.Value);update.Parameters.AddWithValue("@confirmed",(object?)edited.Confirmed??DBNull.Value);
  update.Parameters.AddWithValue("@cancelled",(object?)edited.Cancelled??DBNull.Value);
  update.Parameters.AddWithValue("@inside",edited.Inside);update.Parameters.AddWithValue("@outside",edited.Outside);
  update.Parameters.AddWithValue("@battle",edited.Battle?"ja":"nee");
  update.Parameters.AddWithValue("@festival",original.FestivalId);update.Parameters.AddWithValue("@group",original.GroupId);await update.ExecuteNonQueryAsync();
  await using var audit=new MySqlCommand("""
   INSERT INTO mijn_audit_log(occurred_at,actor_type,actor_id,action,group_id,festival_id,changed_fields)
   VALUES(UTC_TIMESTAMP(6),'beheer',@actor,'FestivalEnrollmentUpdated',@group,@festival,@changes)
   """,connection,tx);
  audit.Parameters.AddWithValue("@actor",actor);audit.Parameters.AddWithValue("@group",original.GroupId);audit.Parameters.AddWithValue("@festival",original.FestivalId);
  audit.Parameters.AddWithValue("@changes",string.Join(',',changes));await audit.ExecuteNonQueryAsync();await tx.CommitAsync();return null;
 }
}
