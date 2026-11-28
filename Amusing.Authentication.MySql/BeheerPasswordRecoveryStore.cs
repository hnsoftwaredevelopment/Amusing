using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using Amusing.Authentication;
namespace Amusing.Authentication.MySql;

public sealed record BeheerResetRequest(uint UserId,string Recipient,string Token);

/// <summary>Shared recovery storage for Beheer accounts; the host owns mail routing and UI.</summary>
public sealed class BeheerPasswordRecoveryStore(DbDataSource source)
{
 private static DbCommand Command(DbConnection connection,DbTransaction? tx,string sql)
 { var command=connection.CreateCommand();command.Transaction=tx;command.CommandText=sql;return command; }
 private static void Add(DbCommand command,string name,object value)
 {var p=command.CreateParameter();p.ParameterName=name;p.Value=value;command.Parameters.Add(p);}
 private static byte[] TokenHash(string token)=>SHA256.HashData(Encoding.ASCII.GetBytes(token));
 private static string Stamp(string credential,string email)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(credential+"|"+email)));

 public async Task<BeheerResetRequest?> IssueAsync(string username,string email,CancellationToken ct)
 {
  if(username.Length is 0 or >100 || email.Length is 0 or >80)return null;
  await using var connection=await source.OpenConnectionAsync(ct);
  uint actor;string stamp;
  await using(var command=Command(connection,null,"""
   SELECT user_id,COALESCE(NULLIF(PasswordHash,''),password),herstel_email
   FROM ah_beheer WHERE username=@username AND herstel_email=@email
    AND NULLIF(TRIM(role),'') IS NOT NULL LIMIT 2
   """))
  {
   Add(command,"@username",username);Add(command,"@email",email);
   await using var reader=await command.ExecuteReaderAsync(ct);
   if(!await reader.ReadAsync(ct))return null;
   actor=Convert.ToUInt32(reader.GetValue(0));email=reader.GetString(2);stamp=Stamp(reader.GetString(1),email);
   if(await reader.ReadAsync(ct))return null;
  }
  var token=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
  await using var insert=Command(connection,null,"""
   INSERT INTO mijn_password_reset(token_hash,person_id,actor_type,credential_stamp,expires_at)
   VALUES(@token,@actor,'beheer',@stamp,DATE_ADD(UTC_TIMESTAMP(6),INTERVAL 1 HOUR))
   """);
  Add(insert,"@token",TokenHash(token));Add(insert,"@actor",actor);Add(insert,"@stamp",stamp);
  await insert.ExecuteNonQueryAsync(ct);
  return new(actor,email,token);
 }

 public async Task<bool> ResetAsync(string token,string password,CancellationToken ct)
 {
  if(!PasswordRecoveryRules.ValidToken(token) || !PasswordRecoveryRules.ValidPassword(password,password))return false;
  await using var connection=await source.OpenConnectionAsync(ct);
  await using var tx=await connection.BeginTransactionAsync(ct);
  await using var lookup=Command(connection,tx,"""
   SELECT person_id,credential_stamp FROM mijn_password_reset
   WHERE token_hash=@token AND actor_type='beheer' AND expires_at>UTC_TIMESTAMP(6)
   """);
  Add(lookup,"@token",TokenHash(token));uint actor;string stamp;
  await using(var reader=await lookup.ExecuteReaderAsync(ct))
  {if(!await reader.ReadAsync(ct))return false;actor=Convert.ToUInt32(reader.GetValue(0));stamp=reader.GetString(1);}
  // Lock account before tokens, consistently with the Mijn credential-change flow.
  await using(var account=Command(connection,tx,"""
   SELECT COALESCE(NULLIF(PasswordHash,''),password),herstel_email FROM ah_beheer
   WHERE user_id=@actor AND NULLIF(TRIM(role),'') IS NOT NULL AND herstel_email IS NOT NULL FOR UPDATE
   """))
  {
   Add(account,"@actor",actor);await using var reader=await account.ExecuteReaderAsync(ct);
   if(!await reader.ReadAsync(ct))return false;
   var current=Stamp(reader.GetString(0),reader.GetString(1));
   if(!CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(current),Encoding.ASCII.GetBytes(stamp)))return false;
  }
  lookup.CommandText="SELECT 1 FROM mijn_password_reset WHERE token_hash=@token AND actor_type='beheer' AND expires_at>UTC_TIMESTAMP(6) FOR UPDATE";
  if(await lookup.ExecuteScalarAsync(ct) is null)return false;
  await using var update=Command(connection,tx,"""
   UPDATE ah_beheer SET PasswordHash=@hash,password='' WHERE user_id=@actor;
   DELETE FROM mijn_password_reset WHERE person_id=@actor AND actor_type='beheer';
   INSERT INTO mijn_audit_log(occurred_at,actor_type,actor_id,action) VALUES(UTC_TIMESTAMP(6),'beheer',@actor,'PasswordReset');
   """);
  Add(update,"@hash",BCrypt.Net.BCrypt.HashPassword(password,11));Add(update,"@actor",actor);
  await update.ExecuteNonQueryAsync(ct);await tx.CommitAsync(ct);return true;
 }
}
