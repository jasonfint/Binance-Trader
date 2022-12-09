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
    /// The current Status of the ID
    /// </summary>
    public struct Status
    {
        /// <summary>
        /// The current Status of the ID
        /// </summary>
        /// <param name="success">the current status of the ID</param>
        /// <param name="message">Message that describes the current Success status</param>
        /// <param name="v"></param>
        public Status(bool success, string message = null!, SecureString v = null!)
        {
            Success = success;
            Message = message;
            ValidationString = v;
        }

        /// <summary>
        /// ID Status
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message that describes the current Success status
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// A predetermined encrypted string you can decrypt to test if your keys are still working
        /// <para>Should equal <see href="Ok"/> when decrypted</para>
        /// </summary>
        public SecureString ValidationString { get; set; }
    }
}
