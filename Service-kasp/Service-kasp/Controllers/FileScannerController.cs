using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Text.Json;
using Service_kasp.Interface;
using Service_kasp.Services;
using Service_kasp.Models;
using System.Collections.Concurrent;


namespace Service_kasp.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    
    public class FileScannerController : Controller
    {
        /// <summary>
        /// Используемый в контроллере файловый сканнер
        /// </summary>
        IFileScanner fileScanner;
        /// <summary>
        /// Список всех задач сканирования
        /// </summary>
        public static Dictionary<int, Task<ScanResult>> scanTasks = new Dictionary<int, Task<ScanResult>>();
        static int lastId = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileScanner">Сканер, который будет использоваться при сканировании директории</param>
        public FileScannerController(IFileScanner fileScanner)
        {
            this.fileScanner = fileScanner;
        }
        /// <summary>
        /// Запускает задачу сканирования директориии и сохраняет её с определённыи id в 
        /// </summary>
        /// <param name="path">Путь к сканируемой директории</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ScanFiles(string path)
        {
            if (!Directory.Exists(path))
            {
                return new BadRequestObjectResult("Directory does not exist");
            }
            lock (scanTasks)
            {
                lastId++;
                scanTasks.Add(lastId, Task<ScanResult>.Run(() => fileScanner.ScanDirectoryAsync(path)));
            }
            return new OkObjectResult($"Scan task was created with ID: {lastId}");
        }
        /// <summary>
        /// Возвращает статус задачи сканирования
        /// </summary>
        /// <param name="id">id задачи сканирования</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetScanResults(int id)
        {
            if (scanTasks.ContainsKey(id))
            {
                if (scanTasks[id].IsCompletedSuccessfully)
                {
                    JsonResult jsonResult = new JsonResult(scanTasks[id].Result)
                    {
                        ContentType = "application/json",
                        SerializerSettings = new JsonSerializerOptions(),
                        StatusCode = 200
                    };
                    return jsonResult;
                }
                else
                {
                    return new BadRequestObjectResult("Scan task in progress, please wait");
                }
            }
            return new BadRequestObjectResult("Scan task not found");
        }
    }
}
