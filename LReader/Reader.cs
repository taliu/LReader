using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
            using (StreamReader sr = new StreamReader(_filePath, _encode))
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
