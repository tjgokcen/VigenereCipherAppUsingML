using Microsoft.ML;

namespace VigenereCipherApp;

public class VigenereCipherML
{
    public static void TrainModel(MLContext mlContext, IDataView trainData)
    {
        // Define the pipeline
        var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(CipherData.EncryptedText))
            .Append(mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(CipherData.Key))) // Map Key to Label
            .Append(mlContext.Transforms.Concatenate("Features", "Features"))
            .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
            .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "PredictedLabel"))
            .Append(mlContext.Transforms.CopyColumns("PredictedKey", "PredictedLabel"));

        // Train the model
        var model = pipeline.Fit(trainData);

        // Save the model for future use
        mlContext.Model.Save(model, trainData.Schema, "vigenere_model.zip");

        Console.WriteLine("Model training complete.");
    }

    public static string PredictKey(MLContext mlContext, string encryptedText)
    {
        // Load the trained model
        var trainedModel = mlContext.Model.Load("vigenere_model.zip", out _);

        // Create a prediction engine
        var predictionEngine = mlContext.Model.CreatePredictionEngine<CipherData, KeyPrediction>(trainedModel);

        // Make a prediction
        var input = new CipherData { EncryptedText = encryptedText };
        var prediction = predictionEngine.Predict(input);

        return prediction.PredictedKey ?? "Could not predict Key."; ;
    }

    public static void EvaluateModel(MLContext mlContext, IDataView testData)
    {
        var trainedModel = mlContext.Model.Load("vigenere_model.zip", out _);
        var predictions = trainedModel.Transform(testData);

        var metrics = mlContext.MulticlassClassification.Evaluate(predictions, "Label");

        Console.WriteLine($"Log-loss: {metrics.LogLoss}");
        Console.WriteLine($"Macro accuracy: {metrics.MacroAccuracy}");
        Console.WriteLine($"Micro accuracy: {metrics.MicroAccuracy}");
    }
}