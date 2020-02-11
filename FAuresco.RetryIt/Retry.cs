using System;

namespace FAuresco.RetryIt
{
    /// <summary>
    /// Allows you to retry code using a fluent API.
    /// </summary>
    public static class Retry
    {

        /// <summary>
        /// The function you want to execute.
        /// </summary>
        /// <typeparam name="TResult">Type of the result of the function.</typeparam>
        /// <param name="func">The function you want to execute.</param>
        /// <returns>A <see cref="RetryCondition{TResult}"/> object that allows you to specify when you want to retry the function.</returns>
        public static RetryDefinitionBuilder<TResult> It<TResult>(Func<TResult> func)
        {
            var builder = new RetryDefinitionBuilder<TResult>(func);
            return builder;
        }

        /// <summary>
        /// The code you want to execute.
        /// </summary>
        /// <param name="action">The code you want to execute.</param>
        /// <returns>A <see cref="RetryCondition{TResult}"/> object that allows you to specify when you want to retry the code.</returns>
        public static RetryDefinitionBuilder<object> It(Action action)
        {
            return It<object>(() =>
            {
                action.Invoke();
                return null;
            });
        }

    }
}
