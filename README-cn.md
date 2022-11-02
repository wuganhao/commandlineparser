# WuGanhao.CommandLineParser

本库提供支持以下功能的命令解释器：

* 支持子命令，开关以及命令参数
* 支持强类型参数

## 其它语言版本

* [English](https://github.com/wuganhao/commandlineparser/blob/master/README.md)

## 安装/使用

可以直接从[NuGet](nuget.org)中安装 `WuGanhao.CommandLineParser`.

## 样例

使用以下代码

```csharp
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using WuGanhao.CommandLineParser;
using WuGanhao.CSharpRewrite.SubCommands;

namespace WuGanhao.CSharpRewrite {
    /// <remarks>RemoveTrailSpaces 是一个实现 WuGanhao.CommandLineParser.SubCommand 的子命令。你需要在RemoveTrailSpaces.Run()中实现你的子命令业务</remarks>
    [SubCommand(typeof(RemoveTrailingSpaces), "remove-trailing-spaces", "Remove trailing spaces")]
    [Description("A tool for help rewrite C# code")]
    public class CommandExecutor {
        [CommandOption("dir", "d", "Directory for start code processing")]
        public string Directory { get; set; }

        [CommandOption("pattern", "p", "Search file pattern")]
        public string Pattern { get; set; }

        [CommandOption("exclude-directories", "e", "Directories excluded for processing, Seperated by comma")]
        public string ExcludeDirectories { get; set; } = "packages;obj;bin";

        [CommandSwitch("interactive", "i", "Allow command interaction")]
        public bool Interactive { get; set; }
    }

    public class Program {
        static async Task<int> Main(string[] args) {
            CommandLineParser<CommandExecutor> CmdLineParser = new CommandLineParser<CommandExecutor>();

            try {
                await CmdLineParser.Invoke();
                return 0;
            } catch (Exception ex) {
                System.Console.WriteLine(ex);
                return -1;
            }
        }
    }
}
```

你会得到下命令行

```bash
C:>cs-rewrite -h
cs-rewrite
A tool for help rewrite C# code

Usage:
    cs-rewrite <command> [options]

Commands:
    remove-trailing-spaces:
        Remove trailing spaces


Options:
    --dir | -d:
        Directory for start code processing

    --pattern | -p:
        Search file pattern

    --exclude-directories | -e:
        Directories excluded for processing, Seperated by comma

    --interactive | -i:
        Allow command interaction

    --help | -h
        Print this help message and exit.

Try cs-rewrite <command> --help for command specific help.
```

## 以下工具使用本库

* [GitHub.CommandLine](https://github.com/wuganhao/GitHub.CommandLine) - 一个用来简单管理GitHub功能的命令行
* [cs-rewrite](https://github.com/wuganhao/cs-rewrite) - 一个用来自动格式和重写C#代码的工具

## 缺陷及功能需求

如果你在使用中遇到BUG，或者有其它功能需求。你可以在[GitHub Issues](https://github.com/wuganhao/commandlineparser/issues)提交。
同样，也欢迎贡献Pull Request。
