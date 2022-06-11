using Service_kasp.Interface;
using Service_kasp.Services;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting.WindowsServices;

namespace Service_kasp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSingleton<IFileScanner, FileScannerService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
