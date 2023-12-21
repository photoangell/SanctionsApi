using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using SanctionsApi.Models;

namespace SanctionsApi.Services;

public class MLNameMatcher : INameMatcher
{
    private readonly ILogger<MLNameMatcher> _logger;

    public MLNameMatcher(ILogger<MLNameMatcher> logger)
    {
        _logger = logger;
    }
    
    public bool Execute(IEnumerable<FullName> fullNames, IReadOnlyList<string> row)
    {
        // Prepare your data
        var mlContext = new MLContext();
        var transformedData = PrepareData(mlContext, fullNames, row);
    
        // Now you can compare names in a more sophisticated way
        var data = mlContext.Data.CreateEnumerable<TransformedData>(transformedData, reuseRowObject: false);
        double threshold = 0.9;
        
        foreach (var item in data)
        {
            float similarity = CalculateCosineSimilarity(item.Features, item.RowFeatures);
            
            if (similarity > threshold) // Choose a threshold that suits you
            {
                //_logger.LogInformation("Found a match: {fullName}", item.FullName);
                return true;
            }
        }
    
        //_logger.LogInformation("No match found");
        return false; // Or true depending on your comparison logic
    }
    
    public IDataView PrepareData(MLContext mlContext, IEnumerable<FullName> fullNames, IReadOnlyList<string> row)
    {
        // Transform the data into a format that ML.net can work with
        var data = new List<InputData>();
        foreach (var name in fullNames)
        {
            data.Add(new InputData { FullName = name.ToString(), RowString = string.Join(" ", row) });
        }

        // Load the data into IDataView
        var dataView = mlContext.Data.LoadFromEnumerable(data);

        // Define the pipeline for feature extraction
        var pipeline = mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", inputColumnName: "FullName")
            .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "RowFeatures", inputColumnName: "RowString"));

        // Transform the data
        var transformedData = pipeline.Fit(dataView).Transform(dataView);

        return transformedData;
    }
    
    private static float CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
    {
        int length = vectorA.Length < vectorB.Length ? vectorA.Length : vectorB.Length;

        float dotProduct = 0;
        float magnitudeA = 0;
        float magnitudeB = 0;

        for (int i = 0; i < length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        float magnitudeAMagnitudeB = (float)Math.Sqrt(magnitudeA) * (float)Math.Sqrt(magnitudeB);
        float similarity = dotProduct / magnitudeAMagnitudeB;
        return similarity;
    }
}

public class InputData
{
    public string FullName { get; set; }
    public string RowString { get; set; }
}

public class TransformedData : InputData
{
    public float[] Features { get; set; }
    public float[] RowFeatures { get; set; }
}

