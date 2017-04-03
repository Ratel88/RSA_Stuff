using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

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
            int factor_base_size = QuadraticSieveFunctions.getFactorBaseSize(integer_to_factor);
            BigInteger sieve_interval_upper = QuadraticSieveFunctions.getSieveInterval(factor_base_size);
            BigInteger sieve_interval_lower = 1;

            BigInteger to_factor = new BigInteger(10235749) * new BigInteger(23572337);

            int k = (int)BigIntegerWrapper.SqRt(to_factor);
            //int k = 100000000;
            // QuadraticSieveFunctions.getFactorBaseSize(integer_to_factor);
            //int test_factor_base_size = QuadraticSieveFunctions.getFactorBaseSize(k);
            int[] factor_base_serial = QuadraticSieveFunctions.SieveOfErastosthenesSerial(k);
            

            int result = factorSerial(factor_base_serial, to_factor);
            int resul2 = factorParallel(factor_base_serial, to_factor);
            //int[] factor_base_parallel_improved = QuadraticSieveFunctions.SieveOfErastosthenesParallelImproved(2 * k);
            //int[] factor_base_parallel = QuadraticSieveFunctions.SieveOfErastosthenesParallel(2*k);

            int ctr = 0;
            for (int i = 0; i < factor_base_serial.Length; i++)
                if (QuadraticSieveFunctions.Legendre(k, factor_base_serial[i]) == 1)
                    ctr++;

            Console.WriteLine("Generated " + ctr + " numbers with quadratic residue.");

            return new BigInteger[] { new BigInteger(QuadraticSieveFunctions.getFactorBaseSize(integer_to_factor)), new BigInteger(QuadraticSieveFunctions.getFactorBaseSize(integer_to_factor)) };
        }

        private int factorParallel(int[] primes, BigInteger co_prime)
        {
            primes.Reverse();

            int a_factor = -1;

            Stopwatch factor_parallel = new Stopwatch();
            factor_parallel.Start();

            Parallel.For(0, primes.Length, (i, state) =>
            {
                if (co_prime % primes[i] == 0)
                {
                    state.Break();
                    a_factor = primes[i];
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
                    break;
                    a_factor = primes[i];
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

        /**
         * It's pretty much impossible to modify data that is being used in parallel for obvious reasons.
         * 
         * What we can do instead is mark "non-prime" entries for removal in parallel and then modify
         * the list in serial.
         * 
         * The parallel implementation of the sieve uses a second array that is written to in parallel to
         * indicate entries in the factor base list that are not primes to mark them for removal later.
         * Once these entries are removed (which must be done in serial) the resulting list will be identical
         * to that obtained from the serial algorithm.
         **/
        public static int[] SieveOfErastosthenesParallel(int factor_base_size)
        {
            var factor_base = new List<int>();
            factor_base.Add(2);
            factor_base.Add(3);

            for (int i = 5; i < factor_base_size; i += 2)
                factor_base.Add(i);

            int p = 0;
            int m = factor_base[factor_base.Count - 1];
            int[] marked_for_removal = new int[factor_base.Count];

            Stopwatch erastosthenes_parallel = new Stopwatch();
            erastosthenes_parallel.Start();

            for (int i = 0; p < m && i < factor_base.Count; i++)
            {
                //As the list starts becoming small, the overhead of coordinating threads starts to outweight the benefits of parallelization.
                //So we allow for a hybrid approach to execute in serial when the list starts becoming very small.
                if (factor_base.Count < min_parallel_list_size)
                {
                    p = factor_base[i];

                    for (int j = i + 1; j < factor_base.Count; j++)
                        if (factor_base[j] % p == 0)
                            factor_base.RemoveAt(j);
                }
                else 
                {
                    Parallel.For(i + 1, factor_base.Count, j =>
                    {
                        p = factor_base[i];

                        if (factor_base[j] % p == 0)
                            marked_for_removal[j] = 1;
                        else
                            marked_for_removal[j] = 0;
                    });

                    for (int k = factor_base.Count - 1; k > 0; k--)
                        if (marked_for_removal[k] == 1)
                            factor_base.RemoveAt(k);
                }
                
            }

            erastosthenes_parallel.Stop();
            Console.WriteLine("Parallel Sieve of Erastosthenes Runtime: " + erastosthenes_parallel.ElapsedMilliseconds + "ms.");

            return factor_base.ToArray();
        }
      
        public static int[] SieveOfErastosthenesParallelImproved(int factor_base_size)
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
                    Parallel.ForEach(RSAProject.BetterEnumerable.SteppedRange(k*k, factor_base_size, 2), i =>
                      {
                          marked_for_removal[i]++;
                      });
                    /*for (int i = k * k; i < factor_base_size && i > 0; i += 2 * k)
                        marked_for_removal[i / 2]++;*/
                }
            }

            erastosthenes_serial.Stop();
            Console.WriteLine("Serial Sieve of Erastosthenes Runtime: " + erastosthenes_serial.ElapsedMilliseconds + "ms.");

            return primes.ToArray();
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
        public static int Legendre(BigInteger a, BigInteger p)
        {
            if (a >= p || a < 0)
                return Legendre(a % p, p);
            else if (a == 0 || a == 1)
            {
                if (a == 0)
                    return 0;
                else
                    return 1;
            }
            else if (a == 2)
            {
                if (isCongruent(p, 1, 8) || isCongruent(p, -1, 8))
                    return 1;
                else
                    return -1;
            }
            else if (a == (p - 1))
            {
                if (isCongruent(p, 1, 4))
                    return 1;
                else
                    return -1;
            }
            else
            {
                if (isCongruent(((p - 1) / 2), 0, 2) || isCongruent(((a - 1) / 2), 0, 2))
                    return Legendre(p, a);
                else
                    return (-1) * Legendre(p, a);
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

            for(int i = 0; i < to_return.Length; i++)
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

        /**
         * Returns the floor of the result of n raised to p
         **/
        /*public static BigInteger Pow(BigInteger n, double p)
        {
            //Compute the integer part of the exponent
            int int_of_exponent = (int)Math.Floor(p);
            //Compute the double part of the exponent
            double fraction_of_exponent = p - int_of_exponent;

            BigInteger temp = BigInteger.Pow(n, int_of_exponent);
            double temp2 = Math.Pow(n, fraction_of_exponent);
        }*/

        public static BigInteger SqRt(BigInteger N)
        {
            return new BigInteger(Math.Exp(BigInteger.Log(N) / 2));
        }
    }
}
