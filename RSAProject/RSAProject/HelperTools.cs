using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HelperTools
{
    class ByteArrayToString
    {
        //converts a byte[] to string equivalent to a decimal number. 
        public static string convert(byte[] array)
        {
            BigInteger final_number = 0;
            BigInteger exponent_calculator = 0;
            for(int i = 0; i < array.Length; i++)
            {
                exponent_calculator = BigInteger.Pow(array[i], i);
                final_number += exponent_calculator;
            }

            return final_number.ToString();
        }

    }

    class RSAParametersTranslator
    {
        //Reverses a byte-array so that the BigInteger constructor will properly convert it to the object.
        public static byte[] translateParameter(byte[] array)
        {
            byte[] temp = new byte[array.Length];
            Array.Copy(array, temp, array.Length);
            Array.Reverse(temp);

            if ((temp[temp.Length - 1] & 0x80) != 0)
                Array.Resize<byte>(ref temp, temp.Length + 1);

            return temp;
        }
    }
    
    public class Erastosthenes: IEnumerable<BigInteger>
    {
        private BigInteger to_factor;
        private static List<BigInteger> _primes = new List<BigInteger>();
        private BigInteger _lastChecked;

        public Erastosthenes(BigInteger input)
        {
            to_factor = input;
            _primes.Add(2);
            _lastChecked = 2;
        }

        //BigInteger test = new BigInteger(4);
        //BigInteger result = new BigInteger(Math.Exp(BigInteger.Log(test) / 2));
        private bool isPrime(BigInteger checkValue)
        {
            bool isPrime = true;

            foreach (BigInteger prime in _primes)
            {
                if ((checkValue % prime) == 0 && prime <= SqRt(checkValue))
                {
                    isPrime = false;
                    break;
                }
            }

            return isPrime;
        }

        public IEnumerator<BigInteger> GetEnumerator()
        {
            foreach (BigInteger prime in _primes)
            {
                yield return prime;
            }

            while(_lastChecked <= SqRt(to_factor))
            {
                _lastChecked++;

                if(isPrime(_lastChecked))
                {
                    _primes.Add(_lastChecked);
                    yield return _lastChecked;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private BigInteger SqRt(BigInteger N)
        {
            return new BigInteger(Math.Exp(BigInteger.Log(N) / 2));
        }

        public static IEnumerable<BigInteger> GetPrimeFactors(BigInteger value)
        {
            List<BigInteger> factors = new List<BigInteger>();
            Erastosthenes erastosthenes = new Erastosthenes(value);
            foreach (int prime in erastosthenes)
            {
                while(value % prime == 0)
                {
                    value /= prime;
                    factors.Add(prime);
                }

                if (value == 1)
                {
                    break;
                }
            }

            return factors;
        }
    }
}
