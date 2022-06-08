using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Text.Json;

namespace Service_kasp.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]

    public  class FileScannerController : Controller
    {
        FileScanner fileScanner;
        static Dictionary<int, Task<Dictionary<ScanRecord, int>>> ScanTasks = new Dictionary<int, Task<Dictionary<ScanRecord, int>>>();//переименовать
        static int lastId = 0;
        public FileScannerController(IConfiguration configuration)
        {
            fileScanner = new FileScanner(configuration["susStingsFilePath"]);
        }

        // GET: FileScannerController
        [HttpGet]
        public IActionResult ScanFiles(string path)
        {
            lastId++;
            ScanTasks.Add(lastId, (fileScanner.ScanDirectoryAsync(path)));

            return new OkObjectResult(lastId);
        }
        [HttpGet]
        public IActionResult GetScanResults(int id)
        {
            return new OkResult();
        }
    }
}
