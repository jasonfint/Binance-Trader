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
    internal class Check
    {
        internal static volatile SecureString Saltyness = null!;

        /// <summary>
        /// Test the Identity_Strong key
        /// <para>If this matches the Identity key then you don't have a weak key</para>
        /// </summary>
        /// <param name="strongIdentityKey"></param>
        /// <param name="password">The additional password your data is secured with</param>
        /// <returns></returns>
        internal static Status StrongID(SecureString strongIdentityKey, SecureString password = null!)
        {
            if (password == null)
            {
                return new Status(false, PASSWORD_NULL);
            }

            try
            {
                string MotherboardUUID = Registry.GetValue(MOTHERBOARD_UUID, CONFIG, "") as string ?? "";

                string builder = "";
                if (Registry.GetValue(TPM_KEY, HASH, "") is byte[] WindowsAKIHash && WindowsAKIHash.Length > 0)
                {
                    foreach (byte k in WindowsAKIHash)
                    {
                        builder += k.ToString();
                    }
                }

                string strong = (builder + S + MotherboardUUID);
                string strong_e = Crypt.Encrypt(strong, (builder + S + MotherboardUUID).Substring(50).ToSecure(), password).Message;

                if (strong_e == strongIdentityKey.GetSecure() && Uid.UniqueID != null)
                {
                    if (Crypt.Decrypt(strong_e, (builder + S + MotherboardUUID).Substring(50).ToSecure(), password).Message == Uid.UniqueID)
                    {
                        return new Status(true, IDENTITY_LOADED);
                    }
                }

                return new Status(false, HARDWARE_MISMATCH);
            }
            catch (Exception ex)
            {
                return new Status(false, ex.Message + STACK_TRACE + ex.StackTrace);
            }
        }
    }
}
