using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace IntegrationTests.Planning
{
    public partial class ObjectToAsserts
    {
        public static void Generate(Dictionary<string, object> comparate1, Dictionary<string, object> comparate2, Dictionary<string, string> specialValues, string name, List<string> fileContentsToWrite = null, bool isRecurse = false)
        {
            if (fileContentsToWrite == null)
            {
                fileContentsToWrite = new List<string>();
            }

            foreach (KeyValuePair<string, object> entry in comparate1)
            {

                if (entry.Value != null)
                {
                    string val1 = entry.Value.ToString();
                    string val2 = comparate2[entry.Key].ToString();
                    if (val1 != "")
                    {
                        Guid guidOutput; //special procedure for GUIDS
                        if (Guid.TryParse(entry.Value.ToString(), out guidOutput))
                        {
                            //value is a guid, we have to do special things

                            if (specialValues.Keys.Contains(val1))
                            {
                                //GUIDS match an id
                                if (val1 == val2)
                                {
                                    fileContentsToWrite.Add("Assert.That(" + name + "[\"" + entry.Key + "\"].ToString(), Is.EqualTo(" + specialValues[val1] + "));");
                                }
                                else
                                {

                                    throw new Exception("Created GUID matched non-created GUID");
                                }
                            }
                            else if (val1 == val2 || val1 != val2)
                            {
                                //guids are the same, but won't be from machine to machine runs
                                //guids are different, but not null
                                fileContentsToWrite.Add("Assert.That(" + name + "[\"" + entry.Key + "\"].ToString(), Is.Not.Null);");
                            }
                            else if (val1 == "00000000-0000-0000-0000-000000000000")
                            {
                                //guid is a null value
                                if (val2 == "00000000-0000-0000-0000-000000000000")
                                {
                                    //confirmed null value
                                    fileContentsToWrite.Add("Assert.That(" + name + "[\"" + entry.Key + "\"].ToString(), Is.EqualTo(\"00000000-0000-0000-0000-000000000000\"));");
                                }
                                else
                                {

                                    throw new Exception("GUID null vs non-null");
                                }
                            }
                        }
                        else if (val1[0] == '{')
                        {
                            //object, recurse
                            fileContentsToWrite.Add("var " + name + "_" + entry.Key + " = JsonConvert.DeserializeObject<Dictionary<string, object>>(" + name + "[\"" + entry.Key + "\"].ToString());");
                            var comparate1next = JsonConvert.DeserializeObject<Dictionary<string, object>>(comparate1[entry.Key].ToString());

                            var comparate2next = JsonConvert.DeserializeObject<Dictionary<string, object>>(comparate1[entry.Key].ToString()); ;

                            Generate(comparate1next, comparate2next, specialValues, name + "_" + entry.Key, fileContentsToWrite, true);
                        }
                        else if (val1[0] == '[')
                        {
                            //array, recurse for each item
                            fileContentsToWrite.Add("var " + name + "_" + entry.Key + " = JsonConvert.DeserializeObject<JArray>(" + name + "[\"" + entry.Key + "\"].ToString());");
                            var nextArray1 = JsonConvert.DeserializeObject<JArray>(comparate1[entry.Key].ToString());
                            var nextArray2 = JsonConvert.DeserializeObject<JArray>(comparate2[entry.Key].ToString());

                            for (int i = 0; i < nextArray1.Count; i++)
                            {
                                fileContentsToWrite.Add("var " + name + "_" + entry.Key + i + " = JsonConvert.DeserializeObject<Dictionary<string, object>>(" + name + "_" + entry.Key + "[" + i + "].ToString());");

                                if (nextArray1[i].ToString()[0] == '{')
                                {
                                    //list of objects
                                    Generate(
                                        JsonConvert.DeserializeObject<Dictionary<string, object>>(nextArray1[i].ToString()),
                                        JsonConvert.DeserializeObject<Dictionary<string, object>>(nextArray2[i].ToString()),
                                        specialValues,
                                        name + "_" + entry.Key.ToString() + i.ToString(),
                                        fileContentsToWrite,
                                        true
                                        );
                                }
                                else
                                {
                                    //just a list of primitive types

                                    //remove fileContentsToWrite for deserializing array and first obj
                                    fileContentsToWrite.RemoveAt(fileContentsToWrite.Count - 1);
                                    fileContentsToWrite.RemoveAt(fileContentsToWrite.Count - 1);
                                    if (entry.Key == "faults")
                                    {
                                        fileContentsToWrite.Add("Assert.That(JsonConvert.DeserializeObject<JArray>(" + name + "[\"" + entry.Key + "\"].ToString()).Count, Is.EqualTo(" + JsonConvert.DeserializeObject<JArray>(comparate1[entry.Key].ToString()).Count.ToString() + "));");
                                    }
                                    else
                                    {
                                        fileContentsToWrite.Add("Assert.That(Regex.Replace(" + name + "[\"" + entry.Key + "\"].ToString(), @\"\\s+\", \"\"), Is.EqualTo(\"" + Regex.Replace(comparate1[entry.Key].ToString().Replace("\"", "\\\""), @"\s+", "") + "\")); ");
                                    }
                                    //stop loop
                                    i = nextArray1.Count;
                                }
                            }
                        }
                        else if (val1 == val2)
                        {
                            if (entry.Key.ToString().Substring(Math.Max(0, entry.Key.ToString().Length - 2)) != "By" &&
                                entry.Key.ToString().Substring(Math.Max(0, entry.Key.ToString().Length - 2)) != "On" &&
                                entry.Key.ToString().Substring(Math.Max(0, entry.Key.ToString().Length - 3)) != "Dtg" &&
                                entry.Key.ToString().Substring(Math.Max(0, entry.Key.ToString().Length - 4)) != "Time" &&
                                entry.Key.ToString().Substring(Math.Max(0, entry.Key.ToString().Length - 8)) != "Modified")
                            {
                                fileContentsToWrite.Add("Assert.That(" + name + "[\"" + entry.Key + "\"].ToString(), Is.EqualTo(\"" + val1.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t") + "\"));");
                            }
                            else
                            {
                                //catch values that are user names
                                fileContentsToWrite.Add("Assert.That(" + name + "[\"" + entry.Key + "\"].ToString(), Is.Not.Null);");
                            }
                        }
                        else if (val1 != val2)
                        {
                            //doesnt match so just make it not null
                            fileContentsToWrite.Add("Assert.That(" + name + "[\"" + entry.Key + "\"].ToString(), Is.Not.Null);");
                        }
                    }
                    else
                    {
                        //empty string
                        if (val2.ToString() == "")
                        {
                            fileContentsToWrite.Add("Assert.That(" + name + "[\"" + entry.Key + "\"].ToString(), Is.EqualTo(\"\"));");
                        }
                        else
                        {
                            throw new Exception("Empty vs non empty");
                        }
                    }
                }
                else
                {
                    if (comparate2[entry.Key] == null)
                    {
                        //null
                        fileContentsToWrite.Add("Assert.That(" + name + "[\"" + entry.Key + "\"], Is.Null);");
                    }
                    else
                    {
                        throw new Exception("null vs non null");
                    }
                }
            }
            if (!isRecurse)
            {
                var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ObjectToAsserts/GeneratedAsserts/Asserts.txt");
                File.WriteAllfileContentsToWrite(path, fileContentsToWrite);
            }
        }

        public static void GenerateArrays(JArray array1, JArray array2, string name, Dictionary<string, string> specialValues = null)
        {
            Generate(array1, array2, name, specialValues);
        }

        public static void Generate(JArray array1, JArray array2, string name, Dictionary<string, string> specialValues = null)
        {
            if (specialValues == null)
            {
                specialValues = new Dictionary<string, string>();
            }
            List<string> fileContentsToWrite = new List<string>();
            for (int i = 0; i < array1.Count; i++)
            {
                fileContentsToWrite.Add("var " + name + i + " = JsonConvert.DeserializeObject<Dictionary<string, object>>(" + name + "[" + i + "].ToString());");

                //list of objects
                Generate(
                    JsonConvert.DeserializeObject<Dictionary<string, object>>(array1[i].ToString()),
                    JsonConvert.DeserializeObject<Dictionary<string, object>>(array2[i].ToString()),
                    specialValues,
                    name + i.ToString(),
                    fileContentsToWrite,
                    i == array1.Count //only write fileContentsToWrite on last iteration
                    );
            }
        }
    }
}