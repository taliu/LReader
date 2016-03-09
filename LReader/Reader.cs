using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LReader
{
    public class Reader
    {
        private string _filePath;
        private Encoding _encode;
        private string _outputFile;
        public Reader(string filePath, string outputFile, string encoding)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }
            _filePath = GetFullPath(filePath);
            if (string.IsNullOrEmpty(encoding))
            {
                _encode = Encoding.Default;
            }
            else
            {
                _encode = Encoding.GetEncoding(encoding);
            }
            _outputFile = outputFile;
        }

        public void PrintHead(uint num)
        {
            if (num == 0)
            {
                num = 10;
            }
            using (FileStream fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs, _encode))
                {
                    for (int i = 0; i < num; i++)
                    {
                        var line = sr.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        Output(line);
                    }
                }
            }
        }

        /// <summary>
        /// 打印匹配 关键字 或 正则表达式 的 行，如果regex存在，则忽略参数keyword
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="regex">正则表达式</param>
        /// <param name="readHead">为true则输出头num行，否则输出尾num行</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <param name="num">输出行数，如果为0则输出所有匹配项</param>
        public void PrintMatch(string keyword, string regex, bool readHead, bool ignoreCase, uint num)
        {
            if (readHead)
            {
                PrintHeadMatch(keyword, regex, ignoreCase, num);
            }
            else
            {
                PrintTailMatch(keyword, regex, ignoreCase, num);
            }
        }

        private void PrintHeadMatch(string keyword, string regex, bool ignoreCase, uint num)
        {
            if (num==0)
            {
                num = int.MaxValue;
            }
            uint count = 0;
            using (FileStream fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs, _encode))
                {
                    if (!string.IsNullOrEmpty(regex))
                    {
                        var reg = ignoreCase ? new Regex(regex, RegexOptions.IgnoreCase) : new Regex(regex);
                        while (count < num)
                        {
                            var line = sr.ReadLine();
                            if (line == null)
                            {
                                break;
                            }
                            if (reg.IsMatch(line))
                            {
                                count++;
                                Output(line);
                            }
                        }
                    }
                    else//keyword
                    {
                        while (count < num)
                        {
                            var line = sr.ReadLine();
                            if (line == null)
                            {
                                break;
                            }
                            var str = ignoreCase ? line.ToLower() : line;
                            if (str.Contains(keyword))
                            {
                                count++;
                                Output(line);
                            }
                        }
                    }
                }
            }
        }

        //这个方法要优化
        private void PrintTailMatch(string keyword, string regex, bool ignoreCase, uint num)
        {
            if (num == 0)
            {
                num = int.MaxValue;
            }
            uint count = 0;
          
            var maxByte = 1 * 1024 * 1024;//1Mb
            using (FileStream fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var len = fs.Length;
                var n = 1;
                long size = 0;
                List<string> lines = null;
                do
                {
                    size = maxByte * n;
                    if (size > len)
                    {
                        size = len;
                    }
                    fs.Seek(len - size, SeekOrigin.Begin);
                    lines = GetAllLine(fs);
                    lines = lines.FindAll(line => {
                        if (!string.IsNullOrEmpty(regex))
                        {
                            var reg = ignoreCase ? new Regex(regex, RegexOptions.IgnoreCase) : new Regex(regex);
                            if (reg.IsMatch(line))
                            {
                                return true; 
                            }
                        }
                        else//keyword
                        {
                            var str = ignoreCase ? line.ToLower() : line;
                            if (str.Contains(keyword))
                            {
                                return true;
                            }
                        }
                        return false;
                    });
                    n++;
                } while (lines.Count < num && size != len);
                PrintList(lines, num);
            }
        }
        //这个方法要优化
        public void PrintTail(uint num)
        {
            if (num == 0)
            {
                num = 10;
            }
            var maxByte = 1 * 1024 * 1024;//1Mb
            using (FileStream fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var len = fs.Length;

                var n = 1;
                long size = 0;
                List<string> lines = null;
                if (!fs.CanSeek)
                {
                    if (len > maxByte * 200)
                    {
                        Output("文件太大！");
                        return;
                    }
                    lines = GetAllLine(fs);
                    PrintList(lines, num);
                    return;
                }
                do
                {
                    size = maxByte * n;
                    if (size > len)
                    {
                        size = len;
                    }
                    fs.Seek(len - size, SeekOrigin.Begin);
                    lines = GetAllLine(fs);
                    n++;
                } while (lines.Count < num && size != len);
                PrintList(lines, num);
            }
        }

        private void PrintList(List<string> list, uint num)
        {
            var lines = (int)num;
            if (list.Count <= lines)
            {
                foreach (var item in list)
                {
                    Output(item);
                }
            }
            else
            {
                for (int i = list.Count - lines; i < list.Count; i++)
                {
                    Output(list[i]);
                }
            }
        }

        private List<string> GetAllLine(FileStream fs)
        {
            StreamReader sr = new StreamReader(fs, _encode);
            List<string> list = new List<string>();
            var line = sr.ReadLine();
            while (line != null)
            {
                list.Add(line);
                line = sr.ReadLine();
            }
            return list;
        }

        private string GetFullPath(string path)
        {
            return path;
        }

        private void Output(string line)
        {
            if (string.IsNullOrEmpty(_outputFile))
            {
                Console.WriteLine(line);
            }
            else
            {
                File.AppendAllText(_outputFile, line + "\r\n");
            }
        }
    }
}
