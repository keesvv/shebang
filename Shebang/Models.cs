using System;
using System.Collections.Generic;
using System.Text;

namespace Shebang
{
    public class Models
    {
        public enum LogType
        {
            INFO = 0,
            WARN = 1,
            ERROR = 2
        }

        public enum ScriptType
        {
            BEFORE_INSTALL = 0,
            AFTER_INSTALL = 1
        }

        public class ColoredString
        {
            public string Text { get; set; }
            public ConsoleColor ForegroundColor { get; set; }
            public ConsoleColor? BackgroundColor { get; set; }
        }
    }
}
