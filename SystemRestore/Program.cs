



string root = @"D:\<YourBackupFilePath>\Data\C\Users\<UserName>\OptionalSubDir";
string equiv = @"C:\<YourTargetDirectory";

CopyFile(root, equiv);
Console.WriteLine("Done");

/// <summary>
/// Copies the files from the folder to the target folder
/// </summary>
void CopyFile(string folder, string targetFolder)
{
    foreach (string nextFolder in Directory.GetDirectories(folder))
    {
        string nextPiece = nextFolder[folder.Length..];
        string newFolder = targetFolder + nextPiece;
        CopyFile(nextFolder, newFolder);
    }
    string[] allFiles = Directory.GetFiles(folder);
    Dictionary<string, FileInfo> coveredFiles = MostRecentFiles(allFiles);
    if (!Directory.Exists(targetFolder))
    {
        Directory.CreateDirectory(targetFolder);
    }
    foreach (KeyValuePair<string, FileInfo> keyVal in coveredFiles)
    {
        string newFileName = targetFolder + '\\' + keyVal.Key;
        if (File.Exists(newFileName))
        {
            continue;//don't overwrite existing files
        }
        string currentFile = keyVal.Value.FullPath;
        try
        {
            File.Copy(currentFile, newFileName, false);
        }
        catch (Exception ex)
        {
            Console.WriteLine(string.Format("Could not copy {0} to {1}: {2}", currentFile, newFileName, ex));
        }
    }


}
/// <summary>
/// Gets the location of the most recent files keys by the file name (with no file path
/// </summary>
Dictionary<string, FileInfo> MostRecentFiles(string[] allFiles)
{
    Dictionary<string, FileInfo> coveredFiles = new();
    foreach (string file in allFiles)
    {
        if (file.Contains("UTC)"))
        {
            string fileName = GetFixedPath(file, out DateTime dateTime);
            if (dateTime == default)
            {
                Console.WriteLine(string.Format("Could not detect date: {0}", file));
            }
            else if (!coveredFiles.ContainsKey(fileName))
            {
                coveredFiles.Add(fileName, new FileInfo()
                {
                    FullPath = file,
                    Date = dateTime,
                });
            }
            //Get the most recent file
            else if (coveredFiles[fileName].Date < dateTime)
            {
                coveredFiles[fileName].Date = dateTime;
                coveredFiles[fileName].FullPath = file;
            }
        }
        else
        {
            Console.WriteLine(string.Format("Skipping {0}", file));
        }
    }

    return coveredFiles;
}
/// <summary>
/// Removes the UTC extension
/// </summary>
string GetFixedPath(string path, out DateTime dateTime)
{
    dateTime = default;
    if (!path.Contains("UTC)"))
    {
        return path;
    }
    List<char> sb = new(path.Length);
    bool skipBegin = false;
    bool skipEnd = false;
    string year = "";
    string month = "";
    string day = "";
    string hour = "";
    string minute = "";
    string second = "";
    for (int i = path.Length - 1; i >= 0; i--)
    {
        char currentChar = path[i];
        if (currentChar == '\\')
        {
            break;
        }
        if (skipEnd)
        {
            sb.Add(currentChar);
        }
        else if (skipBegin && currentChar == '(')
        {
            skipEnd = true;
            i--;
        }
        else if (currentChar == ')')
        {
            skipBegin = true;
        }
        else if (!skipBegin)
        {
            sb.Add(currentChar);
        }
        else if (skipBegin)
        {
            if (!char.IsDigit(currentChar))
            {

            }
            else if (second.Length != 2)
            {
                second = currentChar + second;
            }
            else if (minute.Length != 2)
            {
                minute = currentChar + minute;
            }
            else if (hour.Length != 2)
            {
                hour = currentChar + hour;
            }
            else if (day.Length != 2)
            {
                day = currentChar + day;
            }
            else if (month.Length != 2)
            {
                month = currentChar + month;
            }
            else if (year.Length != 4)
            {
                year = currentChar + year;
                if (year.Length == 4)
                {
                    try
                    {
                        dateTime = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day), int.Parse(hour), int.Parse(minute), int.Parse(second));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(string.Format("Failed to interpret {0}: {1}", path, e));
                    }
                }
            }
        }
    }
    return new string(sb.AsEnumerable().Reverse().ToArray());
}
/// <summary>
/// Helper class
/// </summary>
class FileInfo
{
    /// <summary>
    /// The UTC date of the file's backup time
    /// </summary>
    public DateTime Date { get; set; }
    /// <summary>
    /// The full file path with the file name
    /// </summary>
    public string FullPath { get; set; } = "";
}

