namespace SignalXLib.TestHelperLib
{
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class SignalXAssertionLib
    {
        public void AreNotEqual<T>(T item1, object item2, string message = "")
        {
            if (item1.Equals(Convert.ChangeType(item2, typeof(T))))
                throw new Exception($"Expected {item1} to be not equal to {item2} " + message);
        }

        public void AreEqual<T>(T item1, object item2, string message = "")
        {
            if (!item1.Equals(Convert.ChangeType(item2, typeof(T))))
                throw new Exception($"Expected {item1} to be equal to {item2} " + message);
        }

        public void AreTheSame(object item1, object item2, string message = "")
        {
            string obj1 = JsonConvert.SerializeObject(item1);
            string obj2 = JsonConvert.SerializeObject(item2);
            if (obj1 != obj2)
                throw new Exception($"Expected {obj1} to be equal to {obj2} " + message);
        }

        public async Task<bool> WillPassBeforeGivenTime(TimeSpan timeSpan, Action operation)
        {
            var startTime = DateTime.Now;
            Exception lastException = null;
            do
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    operation();
                    lastException = null;
                }
                catch (Exception e)
                {
                    lastException = e;
                }
            }
            while ((DateTime.Now - startTime).TotalMilliseconds< timeSpan.TotalMilliseconds 
            && lastException!=null);

            if (lastException != null)
            {
                //$"Could not pass within given time {timeSpan}"
                throw lastException;
            }

            return true;
        }
        public void AreDifferent(object item1, object item2, string message = "")
        {
            string obj1 = JsonConvert.SerializeObject(item1);
            string obj2 = JsonConvert.SerializeObject(item2);
            if (obj1 == obj2)
                throw new Exception($"Expected {obj1} to be equal to {obj2} " + message);
        }

        public void IsTrue(bool expectation, string message = "")
        {
            if (!expectation)
                throw new Exception("Expected result to be true " + message);
        }

        public void IsFalse(bool expectation, string message = "")
        {
            if (expectation)
                throw new Exception("Expected result to be not true " + message);
        }

        public void Fail()
        {
            throw new Exception("This line was not expected to be reached");
        }

        public void WaitForSomeTime(TimeSpan fromSeconds)
        {
            DateTime start = DateTime.Now;
            while ((DateTime.Now - start).TotalMilliseconds < fromSeconds.TotalMilliseconds)
                Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
        }
    }
}