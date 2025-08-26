namespace QA_VS;

public class Logger : ILogger
{
    readonly string logPath;
    readonly string logFile = "log.txt";

    public Logger(string logPath)
    {
        this.logPath = logPath;
    }

    public void Log(FileAction fileAction, string message)
    {
        string entry = $"{fileAction} {message}";
        LogEntry(entry);
    }

    public void Log(string message)
    {
        LogEntry(message);
    }

    void LogEntry(string entry)
    {
        Directory.CreateDirectory(logPath);
        string logFilePath = Path.Combine(logPath, logFile);
        string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string combinedEntry = $"{date} {entry}";
        Console.WriteLine(combinedEntry);
        File.AppendAllText(logFilePath, combinedEntry + Environment.NewLine);
    }
}