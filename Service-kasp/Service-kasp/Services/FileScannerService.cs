using System.Text.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using Service_kasp.Interface;
using Service_kasp.Models;
namespace Service_kasp.Services
{


    public class FileScannerService : IFileScanner
    {



        readonly Dictionary<string, string[]> susStrings = new Dictionary<string, string[]>();

        public FileScannerService(IWebHostEnvironment hostEnvironment,IConfiguration configuration)
        {
            
            if (!File.Exists(hostEnvironment.ContentRootPath+"\\"+configuration["SusStingsFilePath"]))
            {
                throw new Exception($"File {hostEnvironment.ContentRootPath + "\\" + configuration["SusStingsFilePath"]} does not exist");
            }
            string jsonString = File.ReadAllText(hostEnvironment.ContentRootPath + "\\" + configuration["SusStingsFilePath"]);
            susStrings = JsonSerializer.Deserialize<Dictionary<string, string[]>>(jsonString);

        }



        async public Task<ScanResult> ScanDirectoryAsync(string path)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            ScanResult result = new ScanResult
            {
                ScanRecords = new ConcurrentDictionary<string, int>(),
                Directory = path
            };

            await ScanDirectoryAsync(path, result);
            stopwatch.Stop();
            result.TimeSpent = stopwatch.Elapsed.Ticks;
            return result;
        }
        async Task ScanDirectoryAsync(string path, ScanResult scanResult)
        {
            List<Task> tasks = new List<Task>();
            Queue<string> pathesToProcess = new Queue<string>() ;
            pathesToProcess.Enqueue(path);
            while (pathesToProcess.Count!=0)
            {
                string curentPath=pathesToProcess.Dequeue();
                try
                {
                    
                    foreach (string directory in Directory.EnumerateDirectories(curentPath))
                    {
                        pathesToProcess.Enqueue(directory);
                    }
                    foreach (string file in Directory.EnumerateFiles(curentPath))
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
