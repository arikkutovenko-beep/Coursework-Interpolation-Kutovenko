using cursova_code.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace cursova_code.IO
{
    public class CalculationArchive
    {
        public string Method { get; set; }
        public string CalculationDate { get; set; }
        public List<PointModel> InputPoints { get; set; }
        public string Result { get; set; }

        public object Coefficients { get; set; }
    }

    public class FileService
    {
        public static void SaveResult(string filePath, List<PointModel> points, string methodName, string analyticExpression, object coefficients)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var archive = new CalculationArchive
                {
                    CalculationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Method = methodName,
                    InputPoints = points,
                    Result = analyticExpression,
                    Coefficients = coefficients?.ToString()
                };

                string jsonString = JsonSerializer.Serialize(archive, options);
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during saving file: {ex.Message}");
            }
        }

        public List<PointModel> LoadPoints(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new List<PointModel>();
                }

                string jsonString = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(jsonString)) return new List<PointModel>();

                using (JsonDocument doc = JsonDocument.Parse(jsonString))
                {
                    if (doc.RootElement.TryGetProperty("InputPoints", out _))
                    {
                        var archive = JsonSerializer.Deserialize<CalculationArchive>(jsonString);
                        return archive?.InputPoints ?? new List<PointModel>();
                    }
                    return JsonSerializer.Deserialize<List<PointModel>>(jsonString) ?? new List<PointModel>();
                }
            }
            catch (JsonException)
            {
                return new List<PointModel>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during reading points from file: {ex.Message}");
            }
        }

        public CalculationArchive LoadAllArchive(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return null;
                }
                string jsonString = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<CalculationArchive>(jsonString);
            }
            catch (Exception ex)
            {
                throw new Exception($"Something wrong with reading archive: {ex.Message}");
            }
        }
    }
}