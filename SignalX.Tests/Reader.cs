using System;
using System.Threading;

namespace SignalXLib.Tests
{
    public static class Reader
    {
        private static readonly AutoResetEvent GetInput;
        private static readonly AutoResetEvent GotInput;
        private static string _input;

        static Reader()
        {
            GetInput = new AutoResetEvent(false);
            GotInput = new AutoResetEvent(false);
            var inputThread = new Thread(ReaderH) {IsBackground = true};
            inputThread.Start();
        }

        private static void ReaderH()
        {
            while (true)
            {
                GetInput.WaitOne();
                _input = Console.ReadLine();
                GotInput.Set();
            }
        }

        public static string ReadLine(TimeSpan timeOut)
        {
            GetInput.Set();
            var success = GotInput.WaitOne(timeOut);
            if (success)
                return _input;
            throw new TimeoutException();
        }
    }
}