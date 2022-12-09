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

namespace BTNET.BVVM.Identity.Structs
{
    /// <summary>
    /// BlockCopy Byte Array
    /// </summary>
    internal struct BlockCopy
    {
        /// <summary>
        /// BlockCopy Byte Array
        /// </summary>
        /// <param name="success">the current status of the copy</param>
        /// <param name="byteArray">the bytes in the array</param>
        /// <param name="error">an error message if status is false</param>
        internal BlockCopy(bool success, byte[] byteArray = null!, string error = null!)
        {
            Success = success;
            Array = byteArray;
            Message = error;
        }

        /// <summary>
        /// Status of the copy
        /// </summary>
        internal bool Success { get; set; }

        /// <summary>
        /// Byte Array if success is true
        /// </summary>
        internal byte[] Array { get; set; }

        /// <summary>
        /// Error String if success is false
        /// </summary>
        internal string Message { get; set; }
    }
}
