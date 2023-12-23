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
    
}

public class SanctionsDataLoader : ISanctionsDataLoader
{
    private readonly ILogger<SanctionsDataLoader> _logger;
    private readonly IEnumerable<SanctionsListConfig> _sanctionsListConfigs;
    private IReadOnlyList<string[]> _sanctionsDataGb;
    private IReadOnlyList<string[]> _sanctionsDataEu;
    private IReadOnlyList<string[]> _sanctionsDataUsa;
    
    public SanctionsDataLoader(ILogger<SanctionsDataLoader> logger,
        IOptionsMonitor<IEnumerable<SanctionsListConfig>> sanctionsListConfigs)
    {
        _logger = logger;
        _sanctionsListConfigs = sanctionsListConfigs.CurrentValue;

    }

    private async IAsyncEnumerable<string[]> LoadSanctionsData(string area)
    {
        var _reportParams = _sanctionsListConfigs.SingleOrDefault(x => x.Area == area) ??
                        throw new ConfigIncorrectException("there was a problem reading the configuration");

        using var fileReader = new StreamReader(_reportParams.FileName, Encoding.GetEncoding(_reportParams.Encoding));
        using var parser = SetupCsvParser(fileReader);
        
        var i = 0;
        while (await parser.ReadAsync())
        {
            yield return ProcessRow(parser.Record!, ++i);
        }
    }

    public async IAsyncEnumerable<string[]> Execute(string area)
    {
        _sanctionsDataGb = await LoadSanctionsData("gb");
        _sanctionsDataEu = await LoadSanctionsData("eu");
        _sanctionsDataUsa = await LoadSanctionsData("usa");
        
        switch (area)
        {
            case "gb":
            {
                foreach (var row in _sanctionsDataGb)
                {
                    yield return row;
                }

                break;
            }
            case "eu":
            {
                foreach (var row in _sanctionsDataEu)
                {
                    yield return row;
                }

                break;
            }
            case "usa":
            {
                foreach (var row in _sanctionsDataUsa)
                {
                    yield return row;
                }

                break;
            }
            default:
                throw new ArgumentException("Invalid area");
        }
        
        
 
    }

    private void ProcessRow(string[] row, int rowIndex)
    {
        //TODO: remove this comment
        if (row == null) return;

        if (rowIndex == _reportParams.MetaDataIndex)
            _reportContainer.Report.ResultSummary.MetaData = String.Join(" ", row).Replace(" ", "");

        if (rowIndex == _reportParams.HeaderIndex)
            _reportParams.HeaderFields.AddRange(row);

        if (_simpleNameMatcher.Execute(_fullNames, row))
        {
            AddRowToReport(row);
        }
    }

}