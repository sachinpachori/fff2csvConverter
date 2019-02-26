using FileConverter.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using static System.String;

namespace FileConverter
{
    public class FFV2CSVConverter
    {
        #region Properties and Variables
        
        private readonly ILogger<FFV2CSVConverter> _logger;
        private readonly IConfigurationRoot _config;
        string sep = "";
        string header = "";        
        List<Metadata> lstMetdata = null;
        DateTime dt = DateTime.MinValue;
        StringBuilder sb = new StringBuilder();
        #endregion

        public FFV2CSVConverter(IConfigurationRoot config, ILogger<FFV2CSVConverter> logger)
        {
            _config = config;
            _logger = logger;
        }

        public void Run()
        {
            string _metadataFilePath;
            string _inputFilePath;
            string _outputFilePath;

            _logger.LogInformation($"Running application.");

            try
            {
                _metadataFilePath = _config["MetadataFilePath"];
                _inputFilePath = _config["InputFilePath"];
                _outputFilePath = _config["OutputFilePath"];
               
                lstMetdata = ReadMetadata(_metadataFilePath);
                if (lstMetdata != null)
                {
                    if(ProcessFile(lstMetdata, _inputFilePath, _outputFilePath))
                        _logger.LogInformation($"Process completed.");
                }
                else
                    throw new Exception("Metadata not found");               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

        }

        public List<Metadata> ReadMetadata(string FileSaveWithPath) 
        {            
            string fullText;
            //string temp = "";
            try
            {
                using (StreamReader sr = new StreamReader(FileSaveWithPath))
                {
                    lstMetdata = new List<Metadata>();

                    while (!sr.EndOfStream)
                    {
                        fullText = sr.ReadToEnd().ToString(); //read full file text  
                        string[] rows = fullText.Split('\n'); //split full file text into rows                      
                        string[] metadataRow = null;

                        for (int i = 0; i < rows.Count(); i++)
                        {
                            metadataRow = rows[i].Split(',');

                            Metadata mt = new Metadata()
                            {
                                ColumnName = metadataRow[0],
                                ColumnLength = Convert.ToInt32(metadataRow[1]),
                                ColumnType = metadataRow[2]
                            };

                            if (i > 0)
                                sep = ",";

                            header = $"{header}{sep}{mt.ColumnName}";

                            lstMetdata.Add(mt);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }
           
            return lstMetdata;
        }

        public bool ProcessFile(List<Metadata> MetadataList, string InputFilePath, string OutputFilePath)
        {
            bool success = false;
            using (var fileStream = File.OpenRead(InputFilePath))
            using (var sr = new StreamReader(fileStream, Encoding.UTF8, true))
            {
                string line;
                //writing header in the file.
                File.AppendAllText(OutputFilePath, $"{header}{Environment.NewLine}");
                while ((line = sr.ReadLine()) != null)
                {
                    success = ProcessLine(MetadataList, line, OutputFilePath);
                }
            }
            return success;
        }        

        private bool ProcessLine(List<Metadata> MetadataList, string Line, string OutputFilePath)
        {
            sep = "";
            bool success = true;
            int _startPosition = 0;
            int count = 0;

            if (!IsNullOrEmpty(Line))
            {
                string temp;
                foreach (var item in MetadataList)
                {
                    if (_startPosition > 0)
                        sep = ",";

                    temp = Line.Substring(_startPosition, item.ColumnLength).Trim();

                    if (item.ColumnType.Trim().ToLowerInvariant() == "date")
                    {
                        dt = DateTime.ParseExact(temp, "yyyy-MM-dd", CultureInfo.InvariantCulture);                        
                        temp = dt.ToString("dd/MM/yyyy");
                    }

                    sb.Append(sep);
                    sb.Append(temp);
                    count = count + 1;
                    _startPosition = _startPosition + item.ColumnLength;
                }

                if (count != lstMetdata.Count())
                {
                    success = false;
                    throw new FormatException("Input file columns count does not match with the metadata file columns count.");                    
                }

                File.AppendAllText(OutputFilePath, $"{sb.ToString()}{Environment.NewLine}");
                sb.Clear();                
            }
            return success;
        }
    }
}
