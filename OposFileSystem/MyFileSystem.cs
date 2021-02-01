using DokanNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace OposFileSystem
{
    class MyFileSystem : IDokanOperations
    {
        long freeBytesAvailable = 536_870_912;
        long totalNumberOfBytes = 536_870_912;
        long totalNumberOfFreeBytes = 536_870_912;
        MyBTree bTree = new MyBTree();
        private Dictionary<string, int> filesDictionary = new Dictionary<string, int>();

        public MyFileSystem(){
            MyFile newFile = new MyFile("\\");
            filesDictionary.Add("\\", newFile.getID());
            bTree.insertion(newFile);
        }
        
      

        public NtStatus CreateFile(string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, IDokanFileInfo info)
        {
            if (fileName == "\\")
                return NtStatus.Success;
            if (access == DokanNet.FileAccess.ReadAttributes && mode == FileMode.Open)
                return NtStatus.Success;
            if (mode == FileMode.CreateNew)
            {
                //string temp = MyTree.GetFileName(fileName);

                if (fileName.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Length > 12) return NtStatus.Error;
                //MyTree node = MyTree.GetFileDir(tree, fileName);
                //if (temp.Length > 25) return NtStatus.ObjectNameInvalid;
               /* if (node.Nodes.Count == 16)
                {
                    return NtStatus.FileTooLarge;
                }*/
                if (attributes == FileAttributes.Directory || info.IsDirectory)
                {

                    //MyTree.AddNodeInTree(tree, fileName, null);
                    string filePath = bTree.getPathFromFileName(fileName);
                    MyFile newFile = new MyFile(filePath);
                    filesDictionary.Add(filePath, newFile.getID());
                    bTree.insertion(newFile);

                }
                else
                {
                    if (Path.GetExtension(fileName).Length != 4) return NtStatus.ObjectNameInvalid;
                    byte[] arr = new byte[0];
                    string filePath = bTree.getPathFromFileName(fileName);
                    MyFile newFile = new MyFile(filePath);
                    newFile.setData(arr);
                    filesDictionary.Add(filePath, newFile.getID());
                    bTree.insertion(newFile);

                }
            }
            return NtStatus.Success;
        }

        public void Cleanup(string fileName, IDokanFileInfo info)
        {
            if (info.DeleteOnClose == true)
            {
                // TODO: Delete file.
                filesDictionary.Remove(fileName);

                int fileId = filesDictionary[fileName];
                bTree.deletion(fileId, bTree.root);
            }
        }

        public void CloseFile(string fileName, IDokanFileInfo info)
        {
           
        }

        public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, IDokanFileInfo info)
        {
            int fileId = filesDictionary[fileName];
            MyFile resFile = null;
            int tmp = 3;
            bTree.searching(fileId, ref tmp, bTree.root, ref resFile);

            if (resFile == null || resFile.getData() == null)
            {
                bytesRead = 0;
                return NtStatus.Success;
            }

            byte[] file = resFile.getData();
            int i = 0;
            for (i = 0; i < file.Length && i < buffer.Length; ++i)
                buffer[i] = file[i];
            bytesRead = i;
            return NtStatus.Success;
        }

        public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, IDokanFileInfo info)
        {
            byte[] file = new byte[buffer.Length];
            int i = 0;
            for (i = 0; i < buffer.Length; ++i)
                file[i] = buffer[i];

            int fileId = filesDictionary[fileName];
            MyFile resFile = null;
            int tmp = 3;
            bTree.searching(fileId, ref tmp, bTree.root, ref resFile);

            resFile.setData(file);
            bytesWritten = i;
            if ((resFile.getData().Length + buffer.Length) > 32 * 1024) return NtStatus.FileTooLarge;

            freeBytesAvailable -= bytesWritten;
            totalNumberOfFreeBytes -= bytesWritten;
            return NtStatus.Success;

        }

        public NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info)
        {
            return NtStatus.Error;
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
        {
            if (!filesDictionary.ContainsKey(fileName)) {
                fileInfo = default(FileInformation);
                return NtStatus.Error;
            }

                int fileId = filesDictionary[fileName];
                MyFile resFile = null;
                int tmp = 3;
                bTree.searching(fileId, ref tmp, bTree.root, ref resFile);
                if (resFile == null)
                {
                    fileInfo = default(FileInformation);

                    /*if(resFile!=null)
                        resFile.setFileInfo(fileInfo);*///ja mislim da ovo ne treba jer nema tog faila;
                    return NtStatus.Error;
                }
                if (resFile.getData() == null)//to znaci da je direktorijum
                {
                    fileInfo = new FileInformation()
                    {
                        FileName = Path.GetFileName(fileName),
                        Attributes = FileAttributes.Directory,
                        CreationTime = null,
                        LastWriteTime = null
                    };
                    resFile.setFileInfo(fileInfo);
                }
                else
                {

                    fileInfo = new FileInformation()
                    {
                        FileName = Path.GetFileName(fileName),
                        Length = resFile.getData().Length,
                        Attributes = FileAttributes.Normal,
                        CreationTime = DateTime.Now,
                        LastWriteTime = DateTime.Now
                    };
                    resFile.setFileInfo(fileInfo);
                }
                return NtStatus.Success;
            

        }

        public NtStatus FindFiles(string fileName, out IList<FileInformation> files, IDokanFileInfo info)
        {

            //files = MyTree.GetFilesOnLocation(tree, fileName);
            int fileId = filesDictionary[fileName];
            TreeNode resNode = null;
            int tmp = 3;
            bTree.searchingForNode(fileId, ref tmp, bTree.root, ref resNode);
            IList<FileInformation> filesInfoTmp = new List<FileInformation>();

            //if (resFile == null) NtStatus.Error;
            if (resNode.files == null) {
                files = filesInfoTmp;
                return NtStatus.Success;
            }
            foreach (MyFile file in resNode.files)
            {
                if(file != null)//jer se moze desiti da prvog nema a drugog ima
                {
                    if (file.getData() == null)
                    {
                        FileInformation fileInfo = new FileInformation();
                        fileInfo.Attributes = FileAttributes.Directory;
                        fileInfo.FileName = Path.GetFileName(file.path);
                        filesInfoTmp.Add(fileInfo);
                    }
                    else
                    {
                        FileInformation fileInfo = new FileInformation();
                        fileInfo.FileName = Path.GetFileName(file.path);
                        fileInfo.Length = file.getData().Length;
                        filesInfoTmp.Add(fileInfo);
                    }
                }
                
            }
            files = filesInfoTmp;
            return NtStatus.Success;
        }

        public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, IDokanFileInfo info)
        {
            files = new FileInformation[0];
            return DokanResult.NotImplemented;
        }

        public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
        {
            return NtStatus.Success;
        }

        public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, IDokanFileInfo info)
        {
            int fileId = filesDictionary[fileName];
            MyFile resFile = null;
            int tmp = 3;
            bTree.searching(fileId, ref tmp, bTree.root, ref resFile);

            //informations[fileName] = new FileInformation()
            FileInformation newInfo= new FileInformation()
            {
                FileName = resFile.getFileInfo().FileName,
                Attributes = resFile.getFileInfo().Attributes,
                Length = resFile.getFileInfo().Length,
                CreationTime = creationTime ?? resFile.getFileInfo().CreationTime,
                LastAccessTime = lastAccessTime ?? resFile.getFileInfo().LastAccessTime,
                LastWriteTime = lastWriteTime ?? resFile.getFileInfo().LastWriteTime
            };
            resFile.setFileInfo(newInfo);
            return NtStatus.Success;
        }

        public NtStatus DeleteFile(string fileName, IDokanFileInfo info)
        {
            if (!info.IsDirectory)
                return DokanResult.Error;

            info.DeleteOnClose = true;
            return NtStatus.Success;
        }

        public NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
        {
            if (!info.IsDirectory)
                return DokanResult.Error;

            info.DeleteOnClose = true;
            return NtStatus.Success;
        }

        public NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
        {
            return NtStatus.Success;//nije gotovoooo
        }

        public NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
        {
            return NtStatus.Error;
        }

        public NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
        {
            return NtStatus.Error;
        }

        public NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            return NtStatus.Error;
        }

        public NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            return NtStatus.Error;
        }

        public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, IDokanFileInfo info)
        {
            freeBytesAvailable = this.freeBytesAvailable;

            totalNumberOfBytes = this.totalNumberOfBytes;
            totalNumberOfFreeBytes = this.totalNumberOfFreeBytes;
            return NtStatus.Success;
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, out uint maximumComponentLength, IDokanFileInfo info)
        {
            volumeLabel = "MyFileSystem";
            features = FileSystemFeatures.None;
            fileSystemName = string.Empty;
            maximumComponentLength = 255;//dodao
            return NtStatus.Success;
        }

        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            security = null;
            return NtStatus.Error;
        }

        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            return NtStatus.Error;
        }

        public NtStatus Mounted(IDokanFileInfo info)
        {
            return NtStatus.Success;
        }

        public NtStatus Unmounted(IDokanFileInfo info)
        {
            return NtStatus.Success;
        }

        public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
        {
            streams = new FileInformation[0];
            return DokanResult.NotImplemented;
        }
    }
}
