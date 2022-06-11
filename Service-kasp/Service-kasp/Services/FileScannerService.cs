using System.Text.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using Service_kasp.Interface;
using Service_kasp.Models;
namespace Service_kasp.Services
{

    /// <summary>
    /// Служба, отвечающая за проведение сканирования директорий
    /// </summary>
    public class FileScannerService : IFileScanner
    {
        /// <summary>
        /// Список подозрительных строк
        /// </summary>
        readonly Dictionary<string, string[]> susStrings = new Dictionary<string, string[]>();

        /// <summary>
        /// Получает имя json файла с подозрительными строками.
        /// </summary>
        /// <param name="hostEnvironment"></param>
        /// <param name="configuration"></param>
        /// <exception cref="Exception"></exception>
        public FileScannerService(IWebHostEnvironment hostEnvironment,IConfiguration configuration)
        {
            //Получает имя json файла с подозрительными строками и записывает его в переменную susString
            if (!File.Exists(hostEnvironment.ContentRootPath+"\\"+configuration["SusStingsFilePath"]))
            {
                throw new Exception($"File {hostEnvironment.ContentRootPath + "\\" + configuration["SusStingsFilePath"]} does not exist");
            }
            string jsonString = File.ReadAllText(hostEnvironment.ContentRootPath + "\\" + configuration["SusStingsFilePath"]);
            susStrings = JsonSerializer.Deserialize<Dictionary<string, string[]>>(jsonString);
        }


        /// <summary>
        /// Запуск асинхронного сканирования директории.
        /// </summary>
        /// <param name="path">Путь к директории для сканирования</param> 
        /// <returns></returns>
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
        /// <summary>
        /// Проводит асинхонное сканирование директории.
        /// </summary>
        /// <param name="path">Путь к директории для сканирования</param>
        /// <param name="scanResult">Переменная, в которую будет записан результат сканирования</param>
        /// <returns></returns>
        async Task ScanDirectoryAsync(string path, ScanResult scanResult)
        {
            //проводим итеративный обход всех файлов и папок в ширину
            List<Task> tasks = new List<Task>();
            Queue<string> pathesToProcess = new Queue<string>() ;
            pathesToProcess.Enqueue(path);
            while (pathesToProcess.Count!=0)
            {
                string curentPath=pathesToProcess.Dequeue();
                try
                {
                    //каждая директория добавляется в очередь на обход
                    foreach (string directory in Directory.EnumerateDirectories(curentPath))
                    {
                        pathesToProcess.Enqueue(directory);
                    }
                    //для каждого файла запускается сканирование
                    foreach (string file in Directory.EnumerateFiles(curentPath))
                    {
                        tasks.Add(Task.Run(() => ScanFile(file, scanResult)));
                    }
                }
                catch (Exception)
                {
                    //в случае ошибки доступа к директории счётчик количества ошибок увеличиваетсяч
                    lock (scanResult)
                    {
                        scanResult.ErrorCount += 1;
                    }
                }
            }
            //ожидание завершения сканирования файлов
            await Task.WhenAll(tasks);
        }
        /// <summary>
        /// Проводит поиск подозрительных строк в файле
        /// </summary>
        /// <param name="path">Путь к файле</param>
        /// <param name="scanResult">Переменная, в которую будет записан результат сканирования</param>
        void ScanFile(string path, ScanResult scanResult)
        {
            lock (scanResult)
            {
                scanResult.FilesCount += 1;
            }
            try
            {
                //определение того, содердатся ли в файле подозрительных строк, подозрительная строка для файла с таким расширением
                string extention = Path.GetExtension(path);
                bool hasSpecialLines = susStrings.ContainsKey(extention);

                foreach (string line in File.ReadLines(path))
                {

                    if (line == null) break;
                    //ищет в строке подозрительные подстроки, относящиеся ко всем типам файлов
                    string? res = susStrings["*"].FirstOrDefault((str) => line.Contains(str));
                    if (res is not null)
                    {
                        scanResult.ScanRecords.AddOrUpdate(res, 1, (key, value) => value + 1);
                        break;
                    }
                    if (!hasSpecialLines) continue;
                    //ищет в строке подозрительные подстроки, относящиеся к типу файла скинруемого файла
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
