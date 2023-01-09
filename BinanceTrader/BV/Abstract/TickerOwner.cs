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

using BTNET.BV.Enum;
using System.Collections.Generic;

namespace BTNET.BV.Abstract
{
    public class TickerOwner
    {
        private volatile List<Owner> _owners = new List<Owner>();
        private readonly object _lock = new object();

        public bool Add(Owner currentOwner, bool allowMultiple)
        {
            lock (_lock)
            {
                if (!allowMultiple && _owners.Contains(currentOwner))
                {
                    return false;
                }

                _owners.Add(currentOwner);
                return true;
            }
        }

        public bool Remove(Owner currentOwner)
        {
            lock (_lock)
            {
                return _owners.Remove(currentOwner);
            }
        }

        public IEnumerable<Owner> GetOwners()
        {
            return _owners;
        }

        public bool Exists()
        {
            lock (_lock)
            {
                if (_owners.Count > 0)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
