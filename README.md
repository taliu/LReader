# LReader
在winodw上读取大文本文件的简单工具

# LReader使用

## 命令格式：
```
>head [OPTION]  FILE
功能：输出指定文件的前部分. 如果没有使用参数选项OPTION,默认输出指定文件前10行到控制台

>tail [OPTION]  FILE
功能：输出指定文件的后部分.  如果没有使用参数选项OPTION,默认输出指定文件最后后10行到控制台

>find [OPTION]  FILE
功能：输出与关键字 或 正则表达式 匹配的行
```

## 参数说明：

###FILE:
 指定的文件名称
 
###OPTION:
```
 -n, --lines
 一个整数，  输出指定文件中的n行
 
-o, --output 
  输出的文件名，将内容到output文件中，默认输出到控制台

-c, --encoding
  打开文件FILE时使用的编码。
```


#### 下面参数只对find命令有效：
```
 -r,--regex
  要匹配的正则表达式字符串
  
-k,--keyword
  要匹配的关键字，如果-r参数存在，则忽略这个-k参数
  
-i,--ignoreCase
  为开关参数，表示查找匹配时，是否要忽略大小写

-t,--tail
 为开关参数，表示查找时，从文件末尾向开始向上查找。如果不使用该参数，默认从文件头部开始向下查找。
```

##例子:
1) 输出文件filename.txt的前5行
```
head -n 5  filename.txt  
```

2)输出文件filename.txt的前5行到myOutput.txt中
```
head -n 5  filename.txt  -o myOutput.txt
```

3)用指定编码gb2312打开文件并输出文件filename.txt的最后5行  
```
tail -n 5  filename.txt -c gb2312
```

4)从文件头部开始向下查找包含关键字 stock 的所有行
```
   find -k stock   filename.txt
```
5)从文件末尾向开始向上查包含关键字 stock 的前5行
```
   find --tail -n 10 -k stock   filename.txt
```
6)从文件头部开始向下查找匹配正则表达式 \d+ 的前10行
```
   find -n 10 -r "\d+"   filename.txt
```
   
7)从文件头部开始向下查找忽略大小写匹配正则表达式 \d+ 的前10行
```
   find -i -n 10 -r "\d+"  filename.txt
```
