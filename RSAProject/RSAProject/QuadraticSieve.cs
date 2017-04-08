using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Numerics;
using HelperTools;

namespace QuadraticSieveAlgorithm
{
    class QuadraticSieve
    {
        private BigInteger integer_to_factor;
        public QuadraticSieve(BigInteger integer_to_factor)
        {
            if (integer_to_factor < 1)
                return;
            else
                this.integer_to_factor = integer_to_factor;
        }

        public BigInteger[] getFactorsSerial()
        {
            BigInteger p = 1997333137;
            BigInteger q = 2106945901;

            BigInteger integer_to_factor = p * q;

            Console.WriteLine("p value: " + p);
            Console.WriteLine("q value: " + q);
            Console.WriteLine("Modulus: " + p * q);

            BigInteger[] bruteforce_Serial = LeedamKeyson.factorModulusSerial(integer_to_factor);

            Console.WriteLine("brute force serial p value: " + bruteforce_Serial[0]);
            Console.WriteLine("brute force serial q value: " + bruteforce_Serial[1]);


            BigInteger[] bruteforce_Parallel = LeedamKeyson.factorModulusParallel(integer_to_factor);

            Console.WriteLine("brute force parallel p value: " + bruteforce_Parallel[0]);
            //Console.WriteLine("brute force parallel q value" + bruteforce_Parallel[1]);


            int factor_serial = QuadraticSieveFunctions.SieveofSundaramSerial(integer_to_factor);
            Console.WriteLine("Serial returns the p value: " + factor_serial);
            Console.WriteLine("Serial returns the q value: " + (integer_to_factor / factor_serial));

            int factor_parallel = QuadraticSieveFunctions.SieveofSundaramParallel(integer_to_factor);

            Console.WriteLine("Parallel returns the p value: " + factor_parallel);
            Console.WriteLine("Parallel returns the q value: " + (integer_to_factor / factor_parallel));



            int factor_base_size = QuadraticSieveFunctions.getFactorBaseSize(integer_to_factor);
            factor_base_size = (int)BigIntegerWrapper.SqRt(integer_to_factor);
            BigInteger sieve_interval_upper = QuadraticSieveFunctions.getSieveInterval(factor_base_size);

            int[] factor_base = QuadraticSieveFunctions.SieveOfErastosthenesSerial(factor_base_size);
            factor_base = new int[] { 0 };
            factor_base = QuadraticSieveFunctions.SieveOfErastosthenesParallel(factor_base_size);




            int[] quadratic_residues = QuadraticSieveFunctions.getResiduesSerial(factor_base, integer_to_factor);

            BigInteger sieve_interval = QuadraticSieveFunctions.getSieveInterval(factor_base_size);
            BigInteger sieve_interval_start = BigIntegerWrapper.SqRt(integer_to_factor);

            LinkedList<BigInteger[]> smooth_numbers = QuadraticSieveFunctions.SmoothNumberSieveSerial(integer_to_factor, quadratic_residues, sieve_interval_start, sieve_interval_start + sieve_interval);
            //int k = 100000000;
            //QuadraticSieveFunctions.getFactorBaseSize(integer_to_factor);
            //int test_factor_base_size = QuadraticSieveFunctions.getFactorBaseSize(k);


            int result = factorSerial(factor_base, integer_to_factor);
            int result2 = factorParallel(factor_base, integer_to_factor);
            //int[] factor_base_parallel_improved = QuadraticSieveFunctions.SieveOfErastosthenesParallelImproved(2 * k);
            //int[] factor_base_parallel = QuadraticSieveFunctions.SieveOfErastosthenesParallel(2*k);

            return new BigInteger[] { result, (int)(sieve_interval_upper / result) };
        }

        private int factorParallel(int[] primes, BigInteger co_prime)
        {
            primes.Reverse();

            int a_factor = -1;

            Stopwatch factor_parallel = new Stopwatch();
            factor_parallel.Start();

            Parallel.ForEach(primes, (i, state) =>
            {
                if (co_prime % i == 0)
                {
                    state.Break();
                    a_factor = i;
                }
            });

            factor_parallel.Stop();
            Console.WriteLine("Parallel Factoring Runtime: " + factor_parallel.ElapsedMilliseconds + "ms.");

            return a_factor;
        }

