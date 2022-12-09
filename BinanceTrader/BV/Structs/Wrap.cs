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

using System.Security;

namespace BTNET.BVVM.Identity.Structs
{
    /// <summary>
    /// Message Wrapper
    /// </summary>
    internal struct Wrap
    {
        /// <summary>
        /// Wrap a request
        /// </summary>
        /// <param name="success">true if request was successful</param>
        /// <param name="message">the requested message if success is true</param>
        /// <param name="error">an error message if success is false</param>
        internal Wrap(bool success, string message = null!, string error = null!)
        {
            Success = success;
            Error = error;
            Message = message;
        }

        /// <summary>
        /// Status
        /// </summary>
        internal bool Success { get; set; }

        /// <summary>
        /// The requested message if success is true
        /// </summary>
        internal string Message { get; set; }

        /// <summary>
        /// An Error message if success is false
        /// </summary>
        internal string Error { get; set; }
    }

    internal struct WrapSS
    {
        /// <summary>
        /// Wrap a request
        /// </summary>
        /// <param name="success">true if request was successful</param>
        /// <param name="message">the requested message if success is true</param>
        /// <param name="error">an error message if success is false</param>
        internal WrapSS(bool success, SecureString message = null!, string error = null!)
        {
            Success = success;
            Error = error;
            Message = message;
        }

        /// <summary>
        /// Status
        /// </summary>
        internal bool Success { get; set; }

        /// <summary>
        /// The requested message if success is true
        /// </summary>
        internal SecureString Message { get; set; }

        /// <summary>
        /// An Error message if success is false
        /// </summary>
        internal string Error { get; set; }
    }
}
