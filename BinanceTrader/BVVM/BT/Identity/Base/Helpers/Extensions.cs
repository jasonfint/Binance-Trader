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

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace BTNET.BVVM.Identity
{
    /// <summary>
    /// Secure String Extensions
    /// ???? Where did it go I forgor
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Get the string the secure string is representing
        /// </summary>
        /// <param name="source">The source secure string</param>
        /// <returns></returns>
        public static string GetSecure(this SecureString source)
        {
            lock (source)
            {
                string result;
                var length = source.Length;
                var pointer = IntPtr.Zero;
                var chars = new char[length];

                try
                {
                    pointer = Marshal.SecureStringToBSTR(source);
                    Marshal.Copy(pointer, chars, 0, length);

                    result = string.Join("", chars);
                }
                finally
                {
                    if (pointer != IntPtr.Zero)
                    {
                        Marshal.ZeroFreeBSTR(pointer);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Create a secure string from a string
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static SecureString ToSecure(this string source)
        {
            var secureString = new SecureString();

            foreach (var c in source)
            {
                secureString.AppendChar(c);
            }

            secureString.MakeReadOnly();
            return secureString;
        }
    }
}
