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
        /* public static void Main()
         {
             new OposFileSystem().Mount("Y:\\", DokanOptions.DebugMode | DokanOptions.StderrOutput);
         }*/


        static void Main(string[] args)
        {
            MyBTree tree = new MyBTree();
            Console.WriteLine("Hello World!");



            tree.insertion(new MyFile("aa"));
            tree.insertion(new MyFile("bb"));
            tree.insertion(new MyFile("asda"));
            tree.insertion(new MyFile("dasdasdasdaa"));
            Console.WriteLine("\nispis:");
            tree.traversal(tree.root);
            tree.insertion(new MyFile("aasdaa"));
            tree.insertion(new MyFile("aasdasda"));
            Console.WriteLine("\nispis:");
            tree.traversal(tree.root);
            int t = 3;
            MyFile res = null;
            tree.searching(2, ref t, tree.root, ref res);
            Console.WriteLine("\nnadjena je " + res.getID() + " " + res.path);
            Console.ReadLine();
        }

    }
}
