using System.Text.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using Service_kasp.Interface;
using Service_kasp.Models;
namespace Service_kasp.Services
{


    public class FileScannerService : IFileScanner
    {
        //доделать ошибки доступа



        readonly Dictionary<string, string[]> susStrings;

        public FileScannerService(IConfiguration configuration)
        {
            susStrings = JsonSerializer.Deserialize<Dictionary<string, string[]>>(System.IO.File.ReadAllText(configuration["susStingsFilePath"]));
        }

        async public Task<ScanResult> ScanDirectoryAsync(string path)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            ScanResult result = new ScanResult();

            result.scanRecords = new ConcurrentDictionary<string, int>();


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
                foreach (string file in Directory.EnumerateFiles(path))
                {
                    tasks.Add(ScanFileAsync(file, scanResult));
                }
                foreach (string directory in Directory.EnumerateDirectories(path))
                {
                    tasks.Add(ScanDirectoryAsync(directory, scanResult));
                }
            }
            catch (Exception)
            {
                lock (scanResult)//переделать
                {
                    scanResult.ErrorCount += 1;
                }
            }

            await Task.WhenAll(tasks);//обработать токены ошибок в тасках
        }
        async Task ScanFileAsync(string path, ScanResult scanResult)
        {
            lock (scanResult)//переделать
            {
                scanResult.FilesCount += 1;
            }
            await Task.Delay(1000);
            try
            {
                string extention = Path.GetExtension(path);
                bool hasSpecialLines = susStrings.ContainsKey(extention);//переименовать
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        while (!reader.EndOfStream)
                        {
                            string? line = reader.ReadLine();
                            if (line == null) break;
                            string? res = susStrings["*"].FirstOrDefault((str) => line.Contains(str));
                            if (!(res is null))
                            {
                                scanResult.scanRecords.AddOrUpdate(res, 1, (key, value) => value + 1);
                                break;
                            }
                            if (!hasSpecialLines) continue;
                            res = susStrings[extention].FirstOrDefault((str) => line.Contains(str));
                            if (!(res is null))
                            {
                                scanResult.scanRecords.AddOrUpdate(res, 1, (key, value) => value + 1);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                lock (scanResult)//переделать
                {
                    scanResult.ErrorCount += 1;
                }

            }


        }

    }
}
