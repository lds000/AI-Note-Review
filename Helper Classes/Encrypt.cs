﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace AI_Note_Review
{
    public class Encryption
    {
        private static string encryptionKey;
        public static string EncryptionKey
        {
            get
            {
                if (encryptionKey == null)
                {
                    WinEnterText we = new WinEnterText("Enter Encryption Key");
                    we.ShowDialog();
                    we.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    if (we.ReturnValue != null)
                    {
                        encryptionKey = we.ReturnValue;
                    }
                    return encryptionKey;
                }
                return EncryptionKey;
            }
        }

        public static string Encrypt(string textToEncrypt)
        {
            try
            {
                string ToReturn = "";
                string publickey = "12345678";
                string secretkey = EncryptionKey;
                byte[] secretkeyByte = { };
                secretkeyByte = System.Text.Encoding.UTF8.GetBytes(secretkey);
                byte[] publickeybyte = { };
                publickeybyte = System.Text.Encoding.UTF8.GetBytes(publickey);
                MemoryStream ms = null;
                CryptoStream cs = null;
                byte[] inputbyteArray = System.Text.Encoding.UTF8.GetBytes(textToEncrypt);
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateEncryptor(publickeybyte, secretkeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    ToReturn = Convert.ToBase64String(ms.ToArray());
                }
                return ToReturn;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        static string EncryptNote(string strNote)
        {
            //remove name information.
            //Change DOB to months, not exact date.
            return Encrypt(strNote);
        }

        /*
        static void Main(string[] args)
        {
            string encrypted = Encrypt();
            Console.WriteLine(encrypted);
        }
        */

        public static string Decrypt(string textToDecrypt)
        {
            try
            {
                string ToReturn = "";
                string publickey = "12345678";
                string secretkey = EncryptionKey;
                byte[] privatekeyByte = { };
                privatekeyByte = System.Text.Encoding.UTF8.GetBytes(secretkey);
                byte[] publickeybyte = { };
                publickeybyte = System.Text.Encoding.UTF8.GetBytes(publickey);
                MemoryStream ms = null;
                CryptoStream cs = null;
                byte[] inputbyteArray = new byte[textToDecrypt.Replace(" ", "+").Length];
                inputbyteArray = Convert.FromBase64String(textToDecrypt.Replace(" ", "+"));
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateDecryptor(publickeybyte, privatekeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    Encoding encoding = Encoding.UTF8;
                    ToReturn = encoding.GetString(ms.ToArray());
                }
                return ToReturn;
            }
            catch (Exception ae)
            {
                throw new Exception(ae.Message, ae.InnerException);
            }
        }

        /*
         Static void Main(string[] args)
        {
            string decrypted = Decrypt();
            Console.WriteLine(decrypted);
        }
         */

    }
}