using FAuresco.RetryIt.Strategies;
using System;
using System.Linq;
using System.Threading;

namespace FAuresco.RetryIt
{
    /// <summary>
    /// Contains methods to fill in the properties of the <see cref="RetryDefinition{TResult}"/> class as well as the method Go which is used to execute the definition.
    /// </summary>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    public class RetryDefinitionBuilder<TResult>
    {

        private readonly RetryDefinition<TResult> _definition = new RetryDefinition<TResult>();

        /// <summary>
        /// Contains methods to fill in the properties of the <see cref="RetryDefinition{TResult}"/> class as well as the method Go which is used to execute the definition.
        /// </summary>
        /// <param name="func">The function that will be executed.</param>
        public RetryDefinitionBuilder(Func<TResult> func)
        {
            _definition.TheFunction = func;
        }

        /// <summary>
        /// Retry when the exception message contains specific message. It recusively searches the exception message of the inner exceptions, if any.
        /// </summary>
        /// <param name="message">The message to search.</param>
        /// <returns>Returns this instance of <see cref="RetryDefinitionBuilder{TResult}"/>.</returns>
        public RetryDefinitionBuilder<TResult> WhenExceptionMessageContains(string message)
        {
            _definition.RetryStrategies.Add(new ExceptionMessageContainsRetryStrategy<TResult>(message));
            return this;
        }

        /// <summary>
        /// Retry when the exception is of specific type.
        /// </summary>
        /// <param name="exceptionType">Type of the exception.</param>
        /// <returns>Returns this instance of <see cref="RetryDefinitionBuilder{TResult}"/>.</returns>
        public RetryDefinitionBuilder<TResult> WhenExceptionTypeIs(Type exceptionType)
        {
            _definition.RetryStrategies.Add(new ExceptionTypeRetryStrategy<TResult>(exceptionType));
            return this;
        }

        /// <summary>
        /// Retry when the provided custom function returns true.
        /// </summary>
        /// <param name="customFunction">A custom function that will receive the result of the original function and an exception (in case one was thrown). It must return true to trigger the retry.</param>
        /// <returns>Returns this instance of <see cref="RetryDefinitionBuilder{TResult}"/>.</returns>
        public RetryDefinitionBuilder<TResult> WhenCustom(Func<TResult, Exception, bool> customFunction)
        {
            _definition.RetryStrategies.Add(new CustomRetryStrategy<TResult>(customFunction));
            return this;
        }

        /// <summary>
        /// Retry when provided custom retry strategy returns true.
        /// </summary>
        /// <param name="customStrategy">A custom retry strategy implementation of <see cref="IRetryStrategy{TResult}"/>.</param>
        /// <returns>Returns this instance of <see cref="RetryDefinitionBuilder{TResult}"/>.</returns>
        public RetryDefinitionBuilder<TResult> WhenCustomStrategy(IRetryStrategy<TResult> customStrategy)
        {
            _definition.RetryStrategies.Add(customStrategy);
            return this;
        }

        /// <summary>
        /// Set how many times the function will be executed in case of failure. So if you set a value of 50, the function will be executed one time + 50 at maximum.
        /// </summary>
        /// <param name="times">How many times to retry.</param>
        /// <returns>Returns this instance of <see cref="RetryDefinitionBuilder{TResult}"/>.</returns>
        public RetryDefinitionBuilder<TResult> Times(int times)
        {
            _definition.Times = times;
            return this;
        }

        /// <summary>
        /// Sets a delay between retries (in miliseconds). This is useful, for instance, when you hit a database timeout error and want to wait a little bit to give time to the server finish whathever it was doing.
        /// </summary>
        /// <param name="ms">The number of miliseconds to wait.</param>
        /// <returns>Returns this instance of <see cref="RetryDefinitionBuilder{TResult}"/>.</returns>
        public RetryDefinitionBuilder<TResult> Delay(int ms)
        {
            _definition.Delay = ms;
            return this;
        }

        /// <summary>
        /// Runs the specified action if all attempts fail.
        /// </summary>
        /// <param name="failureAction">Code to run.</param>
        /// <returns>Returns this instance of <see cref="RetryDefinitionBuilder{TResult}"/>.</returns>
        public RetryDefinitionBuilder<TResult> OnFailure(Action<TResult, Exception> failureAction)
        {
            _definition.OnFailure = failureAction;
            return this;
        }

        /// <summary>
        /// Runs the specified action before each retry.
        /// </summary>
        /// <param name="beforeRetryAction">Code to run.</param>
        /// <returns>Returns this instance of <see cref="RetryDefinitionBuilder{TResult}"/>.</returns>
        public RetryDefinitionBuilder<TResult> BeforeRetry(Action beforeRetryAction)
        {
            _definition.OnBeforeRetry = beforeRetryAction;
            return this;
        }

        /// <summary>
        /// Runs the specified action after each retry.
        /// </summary>
        /// <param name="afterRetryAction">Code to run.</param>
        /// <returns>Returns this instance of <see cref="RetryDefinitionBuilder{TResult}"/>.</returns>
        public RetryDefinitionBuilder<TResult> AfterRetry(Action<TResult, Exception> afterRetryAction)
        {
            _definition.OnAfterRetry = afterRetryAction;
            return this;
        }

        /// <summary>
        /// When and all attempts fail, does not throw the last exception and instead returns the default value for the result type.
        /// </summary>
        /// <returns>Returns this instance of <see cref="RetryDefinitionBuilder{TResult}"/>.</returns>
        public RetryDefinitionBuilder<TResult> IgnoreFailure()
        {
            _definition.IgnoreFailure = true;
            _definition.IgnoreFailureDefault = default(TResult);
            return this;
        }

        /// <summary>
        /// When and all attempts fail, does not throw the last exception and instead returns the specified value.
        /// </summary>
        /// <returns>Returns this instance of <see cref="RetryDefinitionBuilder{TResult}"/>.</returns>
        public RetryDefinitionBuilder<TResult> IgnoreFailure(TResult defaultValue)
        {
            _definition.IgnoreFailure = true;
            _definition.IgnoreFailureDefault = defaultValue;
            return this;
        }

        /// <summary>
        /// Executes the function and applies the retry logic, if necessary.
        /// </summary>
        /// <returns>The result of the function.</returns>
        /// <remarks>If the function fails in all retries, the last exception or the last result will be outputed.</remarks>
        public TResult Go()
        {
            TResult result = default(TResult);
            Exception exception = null;

            var attempts = 0;
            var retry = false;
            var success = false;

            do
            {
                result = default(TResult);
                exception = null;

                if (retry && _definition.OnBeforeRetry != null)
                {
                    _definition.OnBeforeRetry.Invoke();
                }

                try
                {
                    result = _definition.TheFunction.Invoke();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                if (retry && _definition.OnAfterRetry != null)
                {
                    _definition.OnAfterRetry.Invoke(result, exception);
                }

                retry = _definition.RetryStrategies.Any(s => s.MustRetry(result, exception));

                if(retry)
                {
                    attempts++;

                    if(attempts > _definition.Times)
                    {
                        retry = false;
                    }
                    else
                    {
                        if(_definition.Delay > 0)
                        {
                            Thread.Sleep(_definition.Delay);
                        }
                    }
                }
                else
                {
                    if(exception == null)
                    {
                        success = true;
                    }
                }

            } while (retry);

            if(!success)
            {
                if (_definition.OnFailure != null)
                {
                    _definition.OnFailure.Invoke(result, exception);
                }

                if(_definition.IgnoreFailure)
                {
                    return _definition.IgnoreFailureDefault;
                }

                if (exception != null)
                {
                    throw exception;
                }
            }

            return result;
        }

    }
}
