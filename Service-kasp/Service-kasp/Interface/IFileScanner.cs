namespace Service_kasp.Interface
{
    public interface IFileScanner
    {
        public Task<Dictionary<ScanRecord, int>> ScanDirectoryAsync(string path);
    }
}
