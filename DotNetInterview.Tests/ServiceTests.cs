using System;
using System.Reflection;
using NUnit.Framework;
using DotNetInterview.API.Service;
using DotNetInterview.API.Domain;

namespace DotNetInterview.Tests.C_ServiceTests
// unit tests for the item service and pricing class
{

    [TestFixture]
    public class MyClassTests
    {
        [Test]
        public void TestTimeBaseRuleforNotMonday()
        {
            // Arrange
            var pricingClassType = typeof(Pricing);
            var methodInfo = pricingClassType.GetMethod("TimeRule", BindingFlags.NonPublic | BindingFlags.Static);
            // Saturday 29th of March 2025 
            DateTime datetime = new DateTime(2025, 3, 29, 15, 30, 0);
            decimal price = 100.00m;
            // Act
            var result = methodInfo.Invoke(null, new object[] { price, datetime });

            // Assert the result
            Assert.AreEqual(100.00m,result);
        }
        [Test]
        public void TestTimeBaseRuleforMonday_notDiscountHours()
        {
            // Arrange
            var pricingClassType = typeof(Pricing);

            var methodInfo = pricingClassType.GetMethod("TimeRule", BindingFlags.NonPublic | BindingFlags.Static);

            // Monday 24th of March 2025 8:30am
            DateTime datetime = new DateTime(2025, 3, 24, 8, 30, 0);
            decimal price = 100.00m;

            // Act

            var result = methodInfo.Invoke(null, new object[] { price, datetime });

            // Assert the result
            Assert.AreEqual(100.00m,result);
        }
        [Test]
        public void TestTimeBaseRuleforMonday_atDiscountHours()
        {
            // Arrange
            var pricingClassType = typeof(Pricing);
            var methodInfo = pricingClassType.GetMethod("TimeRule", BindingFlags.NonPublic | BindingFlags.Static);

            // Monday 24th of March 2025 12:30am
            DateTime datetime = new DateTime(2025, 3, 24, 12, 30, 0);
            decimal price = 100.00m;
            // Act
            var result = methodInfo.Invoke(null, new object[] { price, datetime });

            // Assert the result
            Assert.AreEqual(50.00m, result);
        }
        [Test]
        public void TestQuantityRule_1Item()
        {
            // Arrange
            var pricingClassType = typeof(Pricing);
            var methodInfo = pricingClassType.GetMethod("QuantityRule", BindingFlags.NonPublic | BindingFlags.Static);

            int quantity = 1;
            decimal price = 100.00m;
            // Act
            var result = methodInfo.Invoke(null, new object[] { price, quantity });

            // Assert the result
            Assert.AreEqual(100.00m,result);
        }
        [Test]
        public void TestQuantityRule_6Items()
        {
            // Arrange
            var pricingClassType = typeof(Pricing);
            var methodInfo = pricingClassType.GetMethod("QuantityRule", BindingFlags.NonPublic | BindingFlags.Static);

            int quantity = 6;
            decimal price = 100.00m;
            // Act
            var result = methodInfo.Invoke(null, new object[] { price, quantity });

            // Assert the result
            Assert.AreEqual(90.00m,result);
        }
        public void TestQuantityRule_11Items()
        {
            // Arrange
            var pricingClassType = typeof(Pricing);
            var methodInfo = pricingClassType.GetMethod("QuantityRule", BindingFlags.NonPublic | BindingFlags.Static);

            int quantity = 11;
            decimal price = 100.00m;
            // Act
            var result = methodInfo.Invoke(null, new object[] { price, quantity });

            // Assert the result
            Assert.AreEqual(80.00m,result);
        }
    }
}