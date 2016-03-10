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
                Console.WriteLine("请输入命令，输入 help 显示帮助，输入 clear 清屏， 输入 exit 退出。\r\n");
                while (true)
                {
                    try
                    {
                        Console.Write("\r\n>");
                        string argLine = Console.ReadLine();
                        argLine = argLine.Trim();
                        if (argLine == "exit")
                        {
                            return;
                        }
                        if (argLine == "help")
                        {
                            PrintHelp();
                            continue;
                        }
                        if (argLine == "clear")
                        {
                            Console.Clear();
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
            else if (cmdArg.CmdName=="find")
            {
                red.PrintMatch(cmdArg.Keyword, cmdArg.Regex, cmdArg.ReadHead, cmdArg.IgnoreCase, cmdArg.Lines);
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


            //find args
            string keyword=""; 
            string regex=""; 
            bool readHead=true; 
            bool ignoreCase=false;

            cmd = cmdArgs.Params[0].Trim().ToLower();
          
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


            if (cmdArgs.ArgPairs.TryGetValue("k", out temp) || cmdArgs.ArgPairs.TryGetValue("keyword", out temp))
            {
                keyword = temp;
            }
            if (cmdArgs.ArgPairs.TryGetValue("r", out temp) || cmdArgs.ArgPairs.TryGetValue("regex", out temp))
            {
                regex = temp;
            }

            if (cmdArgs.ArgPairs.TryGetValue("t", out temp) || cmdArgs.ArgPairs.TryGetValue("tail", out temp))
            {
                readHead = string.IsNullOrEmpty(temp);
            }

            if (cmdArgs.ArgPairs.TryGetValue("i", out temp) || cmdArgs.ArgPairs.TryGetValue("ignoreCase", out temp))
            {
                ignoreCase = !string.IsNullOrEmpty(temp);
            }

            return new CmdArg
            {
                CmdName = cmd,
                Encoding = encoding,
                FileName = path,
                Lines = lines,
                OutputFile = outputFile,

                 IgnoreCase=ignoreCase,
                  Keyword=keyword,
                   ReadHead=readHead,
                    Regex=regex
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

> find [OPTION]  FILE
功能：输出与关键字 或 正则表达式 匹配的行

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

下面参数只对find命令有效：
-r,--regex
  要匹配的正则表达式字符串
-k,--keyword
  要匹配的关键字，如果-r参数存在，则忽略这个-k参数
-i,--ignoreCase
  为开关参数，表示查找匹配时，是否要忽略大小写
-t,--tail
 为开关参数，表示查找时，从文件末尾向开始向上查找。如果不使用该参数，默认从文件头部开始向下查找。

例子:
1)输出文件filename.txt的前5行  
   head -n 5  filename.txt  
2)输出文件filename.txt的前5行到myOutput.txt中
  head -n 5  filename.txt  -o myOutput.txt
3)用指定编码gb2312打开文件并输出文件filename.txt的最后5行  
  tail -n 5  filename.txt -c gb2312

4)从文件头部开始向下查找包含关键字 stock 的所有行
   find -k stock   filename.txt
5)从文件末尾向开始向上查包含关键字 stock 的前5行
   find --tail -n 10 -k stock   filename.txt
6)从文件头部开始向下查找匹配正则表达式 \d+ 的前10行
   find -n 10 -r ""\d+""   filename.txt
7)从文件头部开始向下查找忽略大小写匹配正则表达式 \d+ 的前10行
   find -i -n 10 -r ""\d+""   filename.txt

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

            //find 命令
            public string Keyword { get; set; }
            public string Regex { get; set; }
            public bool ReadHead { get; set; }
            public bool IgnoreCase { get; set; }
        }
    }
}
