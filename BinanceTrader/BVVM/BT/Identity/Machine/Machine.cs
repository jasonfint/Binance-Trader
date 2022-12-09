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

using BTNET.BVVM.Identity.Structs;
using Microsoft.Win32;
using System;
using System.Security;
using static BTNET.BVVM.Identity.Constants.Constants;

namespace BTNET.BVVM.Identity
{
    internal class Machine
    {
        protected static volatile string MotherboardUUID = null!;
        protected static volatile byte[] WindowsAKIHash = null!;

        internal static IdentitySuccess GetNewMachineID(SecureString password = null!)
        {
            if (password == null)
            {
                return new IdentitySuccess(false, PASSWORD_NULL);
            }

            try
            {
                string weakOne = Guid.NewGuid().ToString();
                string weakTwo = Guid.NewGuid().ToString();
                string finalUniqueWeak = (F + weakOne + S + F + weakTwo);

                string builder = "";
                MotherboardUUID = (string)Registry.GetValue(MOTHERBOARD_UUID, CONFIG, "");
                WindowsAKIHash = (byte[])Registry.GetValue(TPM_KEY, HASH, "");
                if (WindowsAKIHash != null && WindowsAKIHash.Length > 0)
                {
                    foreach (byte k in WindowsAKIHash)
                    {
                        builder += k.ToString();
                    }
                }
                string finalUniqueIDStrong = builder + S + MotherboardUUID;

                string finalUniqueID = (builder != "" ? builder : F + weakOne) + S + (MotherboardUUID != "" ? MotherboardUUID : F + weakTwo);

                if (finalUniqueIDStrong != finalUniqueID)
                {
                    return new IdentitySuccess(false, FAILED_TO_CREATE_IDENTITY);
                }

                Check.Saltyness = finalUniqueID.Substring(50).ToSecure();
                WrapSS enc = Crypt.Encrypt(finalUniqueIDStrong.ToSecure(), Check.Saltyness, password);
                if (enc.Success)
                {
                    return new IdentitySuccess(true, enc.Message);
                }
                else
                {
                    return new IdentitySuccess(false, enc.Error);
                }

            }
            catch (Exception ex)
            {
                return new IdentitySuccess(false, ex.Message + STACK_TRACE + ex.StackTrace);
            }
        }
    }
}
