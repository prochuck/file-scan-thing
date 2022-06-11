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
        IFileScanner fileScanner;

        //приколы с многопоточностью
        public static Dictionary<int, Task<ScanResult>> ScanTasks = new Dictionary<int, Task<ScanResult>>();//переименовать
        static int lastId = 0;

        public FileScannerController(IFileScanner fileScanner)
        {
            this.fileScanner = fileScanner;
        }

        // GET: FileScannerController
        [HttpGet]
        public IActionResult ScanFiles(string path)
        {
            if (!Directory.Exists(path))
            {
                return new BadRequestObjectResult("Directory does not exist");
            }
            lock (ScanTasks)
            {
                lastId++;
                ScanTasks.Add(lastId, Task<ScanResult>.Run(() => fileScanner.ScanDirectoryAsync(path)));
            }
            return new OkObjectResult($"Scan task was created with ID: {lastId}");
        }
        [HttpGet]
        public IActionResult GetScanResults(int id)
        {
            if (ScanTasks.ContainsKey(id))
            {
                if (ScanTasks[id].IsCompletedSuccessfully)//доделать
                {
                    JsonResult jsonResult = new JsonResult(ScanTasks[id].Result)
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
