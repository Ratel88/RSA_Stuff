using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HelperTools
{
    class ByteArrayToString
    {
        public static string convert(Byte[] array)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(array);

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
}
