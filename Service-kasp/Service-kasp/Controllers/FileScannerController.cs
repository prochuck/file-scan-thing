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

    public  class FileScannerController : Controller
    {
        IFileScanner fileScanner;

        //приколы с многопоточностью
        static Dictionary<int, Task<ScanResult>> ScanTasks = new Dictionary<int, Task<ScanResult>>();//переименовать
        static int lastId = 0;

        public FileScannerController(IFileScanner fileScanner)
        {
            this.fileScanner = fileScanner;
        }

        // GET: FileScannerController
        [HttpGet]
        public async IActionResult ScanFiles(string path)
        {
            lastId++;//приколы с многопоточностью
            ScanTasks.Add(lastId, (fileScanner.ScanDirectoryAsync(path)));

            return new OkObjectResult(lastId);
        }
        [HttpGet]
        public IActionResult GetScanResults(int id)
        {
            if (ScanTasks.ContainsKey(id))
            {
                if (ScanTasks[id].IsCompletedSuccessfully)//доделать
                {
                    JsonResult jsonResult = new JsonResult(ScanTasks[id].Result);
                    jsonResult.ContentType = "application/json";
                    jsonResult.SerializerSettings = new   JsonSerializerOptions();
                    jsonResult.StatusCode = 200;
                    return Json(jsonResult);
                }
                else
                {
                    return new BadRequestObjectResult("task in progress");
                }
            }
            return new BadRequestObjectResult("not found");
        }
    }
}
