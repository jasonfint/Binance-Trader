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
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using static BTNET.BVVM.Identity.Constants.Constants;

namespace BTNET.BVVM.Identity
{
    internal partial class Crypt
    {
        internal static Wrap Encrypt(string c, SecureString k, SecureString p)
        {
            try
            {
                using (Aes encryptor = Aes.Create())
                {
                    BlockCopy bc = Combine.ByteArray(Uid.SaltBytes, Encoding.UTF8.GetBytes(k.GetSecure()));
                    if (!bc.Success)
                    {
                        return new Wrap(false, null!, FAILED_TO_COPY_BYTES + bc.Message);
                    }

                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(p.GetSecure(), bc.Array);

                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    encryptor.Padding = PaddingMode.Zeros;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            byte[] clearBytes = Encoding.Unicode.GetBytes(c);
                            cs.Write(clearBytes, 0, clearBytes.Length);
                            cs.Close();
                        }

                        return new Wrap(true, Convert.ToBase64String(ms.ToArray()));
                    };
                }
            }
            catch (Exception ex)
            {
                return new Wrap(false, null!, ENCOUNTERED_AN_EXCEPTION + ex.Message);
            }
        }

        internal static WrapSS Encrypt(SecureString c, SecureString k, SecureString p)
        {
            try
            {
                using (Aes encryptor = Aes.Create())
                {
                    BlockCopy bc = Combine.ByteArray(Uid.SaltBytes, Encoding.UTF8.GetBytes(k.GetSecure()));
                    if (!bc.Success)
                    {
                        return new WrapSS(false, null!, FAILED_TO_COPY_BYTES + bc.Message);
                    }

                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(p.GetSecure(), bc.Array);

                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    encryptor.Padding = PaddingMode.Zeros;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            byte[] clearBytes = Encoding.Unicode.GetBytes(c.GetSecure());
                            cs.Write(clearBytes, 0, clearBytes.Length);
                            cs.Close();
                        }

                        return new WrapSS(true, Convert.ToBase64String(ms.ToArray()).ToSecure());
                    };
                }

            }
            catch (Exception ex)
            {
                return new WrapSS(false, null!, ENCOUNTERED_AN_EXCEPTION + ex.Message);
            }
        }
    }
}
