using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode
{
    internal class Lib
    {
        public class Timer : IDisposable
        {
            Stopwatch _sw = Stopwatch.StartNew();
            Action<long> _handler;

            public Timer(Action<long> handler)
            {
                _handler = handler;
            }

            public void Dispose()
            {
                _sw.Stop();
                _handler(_sw.ElapsedMilliseconds);
            }
        }
    }
}
