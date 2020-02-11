# RetryIt
![.NET Core](https://github.com/fauresco/retryit/workflows/.NET%20Core/badge.svg)

Library to effortlessly implement retry logic into your .net applications.

## Installing

Using Nuget:

    Install-Package FAuresco.RetryIt
    
## Using

### Basic Sample

Usually you want to retry your code when something happens for a few times and maybe wait a little bit between retries.

In the example bellow, it will try to fetch data from database, if an exception with the message "timeout" happens, it will retry up to 3 times with a 500ms delay.

```cs
var data = Retry.It(() => SomeRepository.FetchData())
                .WhenExceptionMessageContains("timeout")
                .Times(3)
                .Delay(500)
                .Go();
```

_Note that if all attempts result in "timeout" exception, the exception will be thrown at the last attempt._

### It

"It" is where you put the code you want to retry in case something goes wrong.

You can use it with a function (which does return something):

```cs
var data = Retry.It(() => SomeRepository.FetchData())
                .WhenExceptionMessageContains("timeout")
                .Times(3)
                .Go();
```

It is possible to use actions as well (which does not return value, void):

```cs
Retry.It(() => SomeRepository.SaveData(data))
     .WhenExceptionMessageContains("timeout")
     .Times(3)
     .Go();
```

### When

When something happens, usually an exception, you want to retry. You can use one of the provided "When" methods to set the condition, or use more than one "When" method to set the conditions.

#### WhenExceptionMessageContains

Use this retry strategy when you want to search for a specific message or word in the exception message. If an exception is thrown and the message is found in the exception message, a retry will occur.

_Note that it will also recursively search the inner exceptions._

In the following example, it will try to save some data in the database and if an exception with the word "timeout" occurs, it will retry up to 3 times.

```cs
Retry.It(() => SomeRepository.SaveData(data))
     .WhenExceptionMessageContains("timeout")
     .Times(3)
     .Go();
```

#### WhenExceptionTypeIs

Use this retry strategy when you want to retry if an specifc exception type happens.

In the following example, it will try to save some data in the database and if an exception of type SqlException occurs, it will retry up to 3 times.

```cs
Retry.It(() => SomeRepository.SaveData(data))
     .WhenExceptionTypeIs(typeof(SqlException))
     .Times(3)
     .Go();
```

#### WhenCustom

Use this retry strategy when you want to use a custom provided function to decide whether to retry or not. The inputs of this function are the result of the function and the exception (in case any was thrown), the output must be a boolean, where **true means retry**.

In the following example, it will try to calculate something, but if the result is 50, it will retry up to 10 times. (I know, this a silly example!)

```cs
var number = Retry.It(() => service.Calculate(parameters))
                  .WhenCustom((r,e) => r == 50)
                  .Times(10)
                  .Go();
```

_Note: This is specially useful when you want to call a REST service and examine the returned data to decide if you need to retry it or not._

#### WhenCustomStrategy

This retry strategy allows you to extend the library by providing your own implementation of _IRetryStrategy_ interface. You will create a class that implements this interface and it's method _MustRetry_ will be called with the function result and exception (in case any was thrown), this function will return true or false where **true means retry**._

In the following example, it will try to calculate something and the result of this calculation will be analysed by our custom strategy, if it returns true, it will retry for up to 3 times.

```cs
var number = Retry.It(() => service.Calculate(parameters))
                  .WhenCustomStrategy(new MyCustomStrategy<int>())
                  .Times(3)
                  .Go();
```

#### Using multiple conditions

You can use more than one "When" method, if any of the contitions is true, it will retry.

In the example bellow, it will try to save some data and retry if an error "timeout" or "deadlock" occurs.

```cs
Retry.It(() => SomeRepository.SaveData(data))
     .WhenExceptionMessageContains("timeout")
     .WhenExceptionMessageContains("was deadlocked")
     .Times(3)
     .Go();
```

### Times

How many times you want to retry?

In the following example, it will try to save some data in the database and if an exception of type SqlException occurs, it will retry up to 10 times, after that, it will throw the exception.

```cs
try
{
    Retry.It(() => SomeRepository.SaveData(data))
         .WhenExceptionTypeIs(typeof(SqlException))
         .Times(10)
         .Go();

    Log.Info("Ok good!!!");
} 
catch(SqlException ex)
{
    Log.Error("I have tried for 10 times but it did not work!", ex);
}
```

### Delay

Allows you to specify a delay between each attempt.

In the following example, it will try to fetch information from a not so reliable REST service, if it gets a HTTP 503 (service unavailable) error, it will retry up to 10 times with delay of 1 minute between each attempt.

```cs
var httpResponse = Retry.It(() => MyRestClient.FetchData())
                        .WhenCustom((r,e) => r.StatusCode == 503)
                        .Times(10)
                        .Delay(60 * 1000)
                        .Go();

if(httpResponse.StatusCode == 503)
{
    Log.Error("We have tried to call the service 10 times, but it was offline!");
    return;
}
```

### Go

Well, it goes.