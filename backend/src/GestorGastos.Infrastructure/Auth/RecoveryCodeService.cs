using System.Security.Cryptography;

namespace GestorGastos.Infrastructure.Auth;

public interface IRecoveryCodeService
{
    /// <summary>Generates fresh single-use recovery codes as plaintext/hash pairs.</summary>
    IReadOnlyList<RecoveryCode> Generate(int count = 10);

    /// <summary>Hashes a user-supplied code after normalizing it, for lookup against stored hashes.</summary>
    string HashInput(string code);
}

public record RecoveryCode(string PlainText, string Hash);

public class RecoveryCodeService : IRecoveryCodeService
{
    // Base32 alphabet without easily confused characters (no 0/O/1/I).
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int GroupLength = 5;

    public IReadOnlyList<RecoveryCode> Generate(int count = 10)
    {
        var codes = new List<RecoveryCode>(count);
        for (var i = 0; i < count; i++)
        {
            var plain = $"{RandomGroup()}-{RandomGroup()}";
            codes.Add(new RecoveryCode(plain, TokenHashing.Hash(Normalize(plain))));
        }

        return codes;
    }

    public string HashInput(string code) => TokenHashing.Hash(Normalize(code));

    private static string RandomGroup()
    {
        var chars = new char[GroupLength];
        for (var i = 0; i < GroupLength; i++)
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];

        return new string(chars);
    }

    private static string Normalize(string code) =>
        code.Replace("-", string.Empty).Replace(" ", string.Empty).ToUpperInvariant();
}
