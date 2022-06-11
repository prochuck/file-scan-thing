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
        /// <summary>
        /// Константа, хранящая имя файла с сокращениями
        /// </summary>
        const string ShorteningsFileName = "shortenings.json";
        /// <summary>
        /// Словарь сокращений подозрительных строк
        /// </summary>
        static readonly Dictionary<string, string> shortenings;
        static ScanServiceNegotiator()
        {
            shortenings = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(ShorteningsFileName));
        }
        /// <summary>
        /// отправляет службе запрос на сканирование директории
        /// </summary>
        /// <param name="path">Путь к директории</param>
        /// <returns></returns>
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
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                return "Something gone wrong";
            }


        }
        /// <summary>
        /// отправляет службе запрос на получение информации о сканировании
        /// </summary>
        /// <param name="id">id задачи сканирования</param>
        /// <returns></returns>
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
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                return "Something gone wrong";
            }


        }

        /// <summary>
        /// Перевод ScanResult в строковый вид
        /// </summary>
        /// <param name="scanResult"></param>
        /// <returns></returns>
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
