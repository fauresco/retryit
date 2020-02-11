# RetryIt
![.NET Core](https://github.com/fauresco/retryit/workflows/.NET%20Core/badge.svg)

Library to effortlessly implement retry logic into your .net applications.

## Installing

Using Nuget:

    Install-Package FAuresco.RetryIt
    
## Using

### Basic Sample

Usually you want to retry your code when something happens for a few times:

```cs
var data = Retry.It(() => SomeRepository.FetchData())
                .WhenExceptionMessageContains("timeout")
                .Times(3)
                .Go();
```

### It()

"It" is where you put the code you want to retry in case something goes wrong.

You can use it with a function (which does return something):

```cs
var data = Retry.It(() => SomeRepository.FetchData())
                .WhenExceptionMessageContains("timeout")
                .Times(3)
                .Go();
```

It is possible to use it with actions as well (which does not return value, void):

```cs
Retry.It(() => SomeRepository.SaveData(data))
     .WhenExceptionMessageContains("timeout")
     .Times(3)
     .Go();
```