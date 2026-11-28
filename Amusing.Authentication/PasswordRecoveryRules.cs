using System.Security.Cryptography;
using System.Text;
namespace Amusing.Authentication;
public static class PasswordRecoveryRules
{
 public static bool ValidPassword(string password,string repeated) => password.Length>=12 && Encoding.UTF8.GetByteCount(password)<=72 && password==repeated;
 public static bool ValidToken(string token) => token.Length==64 && token.All(Uri.IsHexDigit);
}
