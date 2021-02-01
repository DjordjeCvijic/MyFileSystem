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
        private  Dictionary<string, int> filesDictionary = new Dictionary<string, int>();

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
                    MyFile newFile = new MyFile(fileName);
                    filesDictionary.Add(fileName, newFile.getID());
                    bTree.insertion(newFile);

                }
                else
                {
                    if (Path.GetExtension(fileName).Length != 4) return NtStatus.ObjectNameInvalid;
                    byte[] arr = new byte[0];
                    MyFile newFile = new MyFile(fileName);
                    newFile.setData(arr);
                    filesDictionary.Add(fileName, newFile.getID());
                    bTree.insertion(newFile);

                }
            }
            return NtStatus.Success;
        }

        public void Cleanup(string fileName, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public void CloseFile(string fileName, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
        {
            int fileId = filesDictionary[fileName];
            MyFile resFile = null;
            int tmp = 3;
            bTree.searching(2, ref tmp, bTree.root, ref resFile);
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
            bTree.searchingForNode(2, ref tmp, bTree.root, ref resNode);
            IList<FileInformation> filesInfoTmp = new List<FileInformation>();

            //if (resFile == null) NtStatus.Error;
            foreach (MyFile file in resNode.files)
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
            files = filesInfoTmp;
            return NtStatus.Success;
        }

        public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, IDokanFileInfo info)
        {
            int fileId = filesDictionary[fileName];
            MyFile resFile = null;
            int tmp = 3;
            bTree.searching(2, ref tmp, bTree.root, ref resFile);

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
            throw new NotImplementedException();
        }

        public NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, out uint maximumComponentLength, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus Mounted(IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus Unmounted(IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }
    }
}
