using System.Security.Cryptography;
using Microsoft.VisualBasic.FileIO;
using SearchOption = System.IO.SearchOption;

namespace QA_VS;

public class Synchronizer
{
    string sourcePath;
    string replicaPath;
    Dictionary<string, FileInfo> sourceFilesList;
    Dictionary<string, FileInfo> replicaFilesList;

    readonly ILogger iLogger;

    public Synchronizer(ILogger iLogger)
    {
        this.iLogger = iLogger;
    }

    public void Synchronize(string sourcePath, string replicaPath)
    {
        this.sourcePath = sourcePath;
        this.replicaPath = replicaPath;

        iLogger.Log($"Try Sync from path {sourcePath} to {replicaPath}");
        CheckDirectories();

        sourceFilesList = GetFilesInDirectory(sourcePath);
        replicaFilesList = GetFilesInDirectory(replicaPath);
        //get files in source
        //get files in replica


        // if file exist in replica && !exist in source delete in replica  + Log File removed 
        RemoveAdditionalFiles();
        // if file exist in replica compare MD5 => if different copy file + Log Replace file
        ReplaceExistingFiles();
        // if file !exist in replica copy file + Log Add file
    }

    void CheckDirectories()
    {
        try
        {
            Directory.CreateDirectory(sourcePath);
            Directory.CreateDirectory(replicaPath);
        }
        catch (Exception e)
        {
            Console.WriteLine("The process failed: {0}", e.ToString());
            iLogger.Log("Error Occured");
        }
    }

    Dictionary<string, FileInfo> GetFilesInDirectory(string path)
    {
        string[] fullPath = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        // string[] relativePathArray = new string[fullPath.Length];
        Dictionary<string, FileInfo> fileList = new Dictionary<string, FileInfo>();
        for (var index = 0; index < fullPath.Length; index++)
        {
            var relativePath = Path.GetRelativePath(path, fullPath[index]);
            //    relativePathArray[index] = relativePath;
            fileList.Add(relativePath, new FileInfo(fullPath[index]));
        }

        Console.WriteLine($"Found {fileList.Count} files in {path}");
        return fileList;
    }

    void RemoveAdditionalFiles()
    {
        foreach (var replicaFile in replicaFilesList.Keys)
        {
            if (!sourceFilesList.ContainsKey(replicaFile))
            {
                iLogger.Log(FileAction.Removed, $"{replicaFile} was removed from Replica at {replicaFilesList[replicaFile].FullName}");
                replicaFilesList.Remove(replicaFile);
                //File.Delete(Path.Combine(replicaPath,replicaFile));
                // FileSystem.DeleteFile(Path.Combine(replicaPath,replicaFile), UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
        }
    }

    void ReplaceExistingFiles()
    {
        foreach (var file in replicaFilesList.Keys)
        {
            if (sourceFilesList.ContainsKey(file))
            {
                if (replicaFilesList[file].Length != sourceFilesList[file].Length)
                {
                    Console.WriteLine($"{file} form replica is {replicaFilesList[file].Length} but in source is {sourceFilesList[file].Length}");
                    CopyFile(file);
                }
                else
                {
                    {
                        var replicaMd5 = ComputeMd5(replicaFilesList[file]);
                        var sourceMd5 = ComputeMd5(sourceFilesList[file]);
                        if (!replicaMd5.SequenceEqual(sourceMd5))
                        {
                            Console.WriteLine($"{file} MD5 is different from source MD5");
                            CopyFile(file);
                        }
                    }
                }
            }
        }
    }

    byte[] ComputeMd5(FileInfo file)
    {
        using var md5 = MD5.Create();
        using var stream = file.OpenRead();
        return md5.ComputeHash(stream);
    }

    void CopyFile(string file)
    {
        var fileToCopy = sourceFilesList[file].FullName;
        File.Copy(fileToCopy, Path.Combine(replicaPath, file), true);
        iLogger.Log(FileAction.Replaced, $"{file} in replica was replaced from Source at {sourceFilesList[file].FullName}");
    }
}