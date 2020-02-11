using System;
using System.Collections.Generic;
using System.Text;

namespace FAuresco.RetryIt.Strategies
{
    /// <summary>
    /// Strategy that allows retry when a provided custom function returns true.
    /// </summary>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    public class CustomRetryStrategy<TResult> : IRetryStrategy<TResult>
    {

        private readonly Func<TResult, Exception, bool> _customFunction;

        /// <summary>
        /// Strategy that allows retry when a provided custom function returns true. 
        /// </summary>
        /// <param name="customFunction">A function that receives the result value of the original function and the exception in case one was thrown and outputs true or false indicating the executor should retry or not.</param>
        public CustomRetryStrategy(Func<TResult, Exception, bool> customFunction)
        {
            _customFunction = customFunction;
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
            return _customFunction.Invoke(result, ex);
        }
    }
}
