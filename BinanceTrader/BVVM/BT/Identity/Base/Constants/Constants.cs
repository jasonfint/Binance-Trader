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

namespace BTNET.BVVM.Identity.Constants
{
    internal class Constants
    {
        internal const string FAILED_TO_COPY_BYTES = "Failed to Copy Bytes: ";
        internal const string ENCOUNTERED_AN_EXCEPTION = "Encountered an Exception: ";
        internal const string FAILED_TO_CREATE_IDENTITY = "Failed to create an Identity for this Machine";
        internal const string FAILED_TO_LOAD_INDENTITY = "Identity failed to Load and Encountered an Exception: ";
        internal const string IDENTITY_LOADED = "Identity Loaded";
        internal const string STACK_TRACE = " | Stack Trace: ";
        internal const string PASSWORD_NULL = "????";
        internal const string HARDWARE_MISMATCH = "Identity doesn't match the hardware or is otherwise incorrect";
        internal const string PROVIDE_IDENTITY = "Must provide Identity First (Not Initialized)";
        internal const string SALT_TOO_SMALL = "Salt is too small, Please make salt longer if you are going to change it";

        internal const string MOTHERBOARD_UUID = @"HKEY_LOCAL_MACHINE\SYSTEM\HardwareConfig";
        internal const string CONFIG = "LastConfig";
        internal const string TPM_KEY = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\TPM\WMI";
        internal const string HASH = "WindowsAIKHash";
        internal const string S = "<=>";
        internal const string F = "F";
    }
}