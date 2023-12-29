using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SanctionsApi.Exceptions;
using SanctionsApi.Models;

namespace SanctionsApi.Services;

public interface ISanctionsDataLoader
{
    IAsyncEnumerable<string[]> Execute(string area);
}

public class SanctionsDataLoader : ISanctionsDataLoader
{
    private readonly ILogger<SanctionsDataLoader> _logger;
    private readonly IEnumerable<SanctionsListConfig> _sanctionsListConfigs;
    private IAsyncEnumerable<string[]> _sanctionsDataEu;
    private IAsyncEnumerable<string[]> _sanctionsDataGb;
    private IAsyncEnumerable<string[]> _sanctionsDataUsa;

    public SanctionsDataLoader(ILogger<SanctionsDataLoader> logger,
        IOptionsMonitor<List<SanctionsListConfig>> sanctionsListConfigs)
    {
        _logger = logger;
        _sanctionsListConfigs = sanctionsListConfigs.CurrentValue;
    }

    public async IAsyncEnumerable<string[]> Execute(string area)
    {
        if (!await DataExistsAsync(_sanctionsDataGb))
        {
            _sanctionsDataGb = LoadSanctionsData("gb");
        }

        if (!await DataExistsAsync(_sanctionsDataEu))
        {
            _sanctionsDataGb = LoadSanctionsData("eu");
        }

        if (!await DataExistsAsync(_sanctionsDataUsa))
        {
            _sanctionsDataGb = LoadSanctionsData("usa");
        }

        switch (area)
        {
            case "gb":
            {
                await foreach (var row in _sanctionsDataGb)
                {
                    yield return row;
                }

                break;
            }
            case "eu":
            {
                await foreach (var row in _sanctionsDataEu)
                {
                    yield return row;
                }

                break;
            }
            case "usa":
            {
                await foreach (var row in _sanctionsDataUsa)
                {
                    yield return row;
                }

                break;
            }
            default:
                throw new ArgumentException("Invalid area");
        }
    }

    private async IAsyncEnumerable<string[]> LoadSanctionsData(string area)
    {
        var _reportParams = _sanctionsListConfigs.SingleOrDefault(x => x.Area == area) ??
                            throw new ConfigIncorrectException("there was a problem reading the configuration");

        using var fileReader = new StreamReader(_reportParams.FileName, Encoding.GetEncoding(_reportParams.Encoding));
        using var parser = SetupCsvParser(fileReader, _reportParams);

        var i = 0;
        while (await parser.ReadAsync())
        {
            yield return ProcessRow(parser.Record!, ++i, _reportParams);
        }
    }

    private static async Task<bool> DataExistsAsync(IAsyncEnumerable<string[]> asyncEnumerable)
    {
        await foreach (var unused in asyncEnumerable)
        {
            return true;
        }

        return false;
    }

    private static string[] ProcessRow(string[] row, int rowIndex, SanctionsListConfig reportParams)
    {
        if (row == null) return new string[] { };

        // if (rowIndex == reportParams.MetaDataIndex)
        //     _reportContainer.Report.ResultSummary.MetaData = String.Join(" ", row).Replace(" ", "");
        //
        // if (rowIndex == reportParams.HeaderIndex)
        //     reportParams.HeaderFields.AddRange(row);

        return row;
    }

    private static CsvParser SetupCsvParser(TextReader fileReader, SanctionsListConfig reportParams)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = reportParams.Delimiter,
            BadDataFound = null
        };

        return new CsvParser(fileReader, config);
    }
}