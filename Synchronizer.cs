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

        //get files in source
        //get files in replica
        sourceFilesList = GetFilesInDirectory(sourcePath);
        replicaFilesList = GetFilesInDirectory(replicaPath);
        // if file exist in replica && !exist in source delete in replica  + Log File removed 
        RemoveAdditionalFiles();
        RemoveAdditionalDirectories();
        // if file exist in replica compare MD5 => if different copy file + Log Replace file
        ReplaceExistingFiles();
        // if file !exist in replica copy file + Log Add file
        AddAdditionalDirectories();
        AddNewFiles();
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
            Console.WriteLine("The process failed: {0}", e);
            iLogger.Log("Error Occured");
        }
    }

    Dictionary<string, FileInfo> GetFilesInDirectory(string path)
    {
        string[] fullPath = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        Dictionary<string, FileInfo> fileList = new Dictionary<string, FileInfo>();
        foreach (var file in fullPath)
        {
            var relativePath = Path.GetRelativePath(path, file);
            fileList.Add(relativePath, new FileInfo(file));
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
#if DEBUG
                FileSystem.DeleteFile(Path.Combine(replicaPath, replicaFile), UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
#else
                File.Delete(Path.Combine(replicaPath, replicaFile));
#endif
            }
        }
    }

    void RemoveAdditionalDirectories()
    {
        var replicaDirs = Directory.GetDirectories(replicaPath, "*", SearchOption.AllDirectories).OrderByDescending(d => d.Length);

        foreach (var dir in replicaDirs)
        {
            var relativePath = Path.GetRelativePath(replicaPath, dir);
            var sourceDirPath = Path.Combine(sourcePath, relativePath);

            if (!Directory.Exists(sourceDirPath) && !Directory.EnumerateFileSystemEntries(dir).Any())
            {
                iLogger.Log(FileAction.Removed, $"{dir} directory was removed from Replica ");
                Directory.Delete(dir);
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
                    CopyFile(file);
                    iLogger.Log(FileAction.Replaced, $"{file} in replica was replaced from Source at {sourceFilesList[file].FullName} because it's size was different");
                }
                else
                {
                    var replicaMd5 = ComputeMd5(replicaFilesList[file]);
                    var sourceMd5 = ComputeMd5(sourceFilesList[file]);
                    if (!replicaMd5.SequenceEqual(sourceMd5))
                    {
                        CopyFile(file);
                        iLogger.Log(FileAction.Replaced, $"{file} in replica was replaced from Source at {sourceFilesList[file].FullName} because it's content was different");
                    }
                }
            }
        }
    }

    void AddNewFiles()
    {
        foreach (var file in sourceFilesList.Keys)
        {
            if (!replicaFilesList.ContainsKey(file))
            {
                CopyFile(file);
                replicaFilesList.Add(file, new FileInfo(file));
                iLogger.Log(FileAction.Added, $"{file} was Added from Source");
            }
        }
    }
    
    void AddAdditionalDirectories()
    {
        var sourceDirs = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories).OrderBy(d => d.Length);

        foreach (var dir in sourceDirs)
        {
            var relativePath = Path.GetRelativePath(sourcePath, dir);
            var replicaDirPath = Path.Combine(replicaPath, relativePath);

            if (!Directory.Exists(replicaDirPath))
            {
                iLogger.Log(FileAction.Added, $"{dir} directory was Added from Source");
                Directory.CreateDirectory(replicaDirPath);
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
        var destinationFilePath = Path.Combine(replicaPath, file);

        var destDir = Path.GetDirectoryName(destinationFilePath);
        if (destDir != null)
            Directory.CreateDirectory(destDir);

        File.Copy(fileToCopy, destinationFilePath, true);
    }
}