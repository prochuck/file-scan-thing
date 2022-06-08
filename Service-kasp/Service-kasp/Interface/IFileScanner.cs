using Service_kasp.Models;
namespace Service_kasp.Interface
{
    public interface IFileScanner
    {
        public Task<ScanResult> ScanDirectoryAsync(string path);
    }
}
