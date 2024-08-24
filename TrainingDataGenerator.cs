namespace VigenereCipherApp;

public class TrainingDataGenerator
{
    private static Random random = new Random();

    // Check if the word list file exists; if not, download it
    // Check if the word list file exists; if not, download it using HttpClient
    public static async Task<List<string>> LoadWordLibraryAsync(string filePath, string downloadUrl)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Word list file not found. Downloading...");

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(downloadUrl);
                response.EnsureSuccessStatusCode();

                // Write the downloaded content to the file
                var content = await response.Content.ReadAsStringAsync();
                File.WriteAllText(filePath, content);
            }

            Console.WriteLine("Download complete.");
        }

        // Load the word library from the file
        return System.IO.File.ReadAllLines(filePath).Select(word => word.ToUpper()).ToList();
    }

    // Generate a random key from the word library or as a random string
    public static string GenerateRandomKey(int length, List<string> wordLibrary, double useWordProbability = 0.5)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        // Use a word from the library with the given probability
        if (random.NextDouble() < useWordProbability && wordLibrary.Count > 0)
        {
            // Select a word from the library and adjust it to match the required length
            var word = wordLibrary[random.Next(wordLibrary.Count)];
            if (word.Length > length)
            {
                return word[..length].ToUpper(); // Truncate to match length
            }

            return word.Length < length ?
                // Pad the word with random characters if shorter than the required length
                word.ToUpper().PadRight(length, chars[random.Next(chars.Length)]) : word.ToUpper(); // Return the word as the key if the length matches
        }

        // Otherwise, generate a random key
        var key = new char[length];
        for (var i = 0; i < length; i++)
        {
            key[i] = chars[random.Next(chars.Length)];
        }
        return new string(key);
    }

    // Generate a random plaintext of a given length
    public static string GenerateRandomPlainText(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ ";
        char[] text = new char[length];

        for (int i = 0; i < length; i++)
        {
            text[i] = chars[random.Next(chars.Length)];
        }

        // Randomly vary text length for some examples
        if (random.NextDouble() < 0.3) // 30% chance of shorter text
        {
            int newLength = random.Next(10, length);
            return new string(text, 0, newLength);
        }

        return new string(text);
    }
    
    public static string AddNoiseToText(string text, double noiseLevel)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ ";
        char[] noisyText = text.ToCharArray();

        for (int i = 0; i < text.Length; i++)
        {
            if (random.NextDouble() < noiseLevel)
            {
                noisyText[i] = chars[random.Next(chars.Length)];
            }
        }

        return new string(noisyText);
    }

    // Generate training and test data with word-based keys
    public static (List<CipherData> trainData, List<CipherData> testData) GenerateTrainingAndTestData(
        int numberOfExamples,
        int testDataPercentage,
        int minKeyLength,
        int maxKeyLength,
        int textLength,
        double noiseLevel,
        List<string> wordLibrary,
        double useWordProbability)
    {
        var allData = new List<CipherData>();

        for (var i = 0; i < numberOfExamples; i++)
        {
            // Generate a key (using word library sometimes)
            var keyLength = random.Next(minKeyLength, maxKeyLength + 1);
            var key = GenerateRandomKey(keyLength, wordLibrary, useWordProbability);

            // Generate plaintext
            var plainText = GenerateRandomPlainText(textLength);

            // Optionally add noise to plaintext
            var noisyPlainText = AddNoiseToText(plainText, noiseLevel);

            // Encrypt the (noisy or original) plaintext with the key
            var encryptedText = VigenereCipher.Encrypt(noisyPlainText, key);

            allData.Add(new CipherData { EncryptedText = encryptedText, Key = key });
        }

        // Split data into training and test data sets
        var testDataSize = (int)(numberOfExamples * (testDataPercentage / 100.0));
        var trainData = allData.Take(numberOfExamples - testDataSize).ToList();
        var testData = allData.Skip(numberOfExamples - testDataSize).ToList();

        return (trainData, testData);
    }
}
