using Service_kasp.Interface;
using Service_kasp.Services;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.AspNetCore;

namespace Service_kasp.Controllers
{
    
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().RunAsService();
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseContentRoot(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName))
                .UseStartup<Startup>()
            .UseUrls("http://127.0.0.1:36458/")//передеалть
                                               //востановить https
            ;
    }
}