using System;
using static Shebang.Models;

namespace Shebang
{
    public class Logger
    {
        public static void Log(string text, LogType level = LogType.INFO)
        {
            switch (level)
            {
                case LogType.INFO:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("=> ");
                    Console.ResetColor();
                    break;
                case LogType.WARN:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("=> ");
                    break;
                case LogType.ERROR:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.Write(" ! ");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(" ");
                    break;
                default:
                    break;
            }
            
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
