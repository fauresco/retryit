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
Retry.It(data = SomeRepository.FetchData())
     .WhenExceptionMessageContains("timeout")
     .Times(3)
     .Go();
```
