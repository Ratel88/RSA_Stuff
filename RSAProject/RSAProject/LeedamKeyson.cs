using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Threading;
using HelperTools;

namespace LeedamAlgorithm
{
    public class LeedamKeyson //I will find you... and I will factor you.
    {
        /**
         * Returns p & q 
         **/
        #region Sequential_Loop
        public static BigInteger[] factorModulusSerial(BigInteger m)
        {
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
            Parallel.ForEach(RSAProject.BetterEnumerable.SteppedRange(number_to_start_on, new BigInteger(1), -2), (possible_factor, state) =>
            {
                if (m % possible_factor == 0) //If modulus (m) is perfectly factored by the current number, then we found a factor.
                {
                    state.Break();
                    factor = possible_factor; //Record that factor.
                }
            });

            return new BigInteger[] { factor, m / factor };
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