        private int factorSerial(int[] primes, BigInteger co_prime)
        {
            primes.Reverse();

            int a_factor = -1;

            Stopwatch factor_serial = new Stopwatch();
            factor_serial.Start();

            for (int i = 0; i < primes.Length; i++)
            {
                if (co_prime % primes[i] == 0)
                {
                    a_factor = primes[i];
                    break;
                }
            }

            factor_serial.Stop();
            Console.WriteLine("Serial Factoring Runtime: " + factor_serial.ElapsedMilliseconds + "ms.");

            return a_factor;
        }

        public BigInteger[] getFactorsParallel()
        {
            int k = QuadraticSieveFunctions.getFactorBaseSize(integer_to_factor);
            return new BigInteger[] { new BigInteger(QuadraticSieveFunctions.getFactorBaseSize(integer_to_factor)), new BigInteger(QuadraticSieveFunctions.getFactorBaseSize(integer_to_factor)) };
        }

        public BigInteger BigRoot(BigInteger k, BigInteger integer_to_factor)
        {
            return (BigInteger.Pow((k + BigIntegerWrapper.SqRt(integer_to_factor)), 2) % integer_to_factor);
        }
    }

    /**
     * Many of the functions and approximations used in this class are taken from Damian Ball's paper on implementing a Quadratic Sieve from here:
     * http://damianball.com/pdf/portfolio/quadratic-sieve.pdf
     **/
    class QuadraticSieveFunctions
    {
        private const int min_parallel_list_size = 40000;

        /**
         * The Factor Base size B is given by (e ^ (sqrt(r))) ^ (sqrt(2)/4)
         * 
         * Where r = ln(n) * ln(ln(n))
         * 
         * Even for numbers in the millions of trillions, r ends up being very small, so in order to do exact math we assume r can fit inside a double.
         * 
         * Since we must have an integer number of factor bases, we take the floor of B.
         **/
        public static int getFactorBaseSize(BigInteger number_to_factor)
        {
            double r = BigInteger.Log(number_to_factor) * (BigInteger.Log(BigIntegerWrapper.Log(number_to_factor)));

            double exact_value = Math.Pow(Math.Pow(Math.E, Math.Sqrt(r)), (Math.Sqrt(2) / 4));
            return (int)Math.Floor(exact_value);
        }

        public static int[] SieveOfErastosthenesSerial(int factor_base_size)
        {
            var primes = new LinkedList<int>();

            primes.AddLast(2);

            int[] marked_for_removal = new int[factor_base_size];

            Stopwatch erastosthenes_serial = new Stopwatch();
            erastosthenes_serial.Start();

            for (int k = 3; k < factor_base_size; k += 2)
            {
                if (marked_for_removal[k / 2] == 0)
                {
                    primes.AddLast(k);
                    for (int i = k * k; i < factor_base_size && i > 0; i += 2 * k)
                        marked_for_removal[i / 2]++;
                }
            }

            erastosthenes_serial.Stop();
            Console.WriteLine("Serial Sieve of Erastosthenes Runtime: " + erastosthenes_serial.ElapsedMilliseconds + "ms.");

            return primes.ToArray();
        }

        public static int[] SieveOfErastosthenesParallel(int factor_base_size)
        {
            var primes = new LinkedList<int>();

            primes.AddLast(2);

            int[] marked_for_removal = new int[factor_base_size];

            Stopwatch erastothenes_parallel = new Stopwatch();
            erastothenes_parallel.Start();

            for (int k = 3; k < factor_base_size; k += 2)
            {
                if (marked_for_removal[k / 2] == 0)
                {
                    primes.AddLast(k);

                    if (factor_base_size - (k * k) < 30000)
                    {
                        for (int i = k * k; i < factor_base_size && i > 0; i += 2 * k)
                            marked_for_removal[i / 2]++;
                    }
                    else
                    {
                        Parallel.For(0, (factor_base_size - (k * k)) / (2 * k), i =>
                            {
                                int index = (int)(new BigInteger(k * k) + new BigInteger(2 * k * i));
                                if (index >= 0)
                                    marked_for_removal[index / 2]++;
                            });
                    }

                }
            }

            erastothenes_parallel.Stop();
            Console.WriteLine("Parallel Sieve of Erastosthenes Runtime: " + erastothenes_parallel.ElapsedMilliseconds + "ms.");

            return primes.ToArray();
        }

