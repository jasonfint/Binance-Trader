/*
*MIT License
*
*Copyright (c) 2022 S Christison
*
*Permission is hereby granted, free of charge, to any person obtaining a copy
*of this software and associated documentation files (the "Software"), to deal
*in the Software without restriction, including without limitation the rights
*to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
*copies of the Software, and to permit persons to whom the Software is
*furnished to do so, subject to the following conditions:
*
*The above copyright notice and this permission notice shall be included in all
*copies or substantial portions of the Software.
*
*THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
*IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
*FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
*AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
*LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
*OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
*SOFTWARE.
*/

using BTNET.BVVM.Log;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace BTNET.BVVM
{
    /// <summary>
    /// Periodically trigger an Action at the Interval
    /// If the action is still running when the Interval Occurs the Action will be skipped
    /// </summary>
    public class PeriodicAction : IDisposable
    {
        readonly Action _Action;
        readonly long _Interval;
        readonly CancellationToken _Ct;
        readonly SemaphoreSlim _Slim;

        private const string INTERVAL = "Increase Interval of: ";

        /// <summary>
        /// Periodically trigger an Action at the Interval
        /// If the action is still running when the Interval Occurs the Action will be skipped
        /// </summary>
        /// <param name="action">The Action</param>
        /// <param name="intervalMs">The Interval to trigger the Action</param>
        /// <param name="ct">Cancellation Token</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public PeriodicAction(Action action, long intervalMs, CancellationToken ct = default)
        {
            if (intervalMs == 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            _Interval = intervalMs;
            _Slim = new SemaphoreSlim(1, 1);
            _Action = action;
            _Ct = ct;
        }

        public void Run()
        {
            if (_Slim.CurrentCount > 0)
            {
                Task.Factory.StartNew(() =>
                {
                    Stopwatch sw = Stopwatch.StartNew();

                    while (!_Ct.IsCancellationRequested)
                    {
                        if (sw.ElapsedMilliseconds >= _Interval)
                        {
                            sw.Restart();
                            var r = _Slim.Wait(0);
                            if (r)
                            {
                                try
                                {
                                    _Action();
                                }
                                catch
                                {
                                    //
                                }
                                finally
                                {
                                    _Slim.Release();
                                }
                            }
                            else
                            {
                                WriteLog.Info(INTERVAL + _Action.Method.Name);
                            }
                        }

                        Thread.Sleep(1);
                    }
                }, TaskCreationOptions.DenyChildAttach).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            _Slim.Dispose();
        }
    }
}
