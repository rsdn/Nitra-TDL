using System;

namespace Tdl2Json
{
    internal struct ConsoleForegroundColor : IDisposable
    {
        private readonly ConsoleColor originalColor;

        public ConsoleForegroundColor(ConsoleColor newColor)
        {
            originalColor = Console.ForegroundColor;
            Console.ForegroundColor = newColor;
        }

        public void Dispose()
        {
            Console.ForegroundColor = originalColor;
        }
    }
}
