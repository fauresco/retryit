using FAuresco.RetryIt.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Diagnostics;

namespace FAuresco.RetryIt.Tests
{
    [TestClass]
    public class RetryTests
    {
        [TestMethod]
        public void Retry_executes_function()
        {
            var service = Mock.Of<ISomeService>();
            Mock.Get(service).Setup(s => s.CalculateSomething()).Returns(100);

            var result = Retry.It(() => service.CalculateSomething()).Go();

            Assert.AreEqual(100, result);
        }

        [TestMethod]
        public void Retry_executes_action()
        {
            var repo = Mock.Of<ISomeDataRepository>();
            var data = new SomeData() { Id = 10, Name = "Fernando" };

            Retry.It(() => repo.SaveSomeData(data)).Go();

            Mock.Get(repo).Verify(r => r.SaveSomeData(It.Is<SomeData>(d => d.Id.Equals(10) && d.Name.Equals("Fernando"))));
        }

        [TestMethod]
        public void Retry_retry_N_times()
        {
            var repo = Mock.Of<ISomeDataRepository>();
            var data = new SomeData() { Id = 10, Name = "Fernando" };

            var cnt = 0;
            Mock.Get(repo).Setup(r => r.SaveSomeData(It.IsAny<SomeData>())).Callback(() => {
                cnt++;
                if(cnt < 3)
                {
                    throw new ApplicationException("Error saving data due to timeout!");
                }
            });

            Retry.It(() => repo.SaveSomeData(data))
                 .WhenExceptionMessageContains("timeout")
                 .Times(3)
                 .Go();

            Mock.Get(repo).Verify(r => r.SaveSomeData(It.Is<SomeData>(d => d.Id.Equals(10) && d.Name.Equals("Fernando"))));
            Assert.AreEqual(3, cnt);
        }

        [TestMethod]
        public void Retry_delay_between_retries()
        {
            var repo = Mock.Of<ISomeDataRepository>();
            var data = new SomeData() { Id = 10, Name = "Fernando" };

            var cnt = 0;
            Mock.Get(repo).Setup(r => r.SaveSomeData(It.IsAny<SomeData>())).Callback(() => {
                cnt++;
                if (cnt < 3)
                {
                    throw new ApplicationException("Error saving data due to timeout!");
                }
            });

            var sw = new Stopwatch();
            sw.Start();

            Retry.It(() => repo.SaveSomeData(data))
                 .WhenExceptionMessageContains("timeout")
                 .Times(3)
                 .Delay(100)
                 .Go();

            sw.Stop();

            Mock.Get(repo).Verify(r => r.SaveSomeData(It.Is<SomeData>(d => d.Id.Equals(10) && d.Name.Equals("Fernando"))));
            Assert.AreEqual(3, cnt);

            // because the delay between retries is 100ms and 2 calls will fail, it should take at least 200ms to execute
            Assert.IsTrue(sw.ElapsedMilliseconds >= 200); 
            Assert.IsTrue(sw.ElapsedMilliseconds < 300);
        }

        [TestMethod]
        public void Retry_retry_when_exception_message_contains()
        {
            var repo = Mock.Of<ISomeDataRepository>();
            var data = new SomeData() { Id = 10, Name = "Fernando" };

            var cnt = 0;
            Mock.Get(repo).Setup(r => r.SaveSomeData(It.IsAny<SomeData>())).Callback(() => {
                cnt++;
                if (cnt < 3)
                {
                    throw new ApplicationException("Error saving data due to timeout!");
                }
            });

            Retry.It(() => repo.SaveSomeData(data))
                 .WhenExceptionMessageContains("timeout")
                 .Times(3)
                 .Go();

            Mock.Get(repo).Verify(r => r.SaveSomeData(It.Is<SomeData>(d => d.Id.Equals(10) && d.Name.Equals("Fernando"))));
            Assert.AreEqual(3, cnt);
        }

        [TestMethod]
        public void Retry_retry_when_exception_type_is()
        {
            var repo = Mock.Of<ISomeDataRepository>();
            var data = new SomeData() { Id = 10, Name = "Fernando" };

            var cnt = 0;
            Mock.Get(repo).Setup(r => r.SaveSomeData(It.IsAny<SomeData>())).Callback(() => {
                cnt++;
                if (cnt < 3)
                {
                    throw new ArgumentNullException("teste");
                }
            });

            Retry.It(() => repo.SaveSomeData(data))
                 .WhenExceptionTypeIs(typeof(ArgumentNullException))
                 .Times(3)
                 .Go();

            Mock.Get(repo).Verify(r => r.SaveSomeData(It.Is<SomeData>(d => d.Id.Equals(10) && d.Name.Equals("Fernando"))));
            Assert.AreEqual(3, cnt);
        }

