using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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
        [Test(Description = "Test the assert generation for properties simple object structures.")]
        [Category("Integration")]
        public async Task GeneratorTestSimpleObjectStructureAsync()
        {
            Generate(
                new Dictionary<string, object>() { { "f", "{\"test\":\"test\"}" } },
                new Dictionary<string, object>() { { "f", "{\"test\":\"test\"}" } },
                new Dictionary<string, string>(), "response");

            

            var templateRequest1 = await FetchToAssertsHttpClient.Get<Dictionary<string, object>>(
                "api/Request/Template?RequestType=FDMA","/nms");

            var templateRequest2 = await FetchToAssertsHttpClient.Get<Dictionary<string, object>>(
                "api/Request/Template?RequestType=FDMA", "/nms");


            Generate(
                templateRequest1,
                templateRequest2,
                new Dictionary<string, string>(), "templateRequest");
        }

    }
}