        public static int SieveofSundaramSerial(BigInteger to_factor)
        {
            if (BigIntegerWrapper.SqRt(to_factor) < int.MaxValue - 1)
            {
                int BAsize = (int)BigIntegerWrapper.SqRt(to_factor);
                int k = BAsize / 2;

                BitArray tracker = new BitArray(BAsize);

                /* SET ALL TO PRIME STATUS */
                tracker.SetAll(true);

                int maxVal = 0;
                int denominator = 0;

                Stopwatch sundaram_serial = new Stopwatch();
                sundaram_serial.Start();

                for (int i = 1; i < k; i++)
                {
                    denominator = (i << 1) + 1;
                    maxVal = (k - i) / denominator;
                    for (int j = i; j <= maxVal; j++)
                    {
                        tracker[i + j * denominator] = false;
                    }
                }

                int dummy = 0;
                for (int i = 1; i < k; i++)
                {
                    if (tracker[i])
                    {
                        dummy = (i << 1) + 1; // dummy contains prime number.The code is here not ignore the prime number calcuation part.
                        if (to_factor % dummy == 0)
                        {
                            break;
                        }
                    }
                }

                sundaram_serial.Stop();
                Console.WriteLine("Serial Sieve of Sundaram Runtime: " + sundaram_serial.ElapsedMilliseconds + "ms.");

                return dummy;
            }
            else
            {
                Console.WriteLine("Too big to factor with Sieve of Sundaram. ");
                return -1;
            }
        }

        public static int SieveofSundaramParallel(BigInteger to_factor)
        {
            if (BigIntegerWrapper.SqRt(to_factor) < int.MaxValue - 1)
            {
                int BAsize = (int)BigIntegerWrapper.SqRt(to_factor);
                int k = BAsize / 2;

                BitArray tracker = new BitArray(BAsize);

                /* SET ALL TO PRIME STATUS */
                tracker.SetAll(true);

                int maxVal = 0;
                int denominator = 0;

                Stopwatch sundaram_parallel = new Stopwatch();
                sundaram_parallel.Start();

                for (int i = 1; i < k; i++)
                {
                    denominator = (i << 1) + 1;
                    maxVal = (k - i) / denominator;
                    for (int j = i; j <= maxVal; j++)
                    {
                        tracker[i + j * denominator] = false;
                    }
                }

                int dummy = 0;
                int final = 0;
                Parallel.For(1, k, (i, state) =>
                {
                    if (tracker[i])
                    {
                        dummy = (i << 1) + 1; // dummy contains prime number.The code is here not ignore the prime number calcuation part.
                        if (to_factor % dummy == 0)
                        {
                            final = dummy;
                            state.Break();
                        }
                    }
                });

                sundaram_parallel.Stop();
                Console.WriteLine("Parallel Sieve of Sundaram Runtime: " + sundaram_parallel.ElapsedMilliseconds + "ms.");

                return final;
            }
            else
            {
                Console.WriteLine("Too big to factor with Sieve of Sundaram. ");
                return -1;
            }
        }

        public static int[] getResiduesSerial(int[] factor_base, BigInteger number_to_factor)
        {
            LinkedList<int> residues = new LinkedList<int>();

            foreach (int element in factor_base)
            {
                if (Legendre(number_to_factor, element) == 1)
                    residues.AddLast(element);
            }

            return residues.ToArray();
        }

