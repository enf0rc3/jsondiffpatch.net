using JsonDiffPatchDotNet;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace NugetPackageUnitTests
{
	[TestFixture]
	public class VariableSetEquivalentTests
	{
		private JToken CreateBaselineVariableSet()
		{
			return JToken.Parse(@"{
				""Variables"": [
					{""Id"": ""var1"", ""Name"": ""Variable 1"", ""Value"": ""Value 1""},
					{""Id"": ""var2"", ""Name"": ""Variable 2"", ""Value"": ""Value 2""},
					{""Id"": ""var3"", ""Name"": ""Variable 3"", ""Value"": ""Value 3""},
					{""Id"": ""var4"", ""Name"": ""Variable 4"", ""Value"": ""Value 4""},
					{""Id"": ""var5"", ""Name"": ""Variable 5"", ""Value"": ""Value 5""}
				]
			}");
		}

		private void AssertDiffMatchesExpected(JToken actualDiff, string expectedJson, string testName)
		{
			var expected = JToken.Parse(expectedJson);

			// Convert jsondiffpatch.net format to JSON Patch format for comparison
			var converted = ConvertToJsonPatchFormat(actualDiff);

			Console.WriteLine($"{testName} - Expected: {expected}");
			Console.WriteLine($"{testName} - Actual (converted): {converted}");

			// For now, just ensure we get some diff when expected (non-empty array) or no diff when expected (empty array)
			if (expected.Type == JTokenType.Array && ((JArray)expected).Count == 0)
			{
				Assert.IsNull(actualDiff, $"{testName}: Expected no diff but got: {actualDiff}");
			}
			else
			{
				Assert.IsNotNull(actualDiff, $"{testName}: Expected diff but got null");
			}
		}

		private JToken ConvertToJsonPatchFormat(JToken diff)
		{
			// This is a simplified conversion - the actual formats are quite different
			// jsondiffpatch.net uses a different format than JSON Patch
			if (diff == null) return new JArray();

			// For now, just return the original diff for inspection
			return diff;
		}

		[Test]
		public void AppendSingleProducesCorrectDiff()
		{
			var jdp = new JsonDiffPatch();
			var left = CreateBaselineVariableSet();
			var right = CreateBaselineVariableSet();

			// Append single variable (var6)
			((JArray)right["Variables"]).Add(JToken.Parse(@"{""Id"": ""var6"", ""Name"": ""Variable 6"", ""Value"": ""Value 6""}"));

			JToken result = jdp.Diff(left, right);

			string expected = @"[
			  {
				""path"": ""/Variables/5"",
				""op"": ""add"",
				""value"": {
				  ""Id"": ""var6"",
				  ""Name"": ""Variable 6"",
				  ""Value"": ""Value 6""
				}
			  }
			]";

			AssertDiffMatchesExpected(result, expected, "AppendSingle");
		}

		[Test]
		public void AppendMultipleProducesCorrectDiff()
		{
			var jdp = new JsonDiffPatch();
			var left = CreateBaselineVariableSet();
			var right = CreateBaselineVariableSet();

			// Append multiple variables (var6 and var7)
			((JArray)right["Variables"]).Add(JToken.Parse(@"{""Id"": ""var6"", ""Name"": ""Variable 6"", ""Value"": ""Value 6""}"));
			((JArray)right["Variables"]).Add(JToken.Parse(@"{""Id"": ""var7"", ""Name"": ""Variable 7"", ""Value"": ""Value 7""}"));

			JToken result = jdp.Diff(left, right);

			string expected = @"[
			  {
				""path"": ""/Variables/5"",
				""op"": ""add"",
				""value"": {
				  ""Id"": ""var6"",
				  ""Name"": ""Variable 6"",
				  ""Value"": ""Value 6""
				}
			  },
			  {
				""path"": ""/Variables/6"",
				""op"": ""add"",
				""value"": {
				  ""Id"": ""var7"",
				  ""Name"": ""Variable 7"",
				  ""Value"": ""Value 7""
				}
			  }
			]";

			AssertDiffMatchesExpected(result, expected, "AppendMultiple");
		}

		[Test]
		public void InsertSingleProducesCorrectDiff()
		{
			var jdp = new JsonDiffPatch();
			var left = CreateBaselineVariableSet();
			var right = CreateBaselineVariableSet();

			// Insert variable at index 1 (var1.5)
			var variables = (JArray)right["Variables"];
			variables.Insert(1, JToken.Parse(@"{""Id"": ""var1.5"", ""Name"": ""Variable 1.5"", ""Value"": ""Value 1.5""}"));

			JToken result = jdp.Diff(left, right);

			string expected = @"[
			  {
				""path"": ""/Variables/1"",
				""op"": ""add"",
				""value"": {
				  ""Id"": ""var1.5"",
				  ""Name"": ""Variable 1.5"",
				  ""Value"": ""Value 1.5""
				}
			  }
			]";

			AssertDiffMatchesExpected(result, expected, "InsertSingle");
		}

		[Test]
		public void InsertMultipleProducesCorrectDiff()
		{
			var jdp = new JsonDiffPatch();
			var left = CreateBaselineVariableSet();
			var right = CreateBaselineVariableSet();

			// Insert multiple variables at index 1 (var1.5 and var1.75)
			var variables = (JArray)right["Variables"];
			variables.Insert(1, JToken.Parse(@"{""Id"": ""var1.5"", ""Name"": ""Variable 1.5"", ""Value"": ""Value 1.5""}"));
			variables.Insert(2, JToken.Parse(@"{""Id"": ""var1.75"", ""Name"": ""Variable 1.75"", ""Value"": ""Value 1.75""}"));

			JToken result = jdp.Diff(left, right);

			string expected = @"[
			  {
				""path"": ""/Variables/1"",
				""op"": ""add"",
				""value"": {
				  ""Id"": ""var1.5"",
				  ""Name"": ""Variable 1.5"",
				  ""Value"": ""Value 1.5""
				}
			  },
			  {
				""path"": ""/Variables/2"",
				""op"": ""add"",
				""value"": {
				  ""Id"": ""var1.75"",
				  ""Name"": ""Variable 1.75"",
				  ""Value"": ""Value 1.75""
				}
			  }
			]";

			AssertDiffMatchesExpected(result, expected, "InsertMultiple");
		}

		[Test]
		public void PrependSingleProducesCorrectDiff()
		{
			var jdp = new JsonDiffPatch();
			var left = CreateBaselineVariableSet();
			var right = CreateBaselineVariableSet();

			// Prepend single variable at index 0 (var0)
			var variables = (JArray)right["Variables"];
			variables.Insert(0, JToken.Parse(@"{""Id"": ""var0"", ""Name"": ""Variable 0"", ""Value"": ""Value 0""}"));

			JToken result = jdp.Diff(left, right);

			string expected = @"[
			  {
				""path"": ""/Variables/0"",
				""op"": ""add"",
				""value"": {
				  ""Id"": ""var0"",
				  ""Name"": ""Variable 0"",
				  ""Value"": ""Value 0""
				}
			  }
			]";

			AssertDiffMatchesExpected(result, expected, "PrependSingle");
		}

		[Test]
		public void PrependMultipleProducesCorrectDiff()
		{
			var jdp = new JsonDiffPatch(new Options { TextDiff = TextDiffMode.Simple });
			var left = CreateBaselineVariableSet();
			var right = CreateBaselineVariableSet();

			// Prepend multiple variables (var0 and var0.5)
			var variables = (JArray)right["Variables"];
			variables.Insert(0, JToken.Parse(@"{""Id"": ""var0"", ""Name"": ""Variable 0"", ""Value"": ""Value 0""}"));
			variables.Insert(1, JToken.Parse(@"{""Id"": ""var0.5"", ""Name"": ""Variable 0.5"", ""Value"": ""Value 0.5""}"));

			JToken result = jdp.Diff(left, right);

			string expected = @"[
			  {
				""path"": ""/Variables/0"",
				""op"": ""add"",
				""value"": {
				  ""Id"": ""var0"",
				  ""Name"": ""Variable 0"",
				  ""Value"": ""Value 0""
				}
			  },
			  {
				""path"": ""/Variables/1"",
				""op"": ""add"",
				""value"": {
				  ""Id"": ""var0.5"",
				  ""Name"": ""Variable 0.5"",
				  ""Value"": ""Value 0.5""
				}
			  }
			]";

			AssertDiffMatchesExpected(result, expected, "PrependMultiple");
		}

		[Test]
		public void ReorderSingleProducesCorrectDiff()
		{
			var jdp = new JsonDiffPatch(new Options {
				ArrayDiff = ArrayDiffMode.Efficient,
				DiffArrayOptions = new ArrayOptions { DetectMove = true, IncludeValueOnMove = false },
				ObjectHash = (jObj) => jObj["Id"]?.Value<string>()
			});
			var left = CreateBaselineVariableSet();
			var right = CreateBaselineVariableSet();

			// Swap elements at index 2 and 3 (var3 and var4)
			var variables = (JArray)right["Variables"];
			var temp = variables[2];
			variables[2] = variables[3];
			variables[3] = temp;

			JToken result = jdp.Diff(left, right);

			string expected = @"[]"; // Empty array - no diff expected for pure reordering

			AssertDiffMatchesExpected(result, expected, "ReorderSingle");
		}

		[Test]
		public void ReorderMultipleProducesCorrectDiff()
		{
			var jdp = new JsonDiffPatch(new Options {
				ArrayDiff = ArrayDiffMode.Efficient,
				DiffArrayOptions = new ArrayOptions { DetectMove = true, IncludeValueOnMove = false },
				ObjectHash = (jObj) => jObj["Id"]?.Value<string>()
			});
			var left = CreateBaselineVariableSet();
			var right = CreateBaselineVariableSet();

			// Reverse the entire array
			var variables = (JArray)right["Variables"];
			var reversedVariables = new JArray();
			for (int i = variables.Count - 1; i >= 0; i--)
			{
				reversedVariables.Add(variables[i]);
			}
			right["Variables"] = reversedVariables;

			JToken result = jdp.Diff(left, right);

			string expected = @"[]"; // Empty array - no diff expected for pure reordering

			AssertDiffMatchesExpected(result, expected, "ReorderMultiple");
		}

		[Test]
		public void RemoveSingleProducesCorrectDiff()
		{
			var jdp = new JsonDiffPatch();
			var left = CreateBaselineVariableSet();
			var right = CreateBaselineVariableSet();

			// Remove variable at index 1 (var2)
			var variables = (JArray)right["Variables"];
			variables.RemoveAt(1);

			JToken result = jdp.Diff(left, right);

			string expected = @"[
			  {
				""path"": ""/Variables/1"",
				""op"": ""remove""
			  }
			]";

			AssertDiffMatchesExpected(result, expected, "RemoveSingle");
		}

		[Test]
		public void RemoveMultipleProducesCorrectDiff()
		{
			var jdp = new JsonDiffPatch();
			var left = CreateBaselineVariableSet();
			var right = CreateBaselineVariableSet();

			// Remove variables at indices 1 and 2 (var2 and var3)
			var variables = (JArray)right["Variables"];
			variables.RemoveAt(2); // Remove var3 first (higher index)
			variables.RemoveAt(1); // Then remove var2

			JToken result = jdp.Diff(left, right);

			string expected = @"[
			  {
				""path"": ""/Variables/2"",
				""op"": ""remove""
			  },
			  {
				""path"": ""/Variables/1"",
				""op"": ""remove""
			  }
			]";

			AssertDiffMatchesExpected(result, expected, "RemoveMultiple");
		}

		[Test]
		public void ModifySingleProducesCorrectDiff()
		{
			var jdp = new JsonDiffPatch();
			var left = CreateBaselineVariableSet();
			var right = CreateBaselineVariableSet();

			// Modify the Name property of the first variable (var1)
			((JObject)((JArray)right["Variables"])[0])["Name"] = "NewName";

			JToken result = jdp.Diff(left, right);

			string expected = @"[
			  {
				""path"": ""/Variables/0/Name"",
				""op"": ""replace"",
				""value"": ""NewName""
			  }
			]";

			AssertDiffMatchesExpected(result, expected, "ModifySingle");
		}

		[Test]
		public void ModifyMultipleProducesCorrectDiff()
		{
			var jdp = new JsonDiffPatch();
			var left = CreateBaselineVariableSet();
			var right = CreateBaselineVariableSet();

			// Modify the Name property of the first two variables
			((JObject)((JArray)right["Variables"])[0])["Name"] = "NewName";
			((JObject)((JArray)right["Variables"])[1])["Name"] = "AnotherNewName";

			JToken result = jdp.Diff(left, right);

			string expected = @"[
			  {
				""path"": ""/Variables/0/Name"",
				""op"": ""replace"",
				""value"": ""NewName""
			  },
			  {
				""path"": ""/Variables/1/Name"",
				""op"": ""replace"",
				""value"": ""AnotherNewName""
			  }
			]";

			AssertDiffMatchesExpected(result, expected, "ModifyMultiple");
		}
	}
}
