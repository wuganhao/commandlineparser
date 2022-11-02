# WuGanhao.CommandLineParser

This package provides a command line parser with following features:

* Supports sub commands, argument switch / options
* Strong typed arguments

## Translation

* [中文](https://github.com/wuganhao/commandlineparser/blob/master/README-cn.md)

## Installation

You can refereces this package from `nuget.org` with id `WuGanhao.CommandLineParser`

## Sample

By using following sample code

```csharp
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using WuGanhao.CommandLineParser;
using WuGanhao.CSharpRewrite.SubCommands;

namespace WuGanhao.CSharpRewrite {
    /// <remarks>RemoveTrailSpaces is a sub command implemented WuGanhao.CommandLineParser.SubCommand. You will need to implement your business inside RemoveTrailSpaces.Run()</remarks>
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

You can expect your command line runs like following

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

## This library is currently used in

* [GitHub.CommandLine](https://github.com/wuganhao/GitHub.CommandLine) - A simple utility for managing with GitHub
* [cs-rewrite](https://github.com/wuganhao/cs-rewrite) - A tool for help rewrite C# code

## Bugs and feature request

For bugs and feature requests. Please log it in [GitHub Issues](https://github.com/wuganhao/commandlineparser/issues).
Contributing a PR will be better.
