using Microsoft.ML;
using VigenereCipherApp;

var mlContext = new MLContext();

// File path and download URL for the Google 10,000 word list
var wordListPath = "google-10000-english.txt";
var wordListUrl = "https://raw.githubusercontent.com/first20hours/google-10000-english/master/google-10000-english.txt";

// Load the word library (download if necessary)
var wordLibrary = await TrainingDataGenerator.LoadWordLibraryAsync(wordListPath, wordListUrl);

// Check if the model already exists
var modelExists = File.Exists("vigenere_model.zip");

// Generate training and test data using the Google word library with a 50% chance of word-based keys
var (trainDataList, testDataList) = TrainingDataGenerator.GenerateTrainingAndTestData(
    numberOfExamples: 5000,
    testDataPercentage: 20,   // 20% for testing
    minKeyLength: 3,
    maxKeyLength: 10,
    textLength: 100,
    noiseLevel: 0.01,          // 10% noise in plaintext
    wordLibrary: wordLibrary,  // Use the Google word library for key generation
    useWordProbability: 0.5    // 50% chance of using word-based keys
);

// Convert lists to IDataView for ML.NET
var trainData = mlContext.Data.LoadFromEnumerable(trainDataList);
var testData = mlContext.Data.LoadFromEnumerable(testDataList);

if (!modelExists)
{
    Console.WriteLine("Model not found. Training the model...");
    VigenereCipherML.TrainModel(mlContext, trainData);
}
else
{
    Console.WriteLine("Trained model found. Skipping training...");
}

// Evaluate the model with test data
VigenereCipherML.EvaluateModel(mlContext, testData);

// Predict the key for a new encrypted text
var encryptedText = "TWPYOGDSTXIHUCMDPN";
var predictedKey = VigenereCipherML.PredictKey(mlContext, encryptedText);

Console.WriteLine($"Predicted Key: {predictedKey}");
var decryptedText = VigenereCipher.Decrypt(encryptedText, predictedKey);
Console.WriteLine($"Decrypted Text: {decryptedText}");