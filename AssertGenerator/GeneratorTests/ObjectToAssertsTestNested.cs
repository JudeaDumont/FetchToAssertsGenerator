using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Tests.Common;

using static IntegrationTests.Planning.HelperFunctions;

namespace IntegrationTests.Planning
{
    [TestFixture]
    public partial class ObjectToAsserts
    {
        [Test(Description = "Test the assert generation for properties deeply nested (5+ objects) in result object structure.")]
        [Category("Integration")]
        public async Task GeneratorTestDeeplyNestedObjects()
        {
            var reqAndOp = await getSingletonRequestA(); //returns a tuple
            var requestId = reqAndOp.Item1;

            var bigListExample1 = await FetchToAssertsHttpClient.Get<Dictionary<string, object>>("/Request/" + requestId + "/definebigListExample?", "/nms");

            var bigListExample2 = await FetchToAssertsHttpClient.Get<Dictionary<string, object>>("/Request/" + requestId + "/definebigListExample?", "/nms");

            Generate(bigListExample1, bigListExample2, new Dictionary<string, string>() {
                { requestId, "changeId" },
            }, "bigListExample");
        }
    }
}