        [TestMethod]
        public void Retry_retry_when_custom_function_returns_true()
        {
            var service = Mock.Of<ISomeService>();

            var cnt = 0;

            Mock.Get(service).Setup(r => r.CalculateSomething()).Returns(() => {
                cnt++;

                if (cnt == 1)
                {
                    return 50;
                }

                return 100;
            });

            int resultado = 0;

            resultado = Retry.It(() => service.CalculateSomething())
                             .WhenCustom((r, e) => r == 50)
                             .Times(10)
                             .Go();

            Assert.AreEqual(2, cnt);
            Assert.AreEqual(100, resultado);
        }

        [TestMethod]
        public void Retry_retry_when_custom_strategy_returns_true()
        {
            var service = Mock.Of<ISomeService>();

            var cnt = 0;

            Mock.Get(service).Setup(r => r.CalculateSomething()).Returns(() => {
                cnt++;

                if (cnt == 1)
                {
                    return 3;
                }

                return 10;
            });

            int resultado = 0;

            resultado = Retry.It(() => service.CalculateSomething())
                             .WhenCustomStrategy(new EvenNumberCustomRetryStrategy<int>()) // retry if the result of CalculateSomething is not even
                             .Times(10)
                             .Go();

            Assert.AreEqual(2, cnt);
            Assert.AreEqual(10, resultado);
        }

        [TestMethod]
        public void Retry_retry_when_any_condition_is_true()
        {
            var service = Mock.Of<ISomeService>();

            var cnt = 0;

            Mock.Get(service).Setup(r => r.CalculateSomething()).Returns(() => {
                cnt++;

                if (cnt == 1)
                {
                    throw new ArgumentNullException("teste");
                }
                else if (cnt == 2)
                {
                    throw new ApplicationException("Isso é um teste!");
                }
                else if (cnt == 3)
                {
                    return 50;
                }

                return 100;
            });

            int resultado = 0;

            resultado = Retry.It(() => service.CalculateSomething())
                             .WhenExceptionTypeIs(typeof(ArgumentNullException))
                             .WhenExceptionMessageContains("teste")
                             .WhenCustom((r,e) => r == 50)
                             .Times(4)
                             .Go();

            Assert.AreEqual(4, cnt);
            Assert.AreEqual(100, resultado);
        }

        [TestMethod]
        public void Retry_throw_original_exception_when_reaches_max_retries()
        {
            var repo = Mock.Of<ISomeDataRepository>();
            var data = new SomeData() { Id = 10, Name = "Fernando" };

            var cnt = 0;
            Mock.Get(repo).Setup(r => r.SaveSomeData(It.IsAny<SomeData>())).Callback(() => {
                cnt++;
                throw new ApplicationException("Error saving data due to timeout!");
            });

            try
            {
                Retry.It(() => repo.SaveSomeData(data))
                     .WhenExceptionMessageContains("timeout")
                     .Times(3)
                     .Go();

                Assert.Fail("Did not throw exception!");
            }
            catch(ApplicationException ex)
            {
                Assert.AreEqual("Error saving data due to timeout!", ex.Message);
                Assert.AreEqual(4, cnt); // 1 + 3 retries
            }
        }

        [TestMethod]
        public void Retry_throws_exception()
        {
            var repo = Mock.Of<ISomeDataRepository>();
            var data = new SomeData() { Id = 10, Name = "Fernando" };

            Mock.Get(repo).Setup(r => r.SaveSomeData(It.IsAny<SomeData>())).Callback(() => {
                throw new ApplicationException("Shit!");
            });

            try
            {
                Retry.It(() => repo.SaveSomeData(data))
                     .WhenExceptionMessageContains("timeout") // exception is "Shit!", will not retry
                     .Times(3)
                     .Go();

                Assert.Fail("Did not throw exception!");
            }
            catch (ApplicationException ex)
            {
                Assert.AreEqual("Shit!", ex.Message);
            }
        }

