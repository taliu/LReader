using System;
using System.Collections.Generic;
using System.Text;
namespace LReader
{
    public class CommandArgs
    {
        public CommandArgs()
        {
            ArgPairs = new Dictionary<string, string>();
            Params = new List<string>();
        } 
        public  Dictionary<string, string> ArgPairs { get; set; } 
        public  List<string> Params { get; set; }
        public static CommandArgs Parse(string[] args)
        {
            var commandArgs = new CommandArgs();
            for (int i = 0; i < args.Length; i++)
            {
                var item = args[i];
                if (item == "-" || item == "--")
                {
                    continue;
                }
                if (item.StartsWith("-"))
                {
                    var key = item.Substring(1);//移除 - 或 --
                    if (key[0] == '-')
                    {
                        key = key.Substring(1);
                    }
                    if (i + 1 < args.Length)//后面是否跟一个值
                    {
                        var nextItem = args[i + 1];
                        if (nextItem.StartsWith("-"))
                        {
                            commandArgs.ArgPairs.Add(key, "true");//不是跟一个值，则使用true代替
                        }
                        else
                        {
                            nextItem = RecoveryValue(nextItem);
                            commandArgs.ArgPairs.Add(key, nextItem);
                            i++;
                        }
                    }
                    else
                    {
                        commandArgs.ArgPairs.Add(key, "true");
                    }
                }
                else
                {
                    item = RecoveryValue(item);
                    commandArgs.Params.Add(item);
                }
            }
            return commandArgs;
        }

        private static string RecoveryValue(string val)
        {
            val = val.Replace(_space, " ");
            if (val[0]=='"'&&val[val.Length-1]=='"')
            {
                return val.Substring(1, val.Length - 2);
            }
            return val;
        }

        private static string _space = "$!space!$";
        public static string[] GetArgs(string cmd)
        {
            if (string.IsNullOrEmpty (cmd))
            {
                return new string[] { };
            }
            var strExp = "\".*?\"";
            var cmdStr = new System.Text.RegularExpressions.Regex(strExp).Replace(cmd, m =>
            {
                var r = m.Value.Replace(" ", _space);
                return r;
            });
          return  cmdStr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}