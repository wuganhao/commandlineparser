using System;

namespace WuGanhao.CommandLineParser
{
    public abstract class SubCommand
    {
        public SubCommand() {
        }

        public abstract bool Run ();
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]

    public class SubCommandAttribute: Attribute {
        public SubCommandAttribute(Type cmdType, string command, string description) {
            this.Type = cmdType;
            this.Command = command;
            this.Description = description;
        }

        public Type Type { get; set; }

        public string Command { get; set; }

        public string Description { get; set; }
    }
}
