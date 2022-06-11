using System.Collections.Concurrent;

namespace Service_kasp.Models
{
    public class ScanResult
    {
        public string Directory { get; set; }
        public int FilesCount { get; set; }
        public long TimeSpent { get; set; }
        public int ErrorCount { get; set; }
        public ConcurrentDictionary<string, int> ScanRecords { get; set; } = new ConcurrentDictionary<string, int>();
    }
}