        public static LinkedList<BigInteger[]> SmoothNumberSieveSerial(BigInteger number_to_factor, int[] list_of_primes, BigInteger interval_start, BigInteger interval_end)
        {
            LinkedList<BigInteger[]> smooth_number_power = new LinkedList<BigInteger[]>();

            for (BigInteger i = interval_start; i < interval_end; i++)
            {
                BigInteger candidate = computeQX(i, number_to_factor);
                BigInteger[] exponents = new BigInteger[list_of_primes.Length];
                for (int j = 0; j < list_of_primes.Length; j++)
                {
                    if (candidate == 15605429)
                        Console.Write("Here I am.");

                    bool flag = true;
                    BigInteger gcd;
                    int exponent_count = 0;
                    while (flag)
                    {
                        gcd = BigInteger.GreatestCommonDivisor(candidate, list_of_primes[j]);
                        if (gcd != 1 && gcd != candidate)
                        {
                            candidate /= gcd;
                            exponent_count++;
                        }
                        else
                            flag = false;
                    }

                    exponents[j] = exponent_count;

                    if (candidate == 1)
                    {
                        smooth_number_power.AddLast(exponents);
                        break;
                    }
                }
            }

            return smooth_number_power;
        }

        public static LinkedList<BigInteger[]> SmoothNumberSieve(BigInteger number_to_factor, int[] list_of_primes, BigInteger interval_start, BigInteger interval_end)
        {
            LinkedList<BigInteger[]> smooth_number_power = new LinkedList<BigInteger[]>();

            for (BigInteger i = interval_start; i < interval_end; i++)
            {
                BigInteger candidate = computeQX(i, number_to_factor);
                BigInteger[] exponents = new BigInteger[list_of_primes.Length];
                for (int j = 0; j < list_of_primes.Length; j++)
                {
                    if (candidate == 15605429)
                        Console.Write("Here I am.");

                    bool flag = true;
                    BigInteger gcd;
                    int exponent_count = 0;
                    while (flag)
                    {
                        gcd = BigInteger.GreatestCommonDivisor(candidate, list_of_primes[j]);
                        if (gcd != 1 && gcd != candidate)
                        {
                            candidate /= gcd;
                            exponent_count++;
                        }
                        else
                            flag = false;
                    }

                    exponents[j] = exponent_count;

                    if (candidate == 1)
                    {
                        smooth_number_power.AddLast(exponents);
                        break;
                    }
                }
            }

            return smooth_number_power;
        }

        public static BigInteger computeQX(BigInteger x, BigInteger n)
        {
            return BigInteger.Pow((x + BigIntegerWrapper.SqRt(n)), 2) % n;
        }

        public static BigInteger getSieveInterval(int factor_base_size)
        {
            return BigInteger.Pow(factor_base_size, 3);
        }

        /**
         * Adopted from the pseudocode at: https://martin-thoma.com/how-to-calculate-the-legendre-symbol/
         * 
         * An extra [else if] is left out since we're assuming that we'll only be checking numbers
         * that are prime.
         **/
        public static int Legendre(BigInteger a, BigInteger prime)
        {
            if (a >= prime || a < 0)
                return Legendre(a % prime, prime);
            else if (a == 0 || a == 1)
            {
                if (a == 0)
                    return 0;
                else
                    return 1;
            }
            else if (a == 2)
            {
                if (isCongruent(prime, 1, 8) || isCongruent(prime, -1, 8))
                    return 1;
                else
                    return -1;
            }
            else if (a == (prime - 1))
            {
                if (isCongruent(prime, 1, 4))
                    return 1;
                else
                    return -1;
            }
            else
            {
                if (isCongruent(((prime - 1) / 2), 0, 2) || isCongruent(((a - 1) / 2), 0, 2))
                    return Legendre(prime, a);
                else
                    return (-1) * Legendre(prime, a);
            }
        }

        public static bool isCongruent(BigInteger b, BigInteger c, double m)
        {
            if (((double)(b - c) / m) % 1 == 0)
                return true;
            else
                return false;
        }

