using System;
using Xunit;
using FileConverter;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileConverterTestSuite
{
    public class FileConverterTest
    {
        private readonly FFV2CSVConverter _ffv2csvConverter; 
        private readonly ILogger<FFV2CSVConverter> _logger;
        private readonly IConfigurationRoot _config;
        private static string _metadataFilePath;
        private static string _inputFilePath;
        private static string _outputFilePath;
       

        public FileConverterTest()
        {
            var serviceCollection = new ServiceCollection();

            _config = new ConfigurationBuilder()
               .SetBasePath(AppContext.BaseDirectory)
               .AddJsonFile("appsettings.test.json", false)
               .Build();

            serviceCollection.AddSingleton(_config);

            serviceCollection.AddLogging(configure => configure.AddConsole())
            .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information)
            .AddTransient<FFV2CSVConverter>();

             var serviceProvider = serviceCollection.BuildServiceProvider();

            _ffv2csvConverter = serviceProvider.GetService<FFV2CSVConverter>();
            _metadataFilePath = _config["MetadataFilePath"];
            _inputFilePath = _config["InputFilePath"];
            _outputFilePath = _config["OutputFilePath"];            
        }


        [Fact]
        public void Is_Metadata_List_Empty()
        {
            var result = _ffv2csvConverter.ReadMetadata(_metadataFilePath);

            Assert.True(result.Count != 0, "Metadata list is not empty.");
        }

        [Fact]
        public void Is_CSV_File_Processed_Successfully()        
        {
            var metadataLst = _ffv2csvConverter.ReadMetadata(_metadataFilePath);
            
            var result = _ffv2csvConverter.ProcessFile(metadataLst, _inputFilePath, _outputFilePath);            
        
            Assert.True(result, "CSV file is processed successfully.");
        }

        [Fact]
        public void Is_InputFile_Matches_Metadata_FileStructure()
        {
            string _badFormatInputFilePath = _config["InputFileNotMatchingMetadataStructure"];
            var metadataLst = _ffv2csvConverter.ReadMetadata(_metadataFilePath);

            Action act = () => _ffv2csvConverter.ProcessFile(metadataLst, _badFormatInputFilePath, _outputFilePath);

            Assert.Throws<ArgumentOutOfRangeException>(act);
        }

        [Fact]
        public void Is_InputFile_Format_NotOk()
        {
            string _badFormatInputFilePath = _config["BadFormatInputFilePath"];
            var metadataLst = _ffv2csvConverter.ReadMetadata(_metadataFilePath);
            Action act = () => _ffv2csvConverter.ProcessFile(metadataLst, _badFormatInputFilePath, _outputFilePath);
            
            Assert.Throws<FormatException>(act);            
        }
    }
}
