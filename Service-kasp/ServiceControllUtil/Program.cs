using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServiceControllUtil
{

    internal class Program
    {
#if DEBUG
        const string ServiceName = "Korolko-Kaspersky-test-filescanner-dev";
#else
    public static const serviceName= "Korolko-Kaspersky-test-filescanner-";
#endif
        static ServiceController service;
        public static void Main(string[] args)
        {
            try
            {
                service = new ServiceController(ServiceName);
            } 
            catch (Exception)
            {
                Console.WriteLine("Service is not installed");
            }
            switch (args.Length)
            {
                case 0:
                    SwitchServiceState();
                    break;
                case 2:
                    if (service.Status != ServiceControllerStatus.Running)
                    {
                        Console.WriteLine("Service is not running");
                        break;
                    }
                    switch (args[0])
                    {
                        case "scan":
                            Console.WriteLine(ScanServiceNegotiator.SendScanRequest(args[1]));
                            break;
                        case "status":
                            int id;
                            if (int.TryParse(args[1], out id))
                            {
                                Console.WriteLine(ScanServiceNegotiator.GetScanResult(id));
                            }
                            else
                            {
                                Console.WriteLine("id should be numerik");
                            }
                            break;

                        default:
                            Console.WriteLine("Unknown comand");
                            break;
                    }
                    break;
                default:
                    Console.WriteLine("Unknown comand");
                    break;
            }
        }
        public static void SwitchServiceState()
        {
            if (service.Status == ServiceControllerStatus.Running)
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped,TimeSpan.FromSeconds(10));
                if (service.Status!= ServiceControllerStatus.Stopped)
                {
                    Console.WriteLine("Something gone wrong. Service is not stopped.");
                }
                else
                {
                    Console.WriteLine("Scan service was stopped.");
                }
            }
            else if(service.Status == ServiceControllerStatus.Stopped)
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                if (service.Status != ServiceControllerStatus.Running)
                {
                    Console.WriteLine("Something gone wrong. Service is not running.");
                }
                else
                {
                    Console.WriteLine("Scan service was started.");
                }
            }
        }
    }
}
