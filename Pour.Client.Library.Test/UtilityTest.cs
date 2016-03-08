using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Pour.Client.Library.Test
{
    [TestClass]
    public class UtilityTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetRepresentation_RequireNotNull_Null()
        {
            Utility.RequireNotNull(null, "some name");
        }

        [TestMethod]
        public void GetRepresentation_RequireNotNull_NotNull()
        {
            string.Empty.RequireNotNull("some name");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetRepresentation_RequireNonEmpty_Null()
        {
            Utility.RequireNonEmpty(null, "some name");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetRepresentation_RequireNonEmpty_Empty()
        {
            string.Empty.RequireNonEmpty("some name");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetRepresentation_RequireNonEmpty_SingleWhiteSpace()
        {
            " ".RequireNonEmpty("some name");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetRepresentation_RequireNonEmpty_MultipleWhiteSpace()
        {
            "  ".RequireNonEmpty("some name");
        }

        [TestMethod]
        public void GetRepresentation_RequireNonEmpty_WhiteSpace()
        {
            "some name with whitepace".RequireNonEmpty("some name");
        }

        [TestMethod]
        public void GetRepresentation_GetContextKey_Null()
        {
            Assert.AreEqual(Utility.GetContextName(null), string.Empty);
        }

        [TestMethod]
        public void GetRepresentation_GetContextKey_Empty()
        {
            Assert.AreEqual(Utility.GetContextName(string.Empty), string.Empty);
        }

        [TestMethod]
        public void GetRepresentation_GetContextKey_WhiteSpace()
        {
            Assert.AreEqual(Utility.GetContextName(" "), string.Empty);
        }

        [TestMethod]
        public void GetRepresentation_GetContextKey_SomeValueWithoutWhiteSpace()
        {
            Assert.AreEqual(Utility.GetContextName("SomeValue"), "SomeValue");
        }

        [TestMethod]
        public void GetRepresentation_GetContextKey_SomeValueWithWhiteSpace()
        {
            Assert.AreEqual(Utility.GetContextName("Some Value"), "SomeValue");
        }

        [TestMethod]
        public void GetRepresentation_GetContextKey_SomeValueWithWhiteSpaceBeginAndEnd()
        {
            Assert.AreEqual(Utility.GetContextName(" Some Value "), "SomeValue");
        }

        [TestMethod]
        public void GetRepresentation_Bool_True()
        {
            string name = "some name";
            bool value = true;
            string json = Utility.GetJsonRepresentation(name, value);
            Validate(json);
        }

        [TestMethod]
        public void GetRepresentation_Bool_False()
        {
            string name = "some name";
            bool value = false;
            string json = Utility.GetJsonRepresentation(name, value);
            Validate(json);
        }

        [TestMethod]
        public void GetRepresentation_Int()
        {
            string name = "some name";
            int value = new Random().Next(int.MinValue, int.MaxValue);
            string json = Utility.GetJsonRepresentation(name, value);
            Validate(json);
        }

        [TestMethod]
        public void GetRepresentation_Double()
        {
            string name = "some name";
            double value = new Random().NextDouble();
            string json = Utility.GetJsonRepresentation(name, value);
            Validate(json);
        }

        [TestMethod]
        public void GetRepresentation_DateTime()
        {
            string name = "some name";
            DateTime value = DateTime.UtcNow;
            string json = Utility.GetJsonRepresentation(name, value);
            Validate(json);
        }

        [TestMethod]
        public void GetRepresentation_Guid()
        {
            string name = "some name";
            Guid value = Guid.NewGuid();
            string json = Utility.GetJsonRepresentation(name, value);
            Validate(json);
        }

        [TestMethod]
        public void GetRepresentation_String()
        {
            string name = "some name";
            string value = "some value";
            string json = Utility.GetJsonRepresentation(name, value);
            Validate(json);
        }

        private static void Validate(string json)
        {
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Length > 0);
        }
    }
}
