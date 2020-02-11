using System;
using System.Collections.Generic;
using System.Text;

namespace FAuresco.RetryIt
{
    /// <summary>
    /// Holds the necessary parameters to execute the function/action and execute the retry logic.
    /// </summary>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    internal class RetryDefinition<TResult>
    {

        /// <summary>
        /// Holds the necessary parameters to execute the function/action and execute the retry logic.
        /// </summary>
        public RetryDefinition()
        {
            RetryStrategies = new List<IRetryStrategy<TResult>>();
        }

        /// <summary>
        /// The function that will be executed.
        /// </summary>
        public Func<TResult> TheFunction { get; set; }

        /// <summary>
        /// A list of retry strategies to be checked.
        /// </summary>
        public IList<IRetryStrategy<TResult>> RetryStrategies { get; set; }

        /// <summary>
        /// How many times we are going to retry in case of any retry strategy tells to retry.
        /// </summary>
        public int Times { get; set; }

        /// <summary>
        /// The delay (in miliseconds) between each retry.
        /// </summary>
        public int Delay { get; set; }

        /// <summary>
        /// Code to run when all attempts fail.
        /// </summary>
        public Action<TResult, Exception> OnFailure { get; set; }

        /// <summary>
        /// Code to run before each retry.
        /// </summary>
        public Action OnBeforeRetry { get; set; }

        /// <summary>
        /// Code to run after each retry.
        /// </summary>
        public Action<TResult, Exception> OnAfterRetry { get; set; }

        /// <summary>
        /// When true and all attempts fail, returns default result value instead of last value or exception.
        /// </summary>
        public bool IgnoreFailure { get; set; }

        /// <summary>
        /// When set and all attempts fail, this value is used to provide a default value to the caller instead of the last value or last exception.
        /// </summary>
        public TResult IgnoreFailureDefault { get; set; }

    }
}
