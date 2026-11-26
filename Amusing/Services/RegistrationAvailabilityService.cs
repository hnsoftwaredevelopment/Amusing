using Amusing.Helpers;
using Amusing.Shared;
using MySqlConnector;

namespace Amusing.Services;
public sealed record RegistrationAvailability(uint FestivalId,uint GroupId,TimeSpan From,TimeSpan Until,TimeSpan First,TimeSpan Last,bool Cancelled)
{
 public FestivalAvailability Window => new(First,Last);
}
public sealed class RegistrationAvailabilityService(IConfiguration config,UserContextHelper users)
{
 private MySqlConnection Connection() => new(config.GetConnectionString("DefaultConnection"));
 public async Task<RegistrationAvailability?> ReadAsync(uint festival,uint group)
 {
  if(await users.GetUserIdAsync()<=0) throw new UnauthorizedAccessException();
  await using var connection=Connection();await connection.OpenAsync();
  return await ReadAsync(connection,null,festival,group);
 }
 private static async Task<RegistrationAvailability?> ReadAsync(MySqlConnection connection,MySqlTransaction? tx,uint festival,uint group)
 {
  await using var command=new MySqlCommand("""
   SELECT i.beschikbaar_van,i.beschikbaar_tot,f.eerste_optreden,f.laatste_optreden,
    COALESCE(CAST(i.afgehaakt AS CHAR) NOT LIKE '0000-%',0)
   FROM ah_inschrijvingen i JOIN ah_festivals f ON f.festival_id=i.festival_id
   WHERE i.festival_id=@festival AND i.zanggroep_id=@group
   """+(tx is null?"":" FOR UPDATE"),connection,tx);
  command.Parameters.AddWithValue("@festival",festival);command.Parameters.AddWithValue("@group",group);
  await using var reader=await command.ExecuteReaderAsync();
  return await reader.ReadAsync()?new(festival,group,reader.GetTimeSpan(0),reader.GetTimeSpan(1),reader.GetTimeSpan(2),reader.GetTimeSpan(3),reader.GetBoolean(4)):null;
 }
 public async Task<string?> SaveAsync(RegistrationAvailability original,TimeSpan from,TimeSpan until)
 {
  var actor=await users.GetUserIdAsync();if(actor<=0)throw new UnauthorizedAccessException();
  await using var connection=Connection();await connection.OpenAsync();await using var tx=await connection.BeginTransactionAsync();
  await using(var account=new MySqlCommand("SELECT COUNT(*) FROM ah_beheer WHERE user_id=@actor AND NULLIF(TRIM(role),'') IS NOT NULL",connection,tx))
  {account.Parameters.AddWithValue("@actor",actor);if(Convert.ToInt32(await account.ExecuteScalarAsync())!=1)throw new UnauthorizedAccessException();}
  var current=await ReadAsync(connection,tx,original.FestivalId,original.GroupId);
  if(current is null || current.Cancelled)return "Deze inschrijving kan niet worden gewijzigd.";
  if(current.From!=original.From || current.Until!=original.Until)return "De beschikbaarheid is inmiddels gewijzigd. Open de tijden opnieuw.";
  if(current.From==from && current.Until==until)return null;
  if(!current.Window.Allows(from,until))return "Kies tijden binnen het festivalvenster in stappen van een halfuur, met minimaal een halfuur tussen begin en eind.";
  await using var update=new MySqlCommand("UPDATE ah_inschrijvingen SET beschikbaar_van=@from,beschikbaar_tot=@until WHERE festival_id=@festival AND zanggroep_id=@group",connection,tx);
  update.Parameters.AddWithValue("@from",from);update.Parameters.AddWithValue("@until",until);update.Parameters.AddWithValue("@festival",original.FestivalId);update.Parameters.AddWithValue("@group",original.GroupId);await update.ExecuteNonQueryAsync();
  await using var audit=new MySqlCommand("""
   INSERT INTO mijn_audit_log(occurred_at,actor_type,actor_id,action,group_id,festival_id,changed_fields)
   VALUES(UTC_TIMESTAMP(6),'beheer',@actor,'FestivalEnrollmentUpdated',@group,@festival,'AvailableFrom,AvailableUntil')
   """,connection,tx);
  audit.Parameters.AddWithValue("@actor",actor);audit.Parameters.AddWithValue("@group",original.GroupId);audit.Parameters.AddWithValue("@festival",original.FestivalId);await audit.ExecuteNonQueryAsync();
  await tx.CommitAsync();return null;
 }
}
