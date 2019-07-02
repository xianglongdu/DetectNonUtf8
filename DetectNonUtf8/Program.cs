using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace DetectNonUtf8
{
    class Program
    {
        public static string SearchFilter = null;

        static void Main(string[] args)
        {
            string DirPath = null;

            if (args.Length == 0)
            {
                ShowUsage();
                return;
            }
            if (args.Length > 0)
            {
                DirPath = args[0];
            }
            if (args.Length > 1)
            {
                SearchFilter = args[1];
            }
            //string DirPath = @"F:\FW\Temp\RimuTrunk";
            if (DirPath != null)
            {
                if (File.Exists(DirPath))
                {
                    ProcessFile(DirPath);
                }
                else
                {
                    ProcessDirectory(DirPath);
                }
            }
            else
            {
                Console.WriteLine("Dir path shouldn't be null");
                ShowUsage();
            }
        }

        static void ShowUsage()
        {
            Console.WriteLine("DetectNonUtf8:");
            Console.WriteLine("     This application is used to find out all the invalid UTF-8 charactors under the specific directory or file");
            Console.WriteLine("\nUsage:");
            Console.WriteLine("     DetectNonUtf8.exe <Directory/File Path> [Search Pattern]");
            Console.WriteLine("\nExample:");
            Console.WriteLine("     DetectNonUtf8.exe c:/fw/ProjectA");
            Console.WriteLine("     DetectNonUtf8.exe c:/fw/ProjectA *.c");
            Console.WriteLine("     DetectNonUtf8.exe c:/fw/ProjectA *.h");
        }

        static void ProcessDirectory(string dir)
        {
            // Process the list of files found in the directory.
            string[] fileEntries;
            if (SearchFilter == null)
            {
                fileEntries = Directory.GetFiles(dir);
            }
            else
            {
                fileEntries = Directory.GetFiles(dir, SearchFilter);
            }
            foreach (string fileName in fileEntries)
            {
                ProcessFile(fileName);
            }

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(dir);
            foreach (string subdirectory in subdirectoryEntries)
            {
                ProcessDirectory(subdirectory);
            }
        }
        static void ProcessFile(string path)
        {
            Thread SearchTask = new Thread(() => SearchFile(path));
            SearchTask.Start();
        }

        static void SearchFile(string path)
        {
            int last_line_num = -1;
            if (File.Exists(path))
            {
                byte[] b = File.ReadAllBytes(path);
                for (int i = 0; i < b.Length; i++)
                {
                    if (b[i] > 0x7F)
                    {
                        // it is not a valid utf-8 charactor
                        last_line_num = PrintLineOfCurrentChar(path, b, i, last_line_num);
                    }
                }
            }
        }

        static int PrintLineOfCurrentChar (string path, byte[] b, int i, int last_line_num)
        {
            int line_start = 0, line_end = b.Length, line_num = 0;
            if (i > 0 && i < b.Length)
            {
                for (int m = i; m > 0; m--)
                {
                    if (b[m] == 0xd || b[m] == 0xa)
                    {
                        line_start = m;
                        break;
                    }
                }
                for (int m = i; m < b.Length; m++)
                {
                    if (b[m] == 0xd || b[m] == 0xa)
                    {
                        line_end = m;
                        break;
                    }
                }
            } else if (i == 0)
            {
                line_start = 0;
                for (int m = i; m < b.Length; m++)
                {
                    if (b[m] == 0xd || b[m] == 0xa)
                    {
                        line_end = m;
                        break;
                    }
                }
            }
            else if (i == b.Length)
            {
                for (int m = i; m > 0; m--)
                {
                    if (b[m] == 0xd || b[m] == 0xa)
                    {
                        line_start = m;
                        break;
                    }
                }
            }
            for (int m = 0; m < i; m++)
            {
                if (b[m] == 0xd)
                {
                    line_num++;
                }
            }
            if (line_num == last_line_num)
            {
                return line_num;
            }
            Console.Write("{0}, Line {1} Colume {2}: ", path, line_num, i - line_start);
            for (int m = line_start; m < line_end; m++)
            {
                Console.Write(Convert.ToChar(b[m]));
            }
            Console.WriteLine();
            return line_num;
        }
    }
}
