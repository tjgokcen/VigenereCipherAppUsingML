namespace VigenereCipherApp;

public class VigenereCipher
{
    // Encrypts text using the Vigenere cipher
    public static string Encrypt(string text, string key)
    {
        var result = string.Empty;
        key = key.ToUpper();
        for (int i = 0, j = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (char.IsLetter(c))
            {
                c = char.ToUpper(c);
                result += (char)((c + key[j % key.Length] - 2 * 'A') % 26 + 'A');
                j++;
            }
            else
            {
                result += c; // Non-letter characters remain unchanged
            }
        }

        return result;
    }

    // Decrypts text using the Vigenere cipher
    public static string Decrypt(string text, string key)
    {
        var result = string.Empty;
        key = key.ToUpper();
        for (int i = 0, j = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (char.IsLetter(c))
            {
                c = char.ToUpper(c);
                result += (char)((c - key[j % key.Length] + 26) % 26 + 'A');
                j++;
            }
            else
            {
                result += c;
            }
        }

        return result;
    }
}