using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    
    public bool ExecuteWithML(IEnumerable<FullName> fullNames, IReadOnlyList<string> row)
    {
        double threshold = 0.5;
        // Prepare your data
        var mlContext = new MLContext();
        var transformedData = PrepareData(mlContext, fullNames, row);
    
        // Now you can compare names in a more sophisticated way
        var data = mlContext.Data.CreateEnumerable<TransformedData>(transformedData, reuseRowObject: false);
        
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
    
    public bool Execute(IEnumerable<FullName> fullNames, IReadOnlyList<string> row)
    {
        var threshold = 3;
        foreach(var fullName in fullNames)
        {
            bool allPartsMatched = true;

            // Compare all name parts with the row strings
            foreach(var namePart in fullName.NameParts)
            {
                var namePartLower = namePart.ToLower();
                bool namePartMatched = false;

                foreach(var s in row)
                {
                    var sLower = s.ToLower();
                    // if (s.ToLower().Contains("naumov") && namePart.ToLower().Contains("naumov"))
                    // {
                    //     Debugger.Break();
                    //     
                    // }

                    var distance = GetLevenshteinDistance(namePartLower, sLower);
                    if(distance < threshold)
                    {
                        namePartMatched = true;
                        break; // break loop if this name part matched
                    }
                }

                // If a name part isn't matched, break the main loop
                if (!namePartMatched)
                {
                    allPartsMatched = false;
                    break;
                }
            }

            // If all parts were matched, this FullName is considered matched
            if (allPartsMatched)
            {
                return true;
            }
        }

        return false;
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
    
    public static int GetLevenshteinDistance(string string1, string string2)
    {
        if (string1 == null) throw new ArgumentNullException("string1");
        if (string2 == null) throw new ArgumentNullException("string2");
        int diff;
        var matrix = new int[string1.Length + 1, string2.Length + 1];
        for (var i = 0; i <= string1.Length; i++)
            matrix[i, 0] = i;
        for (var j = 0; j <= string2.Length; j++)
            matrix[0, j] = j;
        for (var i = 1; i <= string1.Length; i++)
        for (var j = 1; j <= string2.Length; j++)
        {
            if (string1[i - 1] == string2[j - 1])
                diff = 0;
            else
                diff = 1;
            matrix[i, j] = Math.Min(
                Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                matrix[i - 1, j - 1] + diff);
        }
        return matrix[string1.Length, string2.Length];
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

