namespace QA_VS;

public interface ILogger
{
    void Log(string message);
    void Log(FileAction action, string message);
}