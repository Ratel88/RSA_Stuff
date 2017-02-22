using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace RSAProject
{
    class Program
    {
        UnicodeEncoding ByteConverter = new UnicodeEncoding();
        RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
        byte[] plaintext;
        byte[] encryptedtext;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// I changed some stuff here.
        [STAThread]
        static void Main()
        {
            using (RSACryptoServiceProvider playtest = new RSACryptoServiceProvider(1024))
            {
                string message = "Alice and Bob";
                byte[] unencoded_message = Encoding.ASCII.GetBytes(message);
                byte[] encoded_message = playtest.Encrypt(unencoded_message, false);
                RSAParameters parameters = playtest.ExportParameters(true);
                byte[] p = parameters.P;
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(p);

                int converted_bytes = BitConverter.ToInt32(p, 0);

                Console.WriteLine("End.");
            }
            
        }
    }
}
