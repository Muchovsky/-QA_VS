namespace QA_VS;

public enum FileAction
{
    Added,
    Removed,
    Replaced
}

public static class FileActionExtensions
{
    public static string LoggString(this FileAction action)
    {
        return action switch
        {
            FileAction.Added => "[ADD]",
            FileAction.Removed => "[REMOVE]",
            FileAction.Replaced => "[REPLACE]",
            _ => action.ToString()
        };
    }
}