        public static int[] getIterationList(int start_inclusive, int stop_exclusive, int step_value)
        {
            int[] to_return = new int[((stop_exclusive - start_inclusive) / step_value) + 1];

            for (int i = 0; i < to_return.Length; i++)
                to_return[i] = start_inclusive + (i * step_value);

            return to_return;
        }
    }

    class BigIntegerWrapper
    {
        public static BigInteger Log(BigInteger n)
        {
            return new BigInteger(BigInteger.Log(n));
        }

        public static BigInteger SqRt(BigInteger N)
        {
            return new BigInteger(Math.Exp(BigInteger.Log(N) / 2));
        }
    }
    public class LeedamKeyson //I will find you... and I will factor you.
    {
        #region Sequential_Loop
        public static BigInteger[] factorModulusSerial(BigInteger m)
        {
            Stopwatch factor_serial = new Stopwatch();
            factor_serial.Start();

            bool factored_it = false;

            BigInteger number_to_start_on = BigIntegerTools.SqRt(m);
            BigInteger factor = 0;

            if (number_to_start_on.IsEven)
                number_to_start_on--;

            BigInteger initial_number = number_to_start_on;

            Thread.CurrentThread.Priority = ThreadPriority.Highest; //Set main thread priority to highest (pretty sure this is "Real Time");

            while (!factored_it)
            {
                if (m % number_to_start_on == 0) //If modulus (m) is perfectly factored by the current number, then we found a factor.
                {
                    factored_it = true;
                    factor = number_to_start_on; //Record that factor.
                }
                else
                    number_to_start_on -= 2;
            }

            Thread.CurrentThread.Priority = ThreadPriority.Normal; //Set main thread priority back to normal.
            factor_serial.Stop();
            Console.WriteLine("Serial Factoring Runtime: " + factor_serial.ElapsedMilliseconds + "ms.");
            return new BigInteger[] { factor, m / factor };
        }
        #endregion

        /**
         * Returns p & q 
         **/
        #region Parallel_Loop
        public static BigInteger[] factorModulusParallel(BigInteger m)
        {
            BigInteger number_to_start_on = BigIntegerTools.SqRt(m);
            BigInteger factor = 0;

            if (number_to_start_on.IsEven)
                number_to_start_on--;

            BigInteger initial_number = number_to_start_on;

            /**
             * MSDN blog on implementing parallel while loop that are breakable: https://blogs.msdn.microsoft.com/pfxteam/2009/08/12/implementing-parallel-while-with-parallel-foreach/
             * Pretty rough to read though, and our issue is that we also need a -=2 iterator not ++.
             * 
             * So a ForEach parallelized loop is used instead. Notice that the "BetterEnumerable" class method uses yield return so it should be perfectly efficient for memory usage.
             * In this way we generate a: from initial number to 0, decrements of 2 parallelized for loop.
             * 
             * Since we're guaranteed that at some point a thread will find the factor, then once that occurs we break the loop (which is actually a method) 
             * to prevent any further iterations from running and then return our factors.
             * 
             * We're also guaranteed that only one thread will ever execute the break statement since each enumerable is unique. So we will get exactly one factor.
             * 
             **/
            /*Parallel.ForEach(RSAProject.BetterEnumerable.SteppedRange(number_to_start_on, new BigInteger(1), -2), (possible_factor, state) =>
            {
                if (m % possible_factor == 0) //If modulus (m) is perfectly factored by the current number, then we found a factor.
                {
                    state.Break();
                    factor = possible_factor; //Record that factor.
                }
            });*/

            return new BigInteger[] { factor, m };
        }
        #endregion

        /**
         * Unused, might be useful later on? But doubtful, it's pretty complicated.
         **/
        private static void While(ParallelOptions parallelOptions, Func<bool> condition, Action<ParallelLoopState> body)
        {
            Parallel.ForEach(new RSAProject.InfinitePartitioner(), parallelOptions, (ignored, loopState) =>
            {
                if (condition())
                    body(loopState);
                else
                    loopState.Stop();
            });
        }
    }
}
