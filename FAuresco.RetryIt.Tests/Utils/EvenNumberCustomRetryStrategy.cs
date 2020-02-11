using System;
using System.Collections.Generic;
using System.Text;

namespace FAuresco.RetryIt.Tests.Utils
{
    /// <summary>
    /// Retry when the result is not even.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public class EvenNumberCustomRetryStrategy<TResult> : IRetryStrategy<TResult>
    {
        /// <summary>
        /// Runs retry strategy and tells the executor if it must retry or not.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="result">The result of the function/code.</param>
        /// <param name="ex">The exception in case of any errors.</param>
        /// <returns>True if the executor must retry.</returns>
        public bool MustRetry(TResult result, Exception ex)
        {
            var intResult = Convert.ToInt32(result);
            if (intResult % 2 != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
