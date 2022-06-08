using System.Collections.Concurrent;

namespace Service_kasp.Models
{
    public class ScanResult
    {
        public int FilesCount { get; set; }
        public double TimeSpent { get; set; }
        public int ErrorCount { get; set; }
        public ConcurrentDictionary<string, int> ScanRecords { get; set; } = new ConcurrentDictionary<string, int>();
    }
}
