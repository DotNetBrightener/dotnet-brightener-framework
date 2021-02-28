using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace DotNetBrightener.CMS.Tests
{
    public class CacheKeyFromExpressionTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestExpressionToCacheKey()
        {
            var valueTest = "ValueTest";

            Expression<Func<SomeObject, bool>> expression = _ => _.TestValue == valueTest;

            string expectedCacheKey = "SomeObject_[TestValue==ValueTest]";

            string resultCacheKey = expression.GenerateCacheKey();

            Assert.AreEqual(expectedCacheKey, resultCacheKey);

            expression = _ => _.TestValue == valueTest && _.TestValue2 != "SecondCondition" || _.TestValue3 == null;

            expectedCacheKey = "SomeObject_[[[TestValue==ValueTest]&&[TestValue2!=SecondCondition]]||[TestValue3==null]]";

            resultCacheKey = expression.GenerateCacheKey();

            Assert.AreEqual(expectedCacheKey, resultCacheKey);

            var user = new User
            {
                UserId = 10
            };

            expression = _ => _.Id == user.UserId;

            expectedCacheKey = "SomeObject_[Id==10]";

            resultCacheKey = expression.GenerateCacheKey();

            Assert.AreEqual(expectedCacheKey, resultCacheKey);
        }
    }

    class User
    {
        public long UserId { get; set; }
    }

    class SomeObject
    {
        public long Id { get; set; }

        public string TestValue { get; set; }

        public string TestValue2 { get; set; }

        public string TestValue3 { get; set; }
    }
}