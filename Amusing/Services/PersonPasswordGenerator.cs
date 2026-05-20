using System.Security.Cryptography;

namespace Amusing.Services;

public static class PersonPasswordGenerator
{
    private const string LowercaseCharacters = "abcdefghijklmnopqrstuvwxyz";
    private const string UppercaseCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string NumberCharacters = "0123456789";
    public const string SpecialCharacters = "!@#%^&*(){}:;<>?~";

    public static string GenerateTemporaryPassword()
    {
        char[] characters =
        [
            .. PickCharacters(LowercaseCharacters, 7),
            PickCharacter(UppercaseCharacters),
            PickCharacter(NumberCharacters),
            PickCharacter(SpecialCharacters)
        ];

        RandomNumberGenerator.Shuffle<char>(characters);
        return new string(characters);
    }

    private static char[] PickCharacters(string source, int count)
    {
        char[] result = new char[count];

        for (int i = 0; i < count; i++)
        {
            result[i] = PickCharacter(source);
        }

        return result;
    }

    private static char PickCharacter(string source)
    {
        return source[RandomNumberGenerator.GetInt32(source.Length)];
    }
}
