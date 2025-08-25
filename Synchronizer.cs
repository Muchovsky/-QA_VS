namespace QA_VS;

public class Synchronizer
{
    string sourcePath;
    string replicaPath;

    public void Synchronize(string sourcePath, string replicaPath)
    {
        Console.WriteLine($"Try Sync from path {sourcePath} to replica path {replicaPath}");

        //get files in source
        //get files in replica

        // if file !exist in replica copy file + Log Add file
        // if file exist in replica compare MD5 => if different copy file + Log Modified file
        // if file exist in replica && !exist in source delete in replica  + Log File removed 
    }
}