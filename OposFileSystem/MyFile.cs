using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DokanNet;
namespace OposFileSystem
{
    class MyFile
    {
        public static int i = 1;
        public string path;
        private int id;
        private byte[] data;
        private FileInformation fileInfo;
        public MyFile(String p)
        {
            path = p;
            id = i++;
        }
        public int getID()
        {
            return id;
        }
        public void setData(byte[] d)
        {
            data = d;
        }
        public byte[] getData()
        {
            return data;
        }
        public FileInformation getFileInfo()
        {
            return fileInfo;
        }
        public void setFileInfo(FileInformation fi)
        {
            fileInfo = fi;
        }

    }
}
