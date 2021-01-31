using DokanNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;


namespace OposFileSystem
{
    /* Dokan file system example
     * 
     * Steps to run:
     * 1) Download: https://github.com/dokan-dev/dokany/releases/ (DokanSetup.exe)
     * 2) Open the Package Manager Console. (Tools > NuGet Package Manager > Package Manager Console)
     * 3) To install Dokan.NET bindings, execute the following command: Install-Package DokanNet
     * 
     * See:
     * Dokany repo:
     * https://github.com/dokan-dev/dokany
     * Dokan Wiki:
     * https://github.com/dokan-dev/dokany/wiki
     * DokanNet.IDokanOperations Interface Reference:
     * https://dokan-dev.github.io/dokan-dotnet-doc/html/interface_dokan_net_1_1_i_dokan_operations.html
     */
    class OposFileSystem : IDokanOperations
    {
        private readonly static int MAX_FILE_SIZE = 32 * 1024 * 1024;
        private readonly static int CAPACITY = 512 * 1024 * 1024;

        // Dictionary in which file system paths are mapped to corresponding File objects.
        private readonly Dictionary<string, File> files = new Dictionary<string, File>();

        // TODO: Support for directories.

        // Free space (in bytes).
        private int freeBytesAvailable = CAPACITY;

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, out uint maximumComponentLength, IDokanFileInfo info)
        {
            volumeLabel = "OPOS";
            features = FileSystemFeatures.None;
            fileSystemName = "OPOSFileSystem";
            maximumComponentLength = 255;
            return NtStatus.Success;
        }

        public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, IDokanFileInfo info)
        {
            freeBytesAvailable = this.freeBytesAvailable;
            totalNumberOfFreeBytes = this.freeBytesAvailable;
            totalNumberOfBytes = CAPACITY;
            return NtStatus.Success;
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
        {
            if (files.ContainsKey(fileName))
            {
                fileInfo = new FileInformation()
                {
                    FileName = Path.GetFileName(fileName),
                    Length = fileName.Length,
                    Attributes = FileAttributes.Normal,
                    CreationTime = DateTime.Now,
                    LastWriteTime = DateTime.Now
                };
            }
            else
            {
                fileInfo = default(FileInformation);
                return NtStatus.Error;
            }

            return NtStatus.Success;
        }

        public NtStatus CreateFile(string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, IDokanFileInfo info)
        {
            if (mode == FileMode.CreateNew)
            {
                if (attributes == FileAttributes.Directory || info.IsDirectory)
                {
                    // TODO: Implement
                }

                else if (!files.Keys.Contains(fileName))
                {
                    File file = new File();
                    files.Add(fileName, file);
                }
            }

            return NtStatus.Success;
        }

        // See: https://dokan-dev.github.io/dokan-dotnet-doc/html/interface_dokan_net_1_1_i_dokan_operations.html#aedae368efd764c21992e7b989ff2987b
        public void Cleanup(string entry, IDokanFileInfo info)
        {
            if (info.DeleteOnClose == true)
            {
                // TODO: Delete file.
                files.Remove(entry);
            }
        }

        // See: https://dokan-dev.github.io/dokan-dotnet-doc/html/interface_dokan_net_1_1_i_dokan_operations.html#aedae368efd764c21992e7b989ff2987b
        public NtStatus DeleteDirectory(string directory, IDokanFileInfo info)
        {
            if (!info.IsDirectory)
                return NtStatus.Error;
            // DeleteOnClose gets or sets a value indicating whether the file has to be deleted during the IDokanOperations.Cleanup event. 
            info.DeleteOnClose = true;
            return NtStatus.Success;
        }
        
        // See: https://dokan-dev.github.io/dokan-dotnet-doc/html/interface_dokan_net_1_1_i_dokan_operations.html#aedae368efd764c21992e7b989ff2987b
        public NtStatus DeleteFile(string file, IDokanFileInfo info)
        {
            if (info.IsDirectory)
                return NtStatus.Error;
            // DeleteOnClose gets or sets a value indicating whether the file has to be deleted during the IDokanOperations.Cleanup event. 
            info.DeleteOnClose = true;
            return NtStatus.Success;
        }

        public NtStatus MoveFile(string oldPath, string newPath, bool replace, IDokanFileInfo info)
        {
            if (replace)
                return NtStatus.NotImplemented;

            if (oldPath == newPath)
                return NtStatus.Success;

            // TODO: Moving a directory.

            // TODO: Moving a file.

            return NtStatus.Success;
        }

        public NtStatus FindFiles(string dirPathName, out IList<FileInformation> foundFiles, IDokanFileInfo info)
        {
            foundFiles = new List<FileInformation>();
            if (dirPathName == @"\")
                dirPathName = "";
            int pathCount = dirPathName.Split(@"\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length;
            foreach (var foundFile in this.files
                .Where(filePath => filePath.Key.StartsWith(dirPathName + @"\")
                && filePath.Key.Length > dirPathName.Length + 1
                && filePath.Key.Split(@"\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length == pathCount + 1))
            {
                FileInformation fileInfo = new FileInformation();
                fileInfo.FileName = Path.GetFileName(foundFile.Key);
                fileInfo.Length = fileInfo.FileName.Length;
                fileInfo.CreationTime = DateTime.Now;
                fileInfo.LastWriteTime = DateTime.Now;
                foundFiles.Add(fileInfo);
            }
            return NtStatus.Success;
        }

        public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offsetLong, IDokanFileInfo info)
        {
            bytesWritten = 0;

            if (buffer.Length + offsetLong > MAX_FILE_SIZE)
            {
                bytesWritten = 0;
                return NtStatus.FileTooLarge;
            }

            int offset = unchecked((int)offsetLong);

            File file = files[fileName];
            if (offset > file.Bytes.Length)
            {
                bytesWritten = 0;
                return NtStatus.ArrayBoundsExceeded;
            }

            if (info.WriteToEndOfFile)
            {
                // TODO: Appending.
            }
            else
            {
                int difference = file.Bytes.Length - offset;
                freeBytesAvailable += difference;
                file.Bytes = file.Bytes.Take(offset).Concat(buffer).ToArray();
                bytesWritten = buffer.Length;
            }

            // TODO: Update date modified.
            return NtStatus.Success;
        }

        // Read file contents into a buffer, starting from offset
        public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, IDokanFileInfo info)
        {
            int offsetInt = (int)offset;
            File existingFile = files[fileName];
            existingFile.Bytes.Skip(offsetInt).Take(buffer.Length).ToArray().CopyTo(buffer, 0);
            int diff = existingFile.Bytes.Length - offsetInt;
            bytesRead = buffer.Length > diff ? diff : buffer.Length;
            return NtStatus.Success;
        }

        public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, IDokanFileInfo info)
        {
            files = new FileInformation[0];
            return NtStatus.NotImplemented;
        }

        public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
        {
            streams = new FileInformation[0];
            return NtStatus.NotImplemented;
        }

        public NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info) => NtStatus.Success;


        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            security = null;
            return NtStatus.Success;
        }
        public void CloseFile(string fileName, IDokanFileInfo info) { }
        public NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info) => NtStatus.Error;
        public NtStatus Mounted(IDokanFileInfo info) => NtStatus.Success;
        public NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info) => NtStatus.Error;
        public NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info) => NtStatus.Error;
        public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info) => NtStatus.Success;
        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info) => NtStatus.Error;
        public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, IDokanFileInfo info) => NtStatus.Error;
        public NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info) => NtStatus.Error;
        public NtStatus Unmounted(IDokanFileInfo info) => NtStatus.Success;
    }
}
