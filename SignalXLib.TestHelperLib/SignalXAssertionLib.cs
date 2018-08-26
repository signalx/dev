namespace SignalXLib.TestHelperLib
{
    using Newtonsoft.Json;
    using System;

    public class SignalXAssertionLib
    {
        public void NotEqual<T>(T item1, T item2, string message = "")
        {
            if (item1.Equals(item2))
                throw new Exception($"Expected {item1} to be not equal to {item2} " + message);
        }

        public void Equal<T>(T item1, T item2, string message = "")
        {
            if (!item1.Equals(item2))
                throw new Exception($"Expected {item1} to be equal to {item2} " + message);
        }

        public void TheSame(object item1, object item2, string message = "")
        {
            var obj1 = JsonConvert.SerializeObject(item1);
            var obj2 = JsonConvert.SerializeObject(item2);
            if (obj1 != obj2)
                throw new Exception($"Expected {obj1} to be equal to {obj2} " + message);
        }

        public void Different(object item1, object item2, string message = "")
        {
            var obj1 = JsonConvert.SerializeObject(item1);
            var obj2 = JsonConvert.SerializeObject(item2);
            if (obj1 == obj2)
                throw new Exception($"Expected {obj1} to be equal to {obj2} " + message);
        }

        public void IsTrue(bool expectation, string message = "")
        {
            if (!expectation)
                throw new Exception($"Expected result to be true " + message);
        }

        public void IsFalse(bool expectation, string message = "")
        {
            if (expectation)
                throw new Exception($"Expected result to be not true " + message);
        }
    }
}