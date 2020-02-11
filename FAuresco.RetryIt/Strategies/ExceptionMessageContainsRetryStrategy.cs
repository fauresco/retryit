using System;
using System.Collections.Generic;
using System.Text;

namespace FAuresco.RetryIt.Strategies
{
    /// <summary>
    /// Strategy that allows retry when specific message is found int the exception message or in the exception message of the inner exceptions (recursive search).
    /// </summary>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    public class ExceptionMessageContainsRetryStrategy<TResult> : IRetryStrategy<TResult>
    {

        private readonly string _message;

        /// <summary>
        /// Strategy that allows retry when specific message is found int the exception message or in the exception message of the inner exceptions (recursive search).
        /// </summary>
        /// <param name="message">The message to be searched in the exception message.</param>
        public ExceptionMessageContainsRetryStrategy(string message)
        {
            if(string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("message");
            }

            _message = message;
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
            return ex != null && CheckException(ex, _message);
        }

        private bool CheckException(Exception ex, string message)
        {
            if (ex.Message.ToLower().Contains(message.ToLower()))
            {
                return true;
            }
            else
            {
                if (ex.InnerException != null)
                {
                    return CheckException(ex.InnerException, message);
                }
                else
                {
                    return false;
                }
            }

        }
    }
}
