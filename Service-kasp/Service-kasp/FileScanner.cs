using System.Text.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using Service_kasp.Interface;

namespace Service_kasp
{
    public struct ScanResult
    {
        public int filesCount;
        public int timeSpent;
        public int errorCount;
        public Task<Dictionary<ScanRecord, int>> scanRecords;
    }
    public struct ScanRecord//переименовать
    {
        public string fileExtention;
        public string susString;
    }
    public class FileScanner : IFileScanner
    {
        //доделать ошибки доступа


        static readonly ScanRecord errorScanResult = new ScanRecord() { fileExtention = null, susString = "Error" };
        readonly Dictionary<string, string[]> susStrings;

        public FileScanner(string susStringsPath)
        {
            susStrings = JsonSerializer.Deserialize<Dictionary<string, string[]>>(System.IO.File.ReadAllText(susStringsPath));

        }

        async public Task<Dictionary<ScanRecord, int>> ScanDirectoryAsync(string path)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            ConcurrentDictionary<ScanRecord, int> dictionary = new ConcurrentDictionary<ScanRecord, int>();


            await ScanDirectoryAsync(path, dictionary);
            stopwatch.Stop();
            dictionary.TryAdd(new ScanRecord() { fileExtention = "время", susString = "деньги" }, stopwatch.Elapsed.Seconds);//удалииить
            return dictionary.ToDictionary((pair) => pair.Key, (pair) => pair.Value);
        }
        async Task ScanDirectoryAsync(string path, ConcurrentDictionary<ScanRecord, int> dict)
        {
            List<Task> tasks = new List<Task>();

            try
            {
                string[] filePathes = Directory.EnumerateFiles(path).ToArray();
                string[] directoryPathes = Directory.EnumerateDirectories(path).ToArray();
                foreach (string file in Directory.EnumerateFiles(path))
                {
                    tasks.Add(ScanFileAsync(file, dict));
                }
                foreach (string directory in Directory.EnumerateDirectories(path))
                {
                    tasks.Add(ScanDirectoryAsync(directory, dict));
                }
            }
            catch (Exception)
            {
                dict.AddOrUpdate(errorScanResult, 1, (key, value) => value + 1);
            }

            await Task.WhenAll(tasks);//обработать токены ошибок в тасках
        }
        async Task ScanFileAsync(string path, ConcurrentDictionary<ScanRecord, int> dict)
        {
            try
            {
                string extention = Path.GetExtension(path);
                if (extention is null)
                {
                    Console.WriteLine(123123);
                }
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
                                dict.AddOrUpdate(new ScanRecord() { fileExtention = "*", susString = res }, 1, (key, value) => value + 1);
                                break;
                            }
                            if (!hasSpecialLines) continue;
                            res = susStrings[extention].FirstOrDefault((str) => line.Contains(str));
                            if (!(res is null))
                            {
                                dict.AddOrUpdate(new ScanRecord() { fileExtention = extention, susString = res }, 1, (key, value) => value + 1);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                dict.AddOrUpdate(errorScanResult, 1, (key, value) => value + 1);
            }


        }

    }
}
