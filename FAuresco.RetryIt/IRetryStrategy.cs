using System;

namespace FAuresco.RetryIt
{
    /// <summary>
    /// Retry strategy.
    /// </summary>
    public interface IRetryStrategy<TResult>
    {

        /// <summary>
        /// Runs retry strategy and tells the executor if it must retry or not.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="result">The result of the function/code.</param>
        /// <param name="ex">The exception in case of any errors.</param>
        /// <returns>True if the executor must retry.</returns>
        bool MustRetry(TResult result, Exception ex);

    }
}