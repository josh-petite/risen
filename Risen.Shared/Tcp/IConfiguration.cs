namespace Risen.Shared.Tcp
{
    public interface IConfiguration 
    {
        int GetTotalBytesRequiredForInitialBufferConfiguration();
        int GetTotalBufferSize();
        string LogQueue { get; }
    }
}