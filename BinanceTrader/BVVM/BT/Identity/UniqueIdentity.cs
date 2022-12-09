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
    /// <summary>
    /// Provides a Persistent Unique Identifier for this PC
    /// <para>Includes Methods to Encrypt or Decrypt strings based on that Identity an Identifier and an Additional Password</para>
    /// </summary>
    public class UniqueIdentity
    {
        /// <summary>
        /// The specific version of Identity.NET you are using
        /// <para>Major version updates will create new keys that won't work with previous versions</para>
        /// </summary>
        public const double Version = 5.00;

        /// <summary>
        /// The last error
        /// </summary>
        public static string ErrorOutput = null!;

        /// <summary>
        /// Global Unique Identifier for this PC
        /// </summary>
        public static string UUID => Uid.UniqueID ?? throw new Exception(PROVIDE_IDENTITY);

        /// <summary>
        /// Initializes an Identity based on the Unique Identity of this PC
        /// <para>Data Encrypted with an Identifier can only be Decrypted by that Identifier</para>
        /// </summary>
        /// <param name="passwordIdentifier">The password/identifier to make the Identity more unique/secure</param>
        /// <returns><see cref="Status"/> Indicating if the ID is Initialized or an Error Message</returns>
        /// <exception cref="ArgumentNullException">throws when password is null, empty or whitespace</exception>
        /// <exception cref="ArgumentOutOfRangeException">throws when your password is too short, min 12 characters</exception>
        public static Status Initialize(string passwordIdentifier = "Identity.NET")
        {
            if (string.IsNullOrWhiteSpace(passwordIdentifier))
            {
                throw new ArgumentNullException(nameof(passwordIdentifier));
            }

            if (passwordIdentifier.Length < 12)
            {
                throw new ArgumentOutOfRangeException(nameof(passwordIdentifier));
            }

            return Uid.IInternal(passwordIdentifier.ToSecure());
        }

        /// <summary>
        /// Initializes an Identity based on the Unique Identity of this PC
        /// <para>Data Encrypted with an Identifier can only be Decrypted by that Identifier</para>
        /// </summary>
        /// <param name="securePasswordIdentifier">The additional password your data is secured with provided as a secure string</param>
        /// <returns><see cref="Status"/> Indicating if the ID is Initialized or an Error Message</returns>
        /// <exception cref="ArgumentNullException">throws when password is null, empty or whitespace</exception>
        /// <exception cref="ArgumentOutOfRangeException">throws when your password is too short, min 12 characters</exception>
        public static Status Initialize(SecureString securePasswordIdentifier)
        {
            if (securePasswordIdentifier == null)
            {
                throw new ArgumentNullException(nameof(securePasswordIdentifier));
            }

            if (securePasswordIdentifier.Length < 12)
            {
                throw new ArgumentOutOfRangeException(nameof(securePasswordIdentifier));
            }

            return Uid.IInternal(securePasswordIdentifier);
        }

        /// <summary>
        /// Encrypt a string using the Initialized Identity
        /// <para>If you get a null return you can check <see cref="ErrorOutput"/></para>
        /// </summary>
        /// <param name="clearText">The plain text</param>
        /// <param name="password">An additional password - this should NOT match your passwordIdentifier</param>
        /// <returns>Encrypted string or Null</returns>
        /// <exception cref="ArgumentNullException">throws when password is null, empty or whitespace</exception>
        /// <exception cref="ArgumentOutOfRangeException">throws when your password is too short, min 12 characters</exception>
        public static string Encrypt(string clearText, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            if (password.Length < 12)
            {
                throw new ArgumentOutOfRangeException(nameof(password));
            }

            if (string.IsNullOrWhiteSpace(Uid.UniqueID) || !Uid.UniqueID.Contains(S) || Uid.Identity == null)
            {
                ErrorOutput = PROVIDE_IDENTITY;
                return null!;
            }
            else
            {
                Wrap m = Crypt.Encrypt(clearText, Uid.Identity, password.ToSecure());
                if (m.Success)
                {
                    return m.Message;
                }
                else
                {
                    ErrorOutput = m.Error;
                    return null!;
                }
            }
        }

        /// <summary>
        /// Encrypt a string using the Initiazlied Identity and Additional Secure String Password
        /// <para>If you get a null return you can check <see cref="ErrorOutput"/></para>
        /// </summary>
        /// <param name="clearText">The plain text</param>
        /// <param name="securePassword">Additional Secure String Password - this should NOT match your passwordIdentifier</param>
        /// <returns>Encrypted string or Null</returns>
        /// <exception cref="ArgumentNullException">throws when password is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">throws when your password is too short, min 12 characters</exception>
        public static string Encrypt(string clearText, SecureString securePassword)
        {
            if (securePassword == null)
            {
                throw new ArgumentNullException(nameof(securePassword));
            }

            if (securePassword.Length < 12)
            {
                throw new ArgumentOutOfRangeException(nameof(securePassword));
            }

            if (string.IsNullOrWhiteSpace(Uid.UniqueID) || !Uid.UniqueID.Contains(S) || Uid.Identity == null)
            {
                ErrorOutput = PROVIDE_IDENTITY;
                return null!;
            }
            else
            {
                Wrap m = Crypt.Encrypt(clearText, Uid.Identity, securePassword);
                if (m.Success)
                {
                    return m.Message;
                }
                else
                {
                    ErrorOutput = m.Error;
                    return null!;
                }
            }
        }

        /// <summary>
        /// Encrypt a PlainText string using the Initiazlied Identity and Additional Secure String Password and return a Secure String CipherText
        /// <para>If you get a null return you can check <see cref="ErrorOutput"/></para>
        /// </summary>
        /// <param name="plainText">Secure String ClearText</param>
        /// <param name="securePassword">Additional Secure String Password - this should NOT match your passwordIdentifier</param>
        /// <returns>Secure String CipherText</returns>
        /// <exception cref="ArgumentNullException">throws when password is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">throws when your password is too short, min 12 characters</exception>
        public static SecureString Encrypt(SecureString plainText, SecureString securePassword)
        {
            if (securePassword == null)
            {
                throw new ArgumentNullException(nameof(securePassword));
            }

            if (securePassword.Length < 12)
            {
                throw new ArgumentOutOfRangeException(nameof(securePassword));
            }

            if (string.IsNullOrWhiteSpace(Uid.UniqueID) || !Uid.UniqueID.Contains(S) || Uid.Identity == null)
            {
                ErrorOutput = PROVIDE_IDENTITY;
                return null!;
            }
            else
            {
                WrapSS m = Crypt.Encrypt(plainText, Uid.Identity, securePassword);
                if (m.Success)
                {
                    return m.Message;
                }
                else
                {
                    ErrorOutput = m.Error;
                    return null!;
                }
            }
        }

        /// <summary>
        /// Decrypt a string using the Initiazlied Identity and Additional Password
        /// <para>If you get a null return you can check <see cref="ErrorOutput"/></para>
        /// </summary>
        /// <param name="cipherText">The cipher text</param>
        /// <param name="password">An additional password - this should NOT match your passwordIdentifier</param>
        /// <returns>Decrypted string or Null</returns>
        /// <exception cref="ArgumentNullException">throws when password is null, empty or whitespace</exception>
        /// <exception cref="ArgumentOutOfRangeException">throws when your password is too short, min 12 characters</exception>
        public static string Decrypt(string cipherText, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            if (password.Length < 12)
            {
                throw new ArgumentOutOfRangeException(nameof(password));
            }

            if (string.IsNullOrWhiteSpace(Uid.UniqueID) || !Uid.UniqueID.Contains(S) || Uid.Identity == null)
            {
                ErrorOutput = PROVIDE_IDENTITY;
                return null!;
            }
            else
            {
                Wrap m = Crypt.Decrypt(cipherText, Uid.Identity, password.ToSecure());
                if (m.Success)
                {
                    return m.Message;
                }
                else
                {
                    ErrorOutput = m.Error;
                    return null!;
                }
            }
        }

        /// <summary>
        /// Decrypt a string using the Initiazlied Identity and Additional Secure String Password
        /// <para>If you get a null return you can check <see cref="ErrorOutput"/></para>
        /// </summary>
        /// <param name="cipherText">The cipher text</param>
        /// <param name="securePassword">Additional Secure String Password - this should NOT match your passwordIdentifier</param>
        /// <returns>Decrypted string or Null</returns>
        /// <exception cref="ArgumentNullException">throws when password is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">throws when your password is too short, min 12 characters</exception>
        public static string Decrypt(string cipherText, SecureString securePassword)
        {
            if (securePassword == null)
            {
                throw new ArgumentNullException(nameof(securePassword));
            }

            if (securePassword.Length < 12)
            {
                throw new ArgumentOutOfRangeException(nameof(securePassword));
            }

            if (string.IsNullOrWhiteSpace(Uid.UniqueID) || !Uid.UniqueID.Contains(S) || Uid.Identity == null)
            {
                ErrorOutput = PROVIDE_IDENTITY;
                return null!;
            }
            else
            {
                Wrap m = Crypt.Decrypt(cipherText, Uid.Identity, securePassword);
                if (m.Success)
                {
                    return m.Message;
                }
                else
                {
                    ErrorOutput = m.Error;
                    return null!;
                }
            }
        }

        /// <summary>
        /// Decrypt a Secure String CipherText using the Initiazlied Identity and Additional Secure String Password and return a PlainText Secure String
        /// <para>If you get a null return you can check <see cref="ErrorOutput"/></para>
        /// </summary>
        /// <param name="cipherText">Secure String CipherText</param>
        /// <param name="securePassword">Additional Secure String Password - this should NOT match your passwordIdentifier</param>
        /// <returns>Decrypted string or Null</returns>
        /// <exception cref="ArgumentNullException">throws when password is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">throws when your password is too short, min 12 characters</exception>
        public static SecureString Decrypt(SecureString cipherText, SecureString securePassword)
        {
            if (securePassword == null)
            {
                throw new ArgumentNullException(nameof(securePassword));
            }

            if (securePassword.Length < 12)
            {
                throw new ArgumentOutOfRangeException(nameof(securePassword));
            }

            if (string.IsNullOrWhiteSpace(Uid.UniqueID) || !Uid.UniqueID.Contains(S) || Uid.Identity == null)
            {
                ErrorOutput = PROVIDE_IDENTITY;
                return null!;
            }
            else
            {
                WrapSS m = Crypt.Decrypt(cipherText, Uid.Identity, securePassword);
                if (m.Success)
                {
                    return m.Message;
                }
                else
                {
                    ErrorOutput = m.Error;
                    return null!;
                }
            }
        }

        /// <summary>
        /// Optionally change the bytes to use for the Salt
        /// <para>You should do this before you call <see cref="Initialize(string)"/> or <see cref="Initialize(SecureString)"/></para>
        /// <para>Not required just use a strong password</para>
        /// </summary>
        /// <param name="saltBytes"></param>
        public static void SetSaltBytes(byte[] saltBytes)
        {
            if (saltBytes.Length < 10)
            {
                throw new Exception(SALT_TOO_SMALL);
            }

            Uid.SaltBytes = saltBytes;
        }

        /// <summary>
        /// Check a validation string for an expected output to be sure identity has not changed since last initialization
        /// </summary>
        /// <param name="validationString">the validation string from a successful initialization or some string you encrypted</param>
        /// <param name="password">your identifier password</param>
        /// <param name="expectedOutput">the output you expect, default is Ok</param>
        /// <returns></returns>
        public static bool CheckSample(SecureString validationString, SecureString password, string expectedOutput = "Ok")
        {
            try
            {
                WrapSS m = Crypt.Decrypt(validationString, Uid.Identity, password);
                if (m.Success)
                {
                    if (m.Message.GetSecure().Equals(expectedOutput))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
    }
}
