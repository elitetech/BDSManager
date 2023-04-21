using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDSManager.WebUI.IO;

public class DirectoryIO
{
    internal void Copy(string source, string destination, bool recursive)
    {
        DirectoryInfo dir = new DirectoryInfo(source);
        DirectoryInfo[] dirs = dir.GetDirectories();

        // If the source directory does not exist, throw an exception.
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + dir.FullName);
        }

        // If the destination directory does not exist, create it.
        if (!Directory.Exists(destination))
        {
            Directory.CreateDirectory(destination);
        }

        // Get the file contents of the directory to copy.
        FileInfo[] files = dir.GetFiles();

        foreach (FileInfo file in files)
        {
            // Create the path to the new copy of the file.
            string temppath = Path.Combine(destination, file.Name);

            // Copy the file.
            file.CopyTo(temppath, false);
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (recursive)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                // Create the subdirectory.
                string temppath = Path.Combine(destination, subdir.Name);

                // Copy the subdirectories.
                Copy(subdir.FullName, temppath, recursive);
            }
        }
    }
}
