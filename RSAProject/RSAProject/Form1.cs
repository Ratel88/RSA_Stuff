using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Timers;
using System.Numerics;
using System.Diagnostics;

using System.Threading;

namespace RSAEncryption
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        #region-----Encryptionand Decryption Function-----
        static public byte[] Encryption(byte[] Data, RSAParameters RSAKey, bool DoOAEPPadding)
        {
            try
            {
                byte[] encryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(RSAKey);
                    encryptedData = RSA.Encrypt(Data, DoOAEPPadding);
                }
                return encryptedData;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }

        }

        static public byte[] Decryption(byte[] Data, RSAParameters RSAKey, bool DoOAEPPadding)
        {
            try
            {
                byte[] decryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(RSAKey);
                    decryptedData = RSA.Decrypt(Data, DoOAEPPadding);
                }
                return decryptedData;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());

                return null;
            }

        }
        #endregion

        

        #region--variables area
        UnicodeEncoding ByteConverter = new UnicodeEncoding();
        RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
        byte[] plaintext;
        byte[] encryptedtext;
        #endregion

        #region-- Function Implemantation
        private void button1_Click(object sender, EventArgs e)
        {
            plaintext = ByteConverter.GetBytes(txtplain.Text);
            encryptedtext = Encryption(plaintext, RSA.ExportParameters(false), false);
            txtencrypt.Text = ByteConverter.GetString(encryptedtext);


            using (RSACryptoServiceProvider playtest = new RSACryptoServiceProvider(384))// range from 384 to 16384 bits in 8 bit intervals
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                    
                string message = "Alice and Bob";
                byte[] unencoded_message = Encoding.ASCII.GetBytes(message);
                byte[] encoded_message = playtest.Encrypt(unencoded_message, false);
                RSAParameters parameters = playtest.ExportParameters(true);
                byte[] p = parameters.Modulus;
                
                string mod = HelperTools.ByteArrayToString.convert(parameters.Modulus);
                BigInteger p_big = new BigInteger(HelperTools.RSAParametersTranslator.translateParameter(parameters.P));
                BigInteger q_big = new BigInteger(HelperTools.RSAParametersTranslator.translateParameter(parameters.Q));
                BigInteger mod_big = new BigInteger(HelperTools.RSAParametersTranslator.translateParameter(parameters.Modulus));
                BigInteger[] factors;

                Task t = Task.Run(() =>
                {
                    factors = LeedamAlgorithm.LeedamKeyson.factorModulusSerial(new BigInteger(HelperTools.RSAParametersTranslator.translateParameter(parameters.Modulus)));
                });
                t.Wait();
                //int converted_bytes = BitConverter.ToInt32(p, 0);
                //IEnumerable<BigInteger> query = HelperTools.Erastosthenes.GetPrimeFactors(mod_big);
                stopWatch.Stop();
                Console.WriteLine("End.");
            }
        }
        
private void button2_Click(object sender, EventArgs e)
        {
            byte[] decryptedtex = Decryption(encryptedtext, RSA.ExportParameters(true), false);
            txtdecrypt.Text = ByteConverter.GetString(decryptedtex);
        }
        #endregion
    }
}
