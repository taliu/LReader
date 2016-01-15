using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace LReader
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Run(args);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }
        private static void Run(string[] args)
        {
            string errorMsg = "";
            var cmdArg = GetCmdArg(args, ref errorMsg);
            if (cmdArg == null)
            {
                PrintHelp();
                Console.WriteLine("请输入命令，输入 help 显示帮助，输入 exit 退出。\r\n");
                while (true)
                {
                    try
                    {
                        Console.Write("\r\n>");
                        string argLine = Console.ReadLine();
                        argLine = argLine.Trim().ToLower();
                        if (argLine == "exit")
                        {
                            return;
                        }
                        if (argLine == "help")
                        {
                            PrintHelp();
                            continue;
                        }
                        args = CommandArgs.GetArgs(argLine);
                        cmdArg = GetCmdArg(args, ref errorMsg);
                        if (cmdArg == null)
                        {
                            Console.WriteLine(errorMsg);
                            continue;
                        }
                        ExecCmd(cmdArg);
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err.Message);
                    }
                }
                return;
            }
            ExecCmd(cmdArg);
        }
        private static void ExecCmd(CmdArg cmdArg)
        {
            Reader red = new Reader(cmdArg.FileName, cmdArg.OutputFile, cmdArg.Encoding);
            if (cmdArg.CmdName == "head")
            {
                red.PrintHead(cmdArg.Lines);
            }
            else if (cmdArg.CmdName == "tail")
            {
                red.PrintTail(cmdArg.Lines);
            }
            else
            {
                Console.WriteLine("命令有误！");
            }
        }

        private static CmdArg GetCmdArg(string[] args, ref string errorMsg)
        {
            var cmdArgs = CommandArgs.Parse(args);
            if (cmdArgs.Params == null || cmdArgs.Params.Count <= 1)
            {
                errorMsg = "命令有误!";
                return null;
            }

            string cmd = "";//head or tail
            string path = "";
            uint lines = 10;
            string outputFile = "";
            string encoding = "";
            string temp = "";

            cmd = cmdArgs.Params[0].Trim().ToLower();
            if (cmd != "tail" && cmd != "head")
            {
                errorMsg = "错误：不存在命令 " + cmd + " ,命令的类型只能是 head 或 tail";
                return null;
            }
            path = cmdArgs.Params[1];
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                errorMsg = "错误:不存在文件 " + path;
                return null;
            }
            if (cmdArgs.ArgPairs.TryGetValue("n", out temp) || cmdArgs.ArgPairs.TryGetValue("lines", out temp))
            {
                if (!uint.TryParse(temp, out lines))
                {
                    errorMsg = "错误:参数 -n 或 --lines 要为整数";
                    return null;
                }
            }
            if (cmdArgs.ArgPairs.TryGetValue("o", out temp) || cmdArgs.ArgPairs.TryGetValue("output", out temp))
            {
                outputFile = temp;
            }
            if (cmdArgs.ArgPairs.TryGetValue("c", out temp) || cmdArgs.ArgPairs.TryGetValue("encoding", out temp))
            {
                encoding = temp;
            }
            return new CmdArg
            {
                CmdName = cmd,
                Encoding = encoding,
                FileName = path,
                Lines = lines,
                OutputFile = outputFile
            };
        }

        private static void PrintHelp()
        {
            string welcome = @"LReader帮助
命令格式：
> head [OPTION]  FILE
功能：输出指定文件的前部分. 如果没有使用参数选项OPTION,默认输出指定文件前10行到控制台

> tail [OPTION]  FILE
功能：输出指定文件的后部分.  如果没有使用参数选项OPTION,默认输出指定文件最后后10行到控制台

参数说明：
FILE:
指定的文件名称
OPTION:
-n, --lines
 一个整数，  输出指定文件中的n行
-o, --output 
  输出的文件名，将内容到output文件中，默认输出到控制台
-c, --encoding
  打开文件FILE时使用的编码。
例子:
1)输出文件filename.txt的前5行  
   head -n 5  filename.txt  
2)输出文件filename.txt的前5行到myOutput.txt中
  head -n 5  filename.txt  -o myOutput.txt
3)用指定编码gb2312打开文件并输出文件filename.txt的最后5行  
  tail -n 5  filename.txt -c gb2312

作者：taliu@outlook.com
------------------------------
";
            Console.WriteLine("\r\n" + welcome);

        }


        private class CmdArg
        {
            public uint Lines { get; set; }
            public string Encoding { get; set; }
            public string OutputFile { get; set; }
            public string FileName { get; set; }
            public string CmdName { get; set; }
        }
    }
}
