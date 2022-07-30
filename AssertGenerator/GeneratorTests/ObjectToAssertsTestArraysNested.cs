using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tests.Common;

namespace IntegrationTests.Planning
{
    [TestFixture]
    public partial class ObjectToAsserts
    {
        [Test(Description = "Test the assert generation for properties nested in result object structure.")]
        [Category("Integration")]
        public async Task GeneratorTestArraysWithNestedObjects()
        {
            var example1 = await FetchToAssertsHttpClient.Get<JObject>("/Request/example?examplequery=");

            var jTokenExample1 = example1.SelectToken("jTokenExample");
            var jTokenExampleList1 = JsonConvert.DeserializeObject<JArray>(jTokenExample1.ToString());


            var example2 = await FetchToAssertsHttpClient.Get<JObject>("/Request/example?examplequery=");

            var jTokenExample2 = example2.SelectToken("jTokenExample");
            var jTokenExampleList2 = JsonConvert.DeserializeObject<JArray>(jTokenExample2.ToString());

            Generate(jTokenExampleList1, jTokenExampleList1, "jTokenExampleList");

            var examplex1 = await FetchToAssertsHttpClient.Get<Dictionary<string, object>>("/Request/example?examplequery=");

            var examplex2 = await FetchToAssertsHttpClient.Get<Dictionary<string, object>>("/Request/example?examplequery=");

            Generate(examplex1, examplex2, new Dictionary<string, string>(), "jTokenExampleList");
        }
    }
}