        [TestMethod]
        public void Retry_executes_action_when_all_attempts_fail()
        {
            var repo = Mock.Of<ISomeDataRepository>();
            var data = new SomeData() { Id = 10, Name = "Fernando" };

            var cnt = 0;
            Mock.Get(repo).Setup(r => r.SaveSomeData(It.IsAny<SomeData>())).Callback(() => {
                cnt++;
                throw new ApplicationException("Error saving data due to timeout!");
            });

            string log = "";

            try
            {
                Retry.It(() => repo.SaveSomeData(data))
                     .WhenExceptionMessageContains("timeout")
                     .Times(3)
                     .OnFailure((r,e) => { log = "Can't save this s***!"; })
                     .Go();

                Assert.Fail("Did not throw exception!");
            }
            catch (ApplicationException ex)
            {
                Assert.AreEqual("Error saving data due to timeout!", ex.Message);
                Assert.AreEqual(4, cnt); // 1 + 3 retries
                Assert.AreEqual("Can't save this s***!", log);
            }
        }

        [TestMethod]
        public void Retry_executes_action_before_each_retry()
        {
            var repo = Mock.Of<ISomeDataRepository>();
            var data = new SomeData() { Id = 10, Name = "Fernando" };

            var cnt = 0;
            Mock.Get(repo).Setup(r => r.SaveSomeData(It.IsAny<SomeData>())).Callback(() => {
                cnt++;
                if (cnt < 3)
                {
                    throw new ApplicationException("Error saving data due to timeout!");
                }
            });

            int beforeCount = 0;

            Retry.It(() => repo.SaveSomeData(data))
                 .WhenExceptionMessageContains("timeout")
                 .Times(3)
                 .BeforeRetry(() => { beforeCount++; })
                 .Go();

            Mock.Get(repo).Verify(r => r.SaveSomeData(It.Is<SomeData>(d => d.Id.Equals(10) && d.Name.Equals("Fernando"))));
            Assert.AreEqual(3, cnt);
            Assert.AreEqual(2, beforeCount);
        }

        [TestMethod]
        public void Retry_executes_action_after_each_retry()
        {
            var repo = Mock.Of<ISomeDataRepository>();
            var data = new SomeData() { Id = 10, Name = "Fernando" };

            var cnt = 0;
            Mock.Get(repo).Setup(r => r.SaveSomeData(It.IsAny<SomeData>())).Callback(() => {
                cnt++;
                if (cnt < 3)
                {
                    throw new ApplicationException("Error saving data due to timeout!");
                }
            });

            int afterCount = 0;

            Retry.It(() => repo.SaveSomeData(data))
                 .WhenExceptionMessageContains("timeout")
                 .Times(3)
                 .AfterRetry((r,e) => { afterCount++; })
                 .Go();

            Mock.Get(repo).Verify(r => r.SaveSomeData(It.Is<SomeData>(d => d.Id.Equals(10) && d.Name.Equals("Fernando"))));
            Assert.AreEqual(3, cnt);
            Assert.AreEqual(2, afterCount);
        }

        [TestMethod]
        public void Retry_returns_default_value()
        {
            var service = Mock.Of<ISomeService>();

            var cnt = 0;

            Mock.Get(service).Setup(r => r.CalculateSomething()).Returns(() => {
                cnt++;
                throw new ApplicationException("Isso é um teste!");
            });

            int resultado = -10;

            resultado = Retry.It(() => service.CalculateSomething())
                             .WhenExceptionMessageContains("teste")
                             .Times(3)
                             .IgnoreFailure()
                             .Go();

            Assert.AreEqual(4, cnt);
            Assert.AreEqual(0, resultado);
        }

        [TestMethod]
        public void Retry_returns_specified_default_value()
        {
            var service = Mock.Of<ISomeService>();

            var cnt = 0;

            Mock.Get(service).Setup(r => r.CalculateSomething()).Returns(() => {
                cnt++;
                throw new ApplicationException("Isso é um teste!");
            });

            int resultado = 0;

            resultado = Retry.It(() => service.CalculateSomething())
                             .WhenExceptionMessageContains("teste")
                             .Times(3)
                             .IgnoreFailure(33)
                             .Go();

            Assert.AreEqual(4, cnt);
            Assert.AreEqual(33, resultado);
        }

    }
}
