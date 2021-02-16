using DokanNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OposFileSystem
{
    class Program
    {
      
         public static void Main()
         {
                new MyFileSystem().Mount("Y:\\", DokanOptions.DebugMode | DokanOptions.StderrOutput);
         }


       

    }
}
