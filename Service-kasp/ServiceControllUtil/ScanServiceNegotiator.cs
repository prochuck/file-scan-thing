using Service_kasp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
namespace ServiceControllUtil
{

    static class ScanServiceNegotiator
    {
        const string ShorteningsFileName = "shortenings.json";
        static readonly Dictionary<string, string> shortenings;
        static ScanServiceNegotiator()
        {
            shortenings = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(ShorteningsFileName));
        }

        public static string SendScanRequest(string path)
        {
            HttpClient client = new HttpClient();
            UriBuilder uriBuilder = new UriBuilder()
            {
                Scheme = "http",
                Host = "127.0.0.1",
                Port = 36458,
                Path = "FileScanner/ScanFiles",
                Query = $"path={path}"
            };


            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                response = client.GetAsync(uriBuilder.Uri).Result;
                return response.Content.ReadAsStringAsync().Result;//переделать добавление атрибута
            }
            catch (Exception ex)
            {
                return "Что-то пошло не так";//переделать на английский всё
            }


        }
        public static string GetScanResult(int id)
        {
            HttpClient client = new HttpClient();

            HttpResponseMessage response = new HttpResponseMessage();
            UriBuilder uriBuilder = new UriBuilder()
            {
                Scheme = "http",
                Host = "127.0.0.1",
                Port = 36458,
                Path = "FileScanner/GetScanResults",
                Query=$"id={id}"
                
            };
            try
            {

                response = client.GetAsync(uriBuilder.Uri ).Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ScanResult scanResult = JsonSerializer.Deserialize<ScanResult>(response.Content.ReadAsStringAsync().Result);
                    return ScanResultToString(scanResult);
                }
                return response.Content.ReadAsStringAsync().Result;//переделать добавление атрибута
            }
            catch (Exception ex)
            {
                return "Что-то пошло не так";//переделать на английский всё
            }


        }


        static string ScanResultToString(ScanResult scanResult)
        {
            string result = "====== Scan result ======";
            result += $"Directory: {scanResult.Directory}\n";
            result += $"Processed files: {scanResult.FilesCount}\n";

            foreach (KeyValuePair<string, int> scanRecord in scanResult.ScanRecords)
            {
                if (shortenings.ContainsKey(scanRecord.Key))
                {
                    result += $"{shortenings[scanRecord.Key]} detects: {scanRecord.Value}\n";
                }
                else
                {
                    result += $"{scanRecord.Key} detects: {scanRecord.Value}\n";
                }
            }
            result += $"Errors: {scanResult.ErrorCount}\n";
            TimeSpan timeSpan = TimeSpan.FromTicks((long)scanResult.TimeSpent);
            result += $"Exection time: {timeSpan.ToString(@"ss")}\n";
            return result;
        }

    }
}
