/*****************************************************************************************************************************************************
 * Copyright WuGanhao. All Rights Reserved.
 *
 * This software is the confidential and proprietary information of
 * WuGanhao. ("Confidential Information"). You shall not
 * disclose such Confidential Information and shall use it only in
 * accordance with the terms of the license agreement you entered
 * into with WuGanhao.
*****************************************************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WuGanhao.CommandLineParser
{
    using Console = System.Console;

    /// <summary>
    /// Command Line Parser
    /// </summary>
    /// <typeparam name="TCommandExecutor"></typeparam>
    public class CommandLineParser<TCommandExecutor> where TCommandExecutor : class, new() {

        private readonly Type Type = typeof(TCommandExecutor);
        private readonly Dictionary<PropertyInfo, CommandAbstractAttribute> _attrs =
            typeof(TCommandExecutor).GetProperties().Where(p => p.GetCustomAttribute<CommandAbstractAttribute>(true) != null)
            .ToDictionary(p => p, p => p.GetCustomAttribute<CommandAbstractAttribute>(true));
        private readonly IEnumerable<SubCommandAttribute> _commands = typeof(TCommandExecutor).GetCustomAttributes(typeof(SubCommandAttribute), true)
            .Cast<SubCommandAttribute>();

        public CommandLineParser() {
        }

        private void ValidateArguments() {
            // Validate duplicated long name
            IGrouping<string, PropertyInfo> group = this._attrs
                .GroupBy(kvp => kvp.Value.LongName, kvp => kvp.Key).FirstOrDefault(g => g.Count() > 1);

            if (group != null) {
                throw new CommandLineException($"Switch --{group.Key} is assigned to conflicted properties: {string.Join(", ", group)}");
            }

            // Validate duplicated short name
            group = this._attrs
                .GroupBy(kvp => kvp.Value.ShortName, kvp => kvp.Key).FirstOrDefault(g => g.Count() > 1);

            if (group != null) {
                throw new CommandLineException($"Switch -{group.Key} is assigned to conflicted properties: {string.Join(", ", group)}");
            }
        }

        public void ShowHelp() {
            string fileName = Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string description = this.Type.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
            Console.WriteLine(fileName);
            Console.WriteLine(description);
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine($"    {fileName} <command> [options]");
            Console.WriteLine();
            Console.WriteLine("Commands:");

            foreach (SubCommandAttribute cmd in this._commands) {
                Console.WriteLine($"    {cmd.Command}:");
                Console.WriteLine($"        {cmd.Description}");
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("Options:");
            foreach (var kvp in _attrs) {
                var attr = kvp.Value;

                Console.WriteLine($"    --{attr.LongName} | -{attr.ShortName}:");
                Console.WriteLine($"        {attr.Description}");
                Console.WriteLine();
            }

            Console.WriteLine($"    --help | -h");
            Console.WriteLine($"        Print this help message and exit.");
            Console.WriteLine();
            Console.WriteLine($"Try {fileName} <command> --help for command specific help.");
        }

        public void ShowHelp(SubCommandAttribute cmdAttr) {
            string fileName = Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string description = this.Type.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
            Console.WriteLine(fileName);
            Console.WriteLine(description);
            Console.WriteLine();
            Console.WriteLine("Command:");

            Console.WriteLine($"    {cmdAttr.Command}:");
            Console.WriteLine($"        {cmdAttr.Description}");
            Console.WriteLine();

            Console.WriteLine("Usage:");
            string argumentLine = string.Join(
                " ",
                cmdAttr.Type.GetProperties()
                .OrderBy(p => p.GetCustomAttribute<RequiredAttribute>() == null)
                .Select( p => {
                    bool isReq = p.GetCustomAttribute<RequiredAttribute>() != null;
                    bool isMulti = p.PropertyType.GetInterfaces().Contains(typeof(System.Collections.IEnumerable)) && p.PropertyType != typeof(string);
                    CommandAbstractAttribute optAttr = p.GetCustomAttribute<CommandAbstractAttribute>();
                    if (optAttr == null) return null;

                    bool isOpt = optAttr is CommandOptionAttribute;

                    return (isReq ? "" : "[") + "--" + optAttr.LongName + "|" + "-" + optAttr.ShortName + (isOpt ? " " + optAttr.LongName : "") +
                           (isReq ? "" : "]") + (isMulti ? "+" : "");
                }).Where( p => p != null)
                );

            Console.WriteLine($"{fileName} {cmdAttr.Command} {argumentLine}");
            Console.WriteLine();

            var attrs = cmdAttr.Type.GetProperties().Where(p => p.GetCustomAttribute<CommandAbstractAttribute>(true) != null)
            .ToDictionary(p => p, p => p.GetCustomAttribute<CommandAbstractAttribute>(true));

            Console.WriteLine("Options:");
            foreach (var kvp in attrs) {
                var attr = kvp.Value;
                var p = kvp.Key;
                bool isOpt = attr is CommandOptionAttribute;


                Console.WriteLine($"    --{attr.LongName} | -{attr.ShortName}" + (isOpt ? " " + attr.LongName : "") + ":");
                Console.WriteLine($"        {attr.Description}");
                if (p.PropertyType.IsEnum) {
                    string options = string.Join(", ", Enum.GetValues(p.PropertyType).OfType<object>());
                    Console.WriteLine($"        Available options: {options}");
                }
                Console.WriteLine();
            }

            Console.WriteLine($"    --help | -h");
            Console.WriteLine($"        Print this help message and exit.");
            Console.WriteLine();
        }

        /// <summary>
        /// Parse options from current command line
        /// </summary>
        /// <returns></returns>
        public SubCommand GetSubCommand(TCommandExecutor parent, Type subCmdType) {
            IEnumerable<string> args = Environment.GetCommandLineArgs().Skip(2);
            SubCommand cmd = (SubCommand)System.Activator.CreateInstance(subCmdType);

            SubCommand<TCommandExecutor> strongTypedCmd = cmd as SubCommand<TCommandExecutor>;
            if (strongTypedCmd != null) {
                strongTypedCmd.Parent = parent;
            }

            //Validate command options class
            this.ValidateArguments();

            var attrs = subCmdType.GetProperties().Where(p => p.GetCustomAttribute<CommandAbstractAttribute>(true) != null)
            .ToDictionary(p => p, p => p.GetCustomAttribute<CommandAbstractAttribute>(true));
            IEnumerator<string> enumerator = args.GetEnumerator();

            while (enumerator.MoveNext()) {
                if (!attrs.Any(kvp => kvp.Value.TryConsumeArgs(cmd, kvp.Key, enumerator))) {
                    throw new ArgumentException($"Invalid argument '{enumerator.Current}'");
                }
            }

            return cmd;
        }

        public bool IsHelp() {
            IEnumerable<string> args = Environment.GetCommandLineArgs().Skip(1);
            return !args.Any() || args.Any(a => a == "-h" || a == "--help");
        }

        public bool IsHelp(SubCommandAttribute cmd) {
            IEnumerable<string> args = Environment.GetCommandLineArgs().Skip(2);

            return !args.Any() || args.Any(a => a == "-h" || a == "--help");
        }

        private SubCommandAttribute GetSubCommandAttribute(out TCommandExecutor commandExecutor) {
            TCommandExecutor executor = new TCommandExecutor();
            IEnumerable<string> args = Environment.GetCommandLineArgs().Skip(1);

            IEnumerator<string> enumerator = args.GetEnumerator();
            while (enumerator.MoveNext()) {
                string arg = enumerator.Current;
                SubCommandAttribute attr = _commands.FirstOrDefault(c => c.Command == enumerator.Current);
                if (attr != null) {
                    commandExecutor = executor;
                    return attr;
                }

                if (!_attrs.Any(kvp => kvp.Value.TryConsumeArgs(executor, kvp.Key, enumerator))) {
                    throw new ArgumentException($"Invalid argument '{enumerator.Current}'");
                }
            }

            commandExecutor = null;
            return null;
        }

        public async Task<int> Invoke() {
            SubCommandAttribute cmdAttr = this.GetSubCommandAttribute(out TCommandExecutor commandExecutor);
            if (cmdAttr == null && this.IsHelp()) {
                this.ShowHelp();
                return 0;
            }

            if (cmdAttr == null) {
                IEnumerable<string> args = Environment.GetCommandLineArgs().Skip(1);
                string strCmd = args.FirstOrDefault();
                throw new CommandLineException($"Invalid command: {strCmd}");
            }

            if (this.IsHelp(cmdAttr)) {
                this.ShowHelp(cmdAttr);
                return 0;
            }

            if (cmdAttr != null) {
                IEnumerable<string> args = Environment.GetCommandLineArgs().Skip(2);
                SubCommand cmd = this.GetSubCommand(commandExecutor, cmdAttr.Type);
                return await cmd.Run() ? 0 : 1;
            }

            return 0;
        }
    }

}
