using System;
using System.Collections.Generic;
using System.Text;

namespace FAuresco.RetryIt.Strategies
{
    /// <summary>
    /// Strategy that allows retry when specific exception type is thrown.
    /// </summary>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    public class ExceptionTypeRetryStrategy<TResult> : IRetryStrategy<TResult>
    {

        private readonly Type _ex;

        /// <summary>
        /// Strategy that allows retry when specific exception type is thrown.
        /// </summary>
        /// <param name="exceptionType">Type of the exception.</param>
        public ExceptionTypeRetryStrategy(Type exceptionType)
        {
            _ex = exceptionType ?? throw new ArgumentNullException("exceptionType");
        }

        /// <summary>
        /// Runs retry strategy and tells the executor if it must retry or not.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="result">The result of the function/code.</param>
        /// <param name="ex">The exception in case of any errors.</param>
        /// <returns>True if the executor must retry.</returns>
        public bool MustRetry(TResult result, Exception ex)
        {
            return ex != null && ex.GetType() == _ex;
        }
    }
}
