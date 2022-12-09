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
using System;
using System.Security;
using static BTNET.BVVM.Identity.Constants.Constants;

namespace BTNET.BVVM.Identity
{
    internal static class Uid
    {
        internal static volatile string UniqueID = null!;
        internal static volatile SecureString Identity = null!;
        internal static volatile byte[] SaltBytes = { 0x6b, 0x54, 0x4b, 0x4b, 0x76, 0x5a, 0x54, 0x4d, 0x63, 0x67, 0x3d, 0x3d, 0x32, 0x57, 0x44 };

        internal static Status IInternal(SecureString password = null!)
        {
            if (password == null)
            {
                return new Status(false, PASSWORD_NULL);
            }

            try
            {
                IdentitySuccess id = Machine.GetNewMachineID(password);
                if (id.Success)
                {
                    UniqueID = Crypt.Decrypt(id.Identity, Check.Saltyness, password).Message.GetSecure();
                    if (UniqueID != null)
                    {
                        Check.Saltyness = null!;

                        Status s = Check.StrongID(id.Identity, password);
                        if (!s.Success)
                        {
                            return new Status(false, s.Message);
                        }

                        Identity = (UniqueID + id.Identity.GetSecure()).ToSecure();
                        return new Status(true, IDENTITY_LOADED, Crypt.Encrypt("Ok", Identity, password).Message.ToSecure());
                    }
                }

                return new Status(false, FAILED_TO_CREATE_IDENTITY);
            }
            catch (Exception ex)
            {
                return new Status(false, FAILED_TO_LOAD_INDENTITY + ex.Message + STACK_TRACE + ex.StackTrace);
            }
        }
    }
}
