using System.Collections.Concurrent;

namespace Service_kasp.Models
{
    public class ScanResult
    {
        public int FilesCount { get; set; }
        private int _fileCount = 0;
        public double TimeSpent { get; set; }
        public int ErrorCount { get; set; }
        private int _errorCount;
        public ConcurrentDictionary<string, int> scanRecords { get; set; } = new ConcurrentDictionary<string, int>();
    }
}
