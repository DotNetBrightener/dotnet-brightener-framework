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
        }
    }

    class SomeObject
    {
        public string TestValue { get; set; }

        public string TestValue2 { get; set; }

        public string TestValue3 { get; set; }
    }
}