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
        [Test(Description = "Test the assert generation for properties nested in result object structures that contain nested arrays of objects.")]
        [Category("Integration")]
        public async Task GeneratorTestNestedObjectsWithArrays()
        {
            var reqAndOp = await getSingletonRequestB();
            var requestId = reqAndOp.Item1;

            var bigListExample1 = await FetchToAssertsHttpClient.Get<Dictionary<string, object>>("/Request/" + requestId + "/definebigListExample?", "/nms");

            var bigListExample2 = await FetchToAssertsHttpClient.Get<Dictionary<string, object>>("/Request/" + requestId + "/definebigListExample?", "/nms");

            Generate(bigListExample1, bigListExample2, new Dictionary<string, string>() {
                { terminalId, "terminalId" },
                { requestId, "requestId" },
            }, "response");
        }
    }
}