﻿using System.Text.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using Service_kasp.Interface;
using Service_kasp.Models;
namespace Service_kasp.Services
{


    public class FileScannerService : IFileScanner
    {



        readonly Dictionary<string, string[]> susStrings = new Dictionary<string, string[]>();

        public FileScannerService(IConfiguration configuration)
        {
            if (!File.Exists(configuration["susStingsFilePath"]))
            {
                throw new Exception($"{configuration["susStingsFilePath"]} does not exist");
            }
            string jsonString = File.ReadAllText(configuration["susStingsFilePath"]);
            susStrings = JsonSerializer.Deserialize<Dictionary<string, string[]>>(jsonString);

        }



        async public Task<ScanResult> ScanDirectoryAsync(string path)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            ScanResult result = new ScanResult
            {
                ScanRecords = new ConcurrentDictionary<string, int>()
            };

            await ScanDirectoryAsync(path, result);
            stopwatch.Stop();
            result.TimeSpent = stopwatch.Elapsed.TotalSeconds;
            return result;
        }
        async Task ScanDirectoryAsync(string path, ScanResult scanResult)
        {
            List<Task> tasks = new List<Task>();

            try
            {
                foreach (string directory in Directory.EnumerateDirectories(path))
                {
                    tasks.Add(Task.Run(() => ScanDirectoryAsync(directory, scanResult)));
                }
                foreach (string file in Directory.EnumerateFiles(path))
                {
                    tasks.Add(Task.Run(() => ScanFile(file, scanResult)));
                }
            }
            catch (Exception)
            {
                lock (scanResult)
                {
                    scanResult.ErrorCount += 1;
                }
            }

            await Task.WhenAll(tasks);
        }
        void ScanFile(string path, ScanResult scanResult)//сделать что-то с загрузкой диска при чтении
        {
            lock (scanResult)
            {
                scanResult.FilesCount += 1;
            }
            try
            {
                string extention = Path.GetExtension(path);
                bool hasSpecialLines = susStrings.ContainsKey(extention);//переименовать

                foreach (string line in File.ReadLines(path))
                {

                    if (line == null) break;
                    string? res = susStrings["*"].FirstOrDefault((str) => line.Contains(str));
                    if (res is not null)
                    {
                        scanResult.ScanRecords.AddOrUpdate(res, 1, (key, value) => value + 1);
                        break;
                    }
                    if (!hasSpecialLines) continue;
                    res = susStrings[extention].FirstOrDefault((str) => line.Contains(str));
                    if (res is not null)
                    {
                        scanResult.ScanRecords.AddOrUpdate(res, 1, (key, value) => value + 1);
                        break;
                    }


                }
            }
            catch (Exception)
            {
                lock (scanResult)
                {
                    scanResult.ErrorCount += 1;
                }

            }


        }

    }
}