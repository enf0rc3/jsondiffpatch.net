using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JsonDiffPatchDotNet.UnitTests
{
	[TestFixture]
	public class DiffUnitTests
	{
		[Test]
		public void Diff_EmptyObjects_EmptyPatch()
		{
			var jdp = new JsonDiffPatch();
			var empty = JObject.Parse(@"{}");

			JToken result = jdp.Diff(empty, empty);

			Assert.IsNull(result);
		}

		[Test]
		public void Diff_EqualBooleanProperty_NoDiff()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{""p"": true }");
			var right = JObject.Parse(@"{""p"": true }");

			JToken result = jdp.Diff(left, right);

			Assert.IsNull(result);
		}

		[Test]
		public void Diff_DiffBooleanProperty_ValidPatch()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{""p"": true }");
			var right = JObject.Parse(@"{""p"": false }");

			JToken result = jdp.Diff(left, right);

			Assert.AreEqual(JTokenType.Object, result.Type);
			JObject obj = (JObject)result;
			Assert.IsNotNull(obj.Property("p"), "Property Name");
			Assert.AreEqual(JTokenType.Array, obj.Property("p").Value.Type, "Array Value");
			Assert.AreEqual(2, ((JArray)obj.Property("p").Value).Count, "Array Length");
			Assert.IsTrue(((JArray)obj.Property("p").Value)[0].ToObject<bool>(), "Array Old Value");
			Assert.IsFalse(((JArray)obj.Property("p").Value)[1].ToObject<bool>(), "Array New Value");
		}

		[Test]
		public void Diff_BooleanPropertyDeleted_ValidPatch()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ ""p"": true }");
			var right = JObject.Parse(@"{ }");

			JToken result = jdp.Diff(left, right);

			Assert.AreEqual(JTokenType.Object, result.Type);
			JObject obj = (JObject)result;
			Assert.IsNotNull(obj.Property("p"), "Property Name");
			Assert.AreEqual(JTokenType.Array, obj.Property("p").Value.Type, "Array Value");
			Assert.AreEqual(3, ((JArray)obj.Property("p").Value).Count, "Array Length");
			Assert.IsTrue(((JArray)obj.Property("p").Value)[0].ToObject<bool>(), "Array Old Value");
			Assert.AreEqual(0, ((JArray)obj.Property("p").Value)[1].ToObject<int>(), "Array New Value");
			Assert.AreEqual(0, ((JArray)obj.Property("p").Value)[2].ToObject<int>(), "Array Deleted Indicator");
		}

		[Test]
		public void Diff_BooleanPropertyAdded_ValidPatch()
		{
			var jdp = new JsonDiffPatch();
			var left = JObject.Parse(@"{ }");
			var right = JObject.Parse(@"{ ""p"": true }");

			JToken result = jdp.Diff(left, right);

			Assert.AreEqual(JTokenType.Object, result.Type);
			JObject obj = (JObject)result;
			Assert.IsNotNull(obj.Property("p"), "Property Name");
			Assert.AreEqual(JTokenType.Array, obj.Property("p").Value.Type, "Array Value");
			Assert.AreEqual(1, ((JArray)obj.Property("p").Value).Count, "Array Length");
			Assert.IsTrue(((JArray)obj.Property("p").Value)[0].ToObject<bool>(), "Array Added Value");
		}

		[Test]
		public void Diff_EfficientStringDiff_ValidPatch()
		{
			var jdp = new JsonDiffPatch(new Options { TextDiff = TextDiffMode.Efficient });
			var left = JObject.Parse(@"{ ""p"": ""lp.Value.ToString().Length > _options.MinEfficientTextDiffLength"" }");
			var right = JObject.Parse(@"{ ""p"": ""blah1"" }");

			JToken result = jdp.Diff(left, right);

			Assert.AreEqual(JTokenType.Object, result.Type);
			JObject obj = (JObject)result;
			Assert.IsNotNull(obj.Property("p"), "Property Name");
			Assert.AreEqual(JTokenType.Array, obj.Property("p").Value.Type, "Array Value");
			Assert.AreEqual(3, ((JArray)obj.Property("p").Value).Count, "Array Length");
			Assert.AreEqual("@@ -1,64 +1,5 @@\n-lp.Value.ToString().Length %3e _options.MinEfficientTextDiffLength\n+blah1\n", ((JArray)obj.Property("p").Value)[0].ToString(), "Array Added Value");
			Assert.AreEqual(0, ((JArray)obj.Property("p").Value)[1].ToObject<int>(), "Array Added Value");
			Assert.AreEqual(2, ((JArray)obj.Property("p").Value)[2].ToObject<int>(), "Array String Diff Indicator");
		}

		[Test]
		public void Diff_EfficientStringDiff_NoChanges()
		{
			var jdp = new JsonDiffPatch(new Options { TextDiff = TextDiffMode.Efficient });
			var left = JObject.Parse(@"{ ""p"": ""lp.Value.ToString().Length > _options.MinEfficientTextDiffLength"" }");
			var right = JObject.Parse(@"{ ""p"": ""lp.Value.ToString().Length > _options.MinEfficientTextDiffLength"" }");

			JToken result = jdp.Diff(left, right);

			Assert.IsNull(result, "No Changes");
		}

		[Test]
		public void Diff_LeftNull_Exception()
		{
			var jdp = new JsonDiffPatch();
			var obj = JObject.Parse(@"{ }");

			JToken result = jdp.Diff(null, obj);

			Assert.AreEqual(JTokenType.Array, result.Type);
		}

		[Test]
		public void Diff_RightNull_Exception()
		{
			var jdp = new JsonDiffPatch();
			var obj = JObject.Parse(@"{ }");

			JToken result = jdp.Diff(obj, null);

			Assert.AreEqual(JTokenType.Array, result.Type);
		}

		[Test]
		public void Diff_EfficientArrayDiffSame_NullDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var array = JToken.Parse(@"[1,2,3]");

			JToken diff = jdp.Diff(array, array);

			Assert.IsNull(diff);
		}

		[Test]
		public void Diff_EfficientArrayDiffDifferentHeadRemoved_ValidDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"[1,2,3,4]");
			var right = JToken.Parse(@"[2,3,4]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff);
			Assert.AreEqual(2, diff.Properties().Count());
			Assert.IsNotNull(diff["_0"]);
		}

		[Test]
		public void Diff_EfficientArrayDiffDifferentTailRemoved_ValidDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"[1,2,3,4]");
			var right = JToken.Parse(@"[1,2,3]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff);
			Assert.AreEqual(2, diff.Properties().Count());
			Assert.IsNotNull(diff["_3"]);
		}

		[Test]
		public void Diff_EfficientArrayDiffDifferentHeadAdded_ValidDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"[1,2,3,4]");
			var right = JToken.Parse(@"[0,1,2,3,4]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff);
			Assert.AreEqual(2, diff.Properties().Count());
			Assert.IsNotNull(diff["0"]);
		}

		[Test]
		public void Diff_EfficientArrayDiffTailMovedToHead_IgnoreMove_NoChange()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient, DiffArrayOptions = new ArrayOptions { DetectMove = true, IncludeValueOnMove = false } });
			var left = JToken.Parse(@"[1,2,3,4,5,6,7,8,9,10]");
			var right = JToken.Parse(@"[4,1,2,3,7,5,6,8,10,9]");

			var diff = jdp.Diff(left, right);

			Assert.IsNull(diff);
		}

		[Test]
		public void Diff_EfficientArrayDiffTailHeadMovedToTail_IncludeMove_ValidDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient, DiffArrayOptions = new ArrayOptions { DetectMove = true, IncludeValueOnMove = true } });
			var left = JToken.Parse(@"[1,2,3,4]");
			var right = JToken.Parse(@"[2,3,4,1]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff);
			Assert.AreEqual(2, diff.Properties().Count());
			Assert.AreEqual(diff["_0"], JToken.Parse("['', 3, 3]"));
		}

		[Test]
		public void Diff_EfficientArrayDiffWithComplexObjects_IncludeMove_ValidDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient, ObjectHash = (jObj) => jObj["Id"].Value<string>(), DiffArrayOptions = new ArrayOptions { DetectMove = true, IncludeValueOnMove = true } });
			//var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"[{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""p"":false}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9"", ""p"":true}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC10"", ""p"":true}]");
			var right = JToken.Parse(@"[{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC10"", ""p"":false}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""p"":true}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9"", ""p"":true}]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff);
			Assert.AreEqual(4, diff.Properties().Count());
			Assert.AreEqual(diff["_2"], JToken.Parse("['', 0, 3]"));
			Assert.AreEqual(diff["0"]["p"], JToken.Parse("[true, false]"));
			Assert.AreEqual(diff["1"]["p"], JToken.Parse("[false, true]"));
		}

		[Test]
		public void Diff_EfficientArrayDiffDifferentTailAdded_ValidDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"[1,2,3,4]");
			var right = JToken.Parse(@"[1,2,3,4,5]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff);
			Assert.AreEqual(2, diff.Properties().Count());
			Assert.IsNotNull(diff["4"]);
		}

		[Test]
		public void Diff_EfficientArrayDiffDifferentHeadTailAdded_ValidDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"[1,2,3,4]");
			var right = JToken.Parse(@"[0,1,2,3,4,5]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff);
			Assert.AreEqual(3, diff.Properties().Count());
			Assert.IsNotNull(diff["0"]);
			Assert.IsNotNull(diff["5"]);
		}

		[Test]
		public void Diff_EfficientArrayDiffSameLengthNested_ValidDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient, ObjectHash = (jObj) => jObj["Id"].Value<string>() });
			var left = JToken.Parse(@"[1,2,{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""p"":false},4]");
			var right = JToken.Parse(@"[1,2,{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""p"":true},4]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff);
			Assert.AreEqual(2, diff.Properties().Count());
			Assert.IsNotNull(diff["2"]);
		}

        [Test]
        public void Diff_EfficientArrayDiffWithComplexObject_ValidDiff()
        {
            var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient, ObjectHash = (jObj) => jObj["Id"].Value<string>() });
            //var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
            var left = JToken.Parse(@"[{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""p"":false}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC9"", ""p"":true}]");
            var right = JToken.Parse(@"[{""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC8"", ""p"":true}, {""Id"" : ""F12B21EF-F57D-4958-ADDC-A3F52EC25EC10"", ""p"":false}]");

            JObject diff = jdp.Diff(left, right) as JObject;

            Assert.IsNotNull(diff);
            Assert.AreEqual(4, diff.Properties().Count());
        }

		[Test]
		public void Diff_EfficientArrayDiffSameWithObject_NoDiff()
		{
			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient });
			var left = JToken.Parse(@"
{
	""@context"": [
		""http://www.w3.org/ns/csvw"",
		{
			""@language"": ""en"",
			""@base"": ""http://example.org""
		}
	]
}");
			var right = left.DeepClone();

			JToken diff = jdp.Diff(left, right);

			Assert.IsNull(diff);
		}

		[Test]
		public void Diff_EfficientArrayDiffHugeArrays_NoStackOverflow()
		{
			const int arraySize = 1000;
			Func<int, int, JToken> hugeArrayFunc = (startIndex, count) =>
			{
				var builder = new StringBuilder("[");
				foreach (var i in Enumerable.Range(startIndex, count))
				{
					builder.Append($"{i},");
				}
				builder.Append("]");

				return JToken.Parse(builder.ToString());
			};

			var jdp = new JsonDiffPatch();
			var left = hugeArrayFunc(0, arraySize);
			var right = hugeArrayFunc(arraySize / 2, arraySize);

			JToken diff = jdp.Diff(left, right);
			var restored = jdp.Patch(left, diff);

			Assert.That(JToken.DeepEquals(restored, right));
		}

		[Test]
		public void Diff_EfficientArrayDiffHugeArrays_OnlyIgnoredMoves_NoStackOverflow()
		{
			const int arraySize = 1000;

			Func<JToken> shuffledArrayFunc = () =>
			{
				Random rng = new Random();
				var builder = new StringBuilder("[");

				var randomList = new List<int>();
				for (int i = 0; i < arraySize; i++)
				{
					randomList.Add(i);
				}

				// Shuffle array
				int n = arraySize - 1;
				while (n > 1)
				{
					n--;
					int k = rng.Next(n + 1);
					int val = randomList[k];
					randomList[k] = randomList[n];
					randomList[n] = val;
				}

				foreach (var i in randomList)
				{
					builder.Append($"{i},");
				}
				builder.Append("]");

				return JToken.Parse(builder.ToString());
			};

			var jdp = new JsonDiffPatch(new Options { ArrayDiff = ArrayDiffMode.Efficient, DiffArrayOptions = new ArrayOptions { DetectMove = true, IncludeValueOnMove = false } });
			var left = shuffledArrayFunc();
			var right = shuffledArrayFunc();

			JToken diff = jdp.Diff(left, right);
			Assert.IsNull(diff);
		}

		[Test]
		public void Diff_IntStringDiff_ValidPatch()
		{
			var jdp = new JsonDiffPatch();
			var left = JToken.Parse(@"1");
			var right = JToken.Parse(@"""hello""");

			JToken result = jdp.Diff(left, right);

			Assert.AreEqual(JTokenType.Array, result.Type);
			JArray array = (JArray)result;
			Assert.AreEqual(2, array.Count);
			Assert.AreEqual(left, array[0]);
			Assert.AreEqual(right, array[1]);
		}

		[Test]
		public void Diff_ArrayInsertMidTextDiffNoObjectHash_ValidPatch_only_what_changes()
		{
			var jdp = new JsonDiffPatch(new Options {
				TextDiff = TextDiffMode.Simple,
			});

			var left = JToken.Parse(@"{
				""Variables"": [
					{""Id"": ""var1"", ""Name"": ""Variable 1"", ""Value"": ""Value 1""},
					{""Id"": ""var2"", ""Name"": ""Variable 2"", ""Value"": ""Value 2""},
					{""Id"": ""var3"", ""Name"": ""Variable 3"", ""Value"": ""Value 3""},
					{""Id"": ""var4"", ""Name"": ""Variable 4"", ""Value"": ""Value 4""},
					{""Id"": ""var5"", ""Name"": ""Variable 5"", ""Value"": ""Value 5""}
				]
			}");

			var right = JToken.Parse(@"{
				""Variables"": [
					{""Id"": ""var1"", ""Name"": ""Variable 1"", ""Value"": ""Value 1""},
					{""Id"": ""var1.5"", ""Name"": ""Variable 1.5"", ""Value"": ""Value 1.5""},
					{""Id"": ""var2"", ""Name"": ""Variable 2"", ""Value"": ""Value 2""},
					{""Id"": ""var3"", ""Name"": ""Variable 3"", ""Value"": ""Value 3""},
					{""Id"": ""var4"", ""Name"": ""Variable 4"", ""Value"": ""Value 4""},
					{""Id"": ""var5"", ""Name"": ""Variable 5"", ""Value"": ""Value 5""}
				]
			}");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff, "Diff should not be null");
			Console.WriteLine($"Fixed diff with ObjectHash: {diff}");

			var variablesDiff = diff["Variables"] as JObject;
			Assert.IsNotNull(variablesDiff, "Variables diff should not be null");

			// With ObjectHash, we should only see the array type marker and the insert
			Assert.AreEqual(2, variablesDiff.Properties().Count(), "Should have minimal diff entries with ObjectHash");
			Assert.IsNotNull(variablesDiff["_t"], "Should have array type marker");
			Assert.IsNotNull(variablesDiff["1"], "Should have insert at index 1");

			// The insert should be a single-element array (add operation)
			var insertDiff = variablesDiff["1"] as JArray;
			Assert.IsNotNull(insertDiff, "Insert diff should be array");
			Assert.AreEqual(1, insertDiff.Count, "Insert should have one element");
		}

		[Test]
		public void Diff_ArrayRemoveMultipleMiddleNoObjectHash_ValidPatch_only_what_changes()
		{
			var jdp = new JsonDiffPatch(new Options {
				TextDiff = TextDiffMode.Simple
			});

			var left = JToken.Parse(@"{
				""Variables"": [
					{""Id"": ""var1"", ""Name"": ""Variable 1"", ""Value"": ""Value 1""},
					{""Id"": ""var2"", ""Name"": ""Variable 2"", ""Value"": ""Value 2""},
					{""Id"": ""var3"", ""Name"": ""Variable 3"", ""Value"": ""Value 3""},
					{""Id"": ""var4"", ""Name"": ""Variable 4"", ""Value"": ""Value 4""},
					{""Id"": ""var5"", ""Name"": ""Variable 5"", ""Value"": ""Value 5""}
				]
			}");

			var right = JToken.Parse(@"{
				""Variables"": [
					{""Id"": ""var1"", ""Name"": ""Variable 1"", ""Value"": ""Value 1""},
					{""Id"": ""var4"", ""Name"": ""Variable 4"", ""Value"": ""Value 4""},
					{""Id"": ""var5"", ""Name"": ""Variable 5"", ""Value"": ""Value 5""}
				]
			}");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff, "Diff should not be null");
			Console.WriteLine($"Remove multiple diff: {diff}");

			var variablesDiff = diff["Variables"] as JObject;
			Assert.IsNotNull(variablesDiff, "Variables diff should not be null");

			// With auto-detection, should show minimal diff: array marker + two deletions
			Assert.AreEqual(3, variablesDiff.Properties().Count(), "Should have minimal diff entries for remove multiple");
			Assert.IsNotNull(variablesDiff["_t"], "Should have array type marker");
			Assert.IsNotNull(variablesDiff["_1"], "Should have deletion at index 1 (var2)");
			Assert.IsNotNull(variablesDiff["_2"], "Should have deletion at index 2 (var3)");

			// The deletions should be three-element arrays (delete operations)
			var delete1Diff = variablesDiff["_1"] as JArray;
			Assert.IsNotNull(delete1Diff, "Delete diff should be array");
			Assert.AreEqual(3, delete1Diff.Count, "Delete should have three elements");
			Assert.AreEqual(0, delete1Diff[2].Value<int>(), "Should be delete operation");

			var delete2Diff = variablesDiff["_2"] as JArray;
			Assert.IsNotNull(delete2Diff, "Delete diff should be array");
			Assert.AreEqual(3, delete2Diff.Count, "Delete should have three elements");
			Assert.AreEqual(0, delete2Diff[2].Value<int>(), "Should be delete operation");
		}

		[Test]
		public void Diff_ArrayRemoveFirstElementNoObjectHash_MinimalDiff()
		{
			var jdp = new JsonDiffPatch(new Options {
				TextDiff = TextDiffMode.Simple
			});

			var left = JToken.Parse(@"{
				""Items"": [
					{""Type"": ""Header"", ""Content"": ""Title""},
					{""Type"": ""Paragraph"", ""Content"": ""First paragraph""},
					{""Type"": ""Paragraph"", ""Content"": ""Second paragraph""}
				]
			}");

			var right = JToken.Parse(@"{
				""Items"": [
					{""Type"": ""Paragraph"", ""Content"": ""First paragraph""},
					{""Type"": ""Paragraph"", ""Content"": ""Second paragraph""}
				]
			}");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff, "Diff should not be null");
			var itemsDiff = diff["Items"] as JObject;
			Assert.IsNotNull(itemsDiff, "Items diff should not be null");

			// Should only show the deletion, not treat all remaining items as changed
			Assert.AreEqual(2, itemsDiff.Properties().Count(), "Should have minimal diff entries");
			Assert.IsNotNull(itemsDiff["_t"], "Should have array type marker");
			Assert.IsNotNull(itemsDiff["_0"], "Should have deletion at index 0");
		}

		[Test]
		public void Diff_ArrayInsertAtBeginningNoObjectHash_MinimalDiff()
		{
			var jdp = new JsonDiffPatch(new Options {
				TextDiff = TextDiffMode.Simple
			});

			var left = JToken.Parse(@"[
				{""Priority"": 2, ""Task"": ""Review code""},
				{""Priority"": 3, ""Task"": ""Deploy to staging""}
			]");

			var right = JToken.Parse(@"[
				{""Priority"": 1, ""Task"": ""Fix critical bug""},
				{""Priority"": 2, ""Task"": ""Review code""},
				{""Priority"": 3, ""Task"": ""Deploy to staging""}
			]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff, "Diff should not be null");

			// Should only show the insertion at index 0, not treat all items as changed
			Assert.AreEqual(2, diff.Properties().Count(), "Should have minimal diff entries");
			Assert.IsNotNull(diff["_t"], "Should have array type marker");
			Assert.IsNotNull(diff["0"], "Should have insertion at index 0");

			// The insertion should be a single-element array
			var insertDiff = diff["0"] as JArray;
			Assert.IsNotNull(insertDiff, "Insert diff should be array");
			Assert.AreEqual(1, insertDiff.Count, "Insert should have one element");
		}

		[Test]
		public void Diff_ArrayReplaceMiddleElementNoObjectHash_CompleteReplacement()
		{
			var jdp = new JsonDiffPatch(new Options {
				TextDiff = TextDiffMode.Simple
			});

			var left = JToken.Parse(@"[
				{""Status"": ""Active"", ""UserId"": 1},
				{""Status"": ""Inactive"", ""UserId"": 2},
				{""Status"": ""Active"", ""UserId"": 3}
			]");

			var right = JToken.Parse(@"[
				{""Status"": ""Active"", ""UserId"": 1},
				{""Status"": ""Pending"", ""UserId"": 4},
				{""Status"": ""Active"", ""UserId"": 3}
			]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff, "Diff should not be null");

			// With content-based matching (no ObjectHash), the algorithm detects that:
			// - Index 0 and 2 have identical objects, so no changes
			// - Index 1 has a completely different object (both Status AND UserId changed)
			// - Since this is a complete object replacement (not a property change),
			//   it shows both the deletion of the old object and addition of the new one
			Assert.AreEqual(3, diff.Properties().Count(), "Should have array marker plus replacement operations");
			Assert.IsNotNull(diff["_t"], "Should have array type marker");
			Assert.IsNotNull(diff["_1"], "Should have deletion of old object at index 1");
			Assert.IsNotNull(diff["1"], "Should have addition of new object at index 1");

			// Should not have changes for indices 0 or 2 since those objects are identical
			Assert.IsNull(diff["0"], "Should not have changes at index 0");
			Assert.IsNull(diff["2"], "Should not have changes at index 2");
		}

		[Test]
		public void Diff_NestedObjectArrayNoObjectHash_MinimalDiff()
		{
			var jdp = new JsonDiffPatch(new Options {
				TextDiff = TextDiffMode.Simple
			});

			var left = JToken.Parse(@"{
				""Departments"": [
					{
						""Name"": ""Engineering"",
						""Employees"": [
							{""Name"": ""John"", ""Role"": ""Developer""},
							{""Name"": ""Jane"", ""Role"": ""Manager""}
						]
					},
					{
						""Name"": ""Sales"",
						""Employees"": [
							{""Name"": ""Mike"", ""Role"": ""Rep""}
						]
					}
				]
			}");

			var right = JToken.Parse(@"{
				""Departments"": [
					{
						""Name"": ""HR"",
						""Employees"": [
							{""Name"": ""Sarah"", ""Role"": ""Coordinator""}
						]
					},
					{
						""Name"": ""Engineering"",
						""Employees"": [
							{""Name"": ""John"", ""Role"": ""Developer""},
							{""Name"": ""Jane"", ""Role"": ""Manager""}
						]
					},
					{
						""Name"": ""Sales"",
						""Employees"": [
							{""Name"": ""Mike"", ""Role"": ""Rep""}
						]
					}
				]
			}");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff, "Diff should not be null");
			var deptsDiff = diff["Departments"] as JObject;
			Assert.IsNotNull(deptsDiff, "Departments diff should not be null");

			// Should detect that Engineering and Sales departments are the same, just inserted HR at beginning
			Assert.AreEqual(2, deptsDiff.Properties().Count(), "Should have minimal diff entries for nested objects");
			Assert.IsNotNull(deptsDiff["_t"], "Should have array type marker");
			Assert.IsNotNull(deptsDiff["0"], "Should have insertion at index 0");
		}

		[Test]
		public void Diff_MixedArrayTypesNoObjectHash_MinimalDiff()
		{
			var jdp = new JsonDiffPatch(new Options {
				TextDiff = TextDiffMode.Simple
			});

			var left = JToken.Parse(@"[
				""string1"",
				{""Type"": ""object"", ""Value"": 42},
				[1, 2, 3],
				123
			]");

			var right = JToken.Parse(@"[
				""string1"",
				""inserted string"",
				{""Type"": ""object"", ""Value"": 42},
				[1, 2, 3],
				123
			]");

			JObject diff = jdp.Diff(left, right) as JObject;

			Assert.IsNotNull(diff, "Diff should not be null");

			// Should only show the insertion, not treat all subsequent items as changed
			Assert.AreEqual(2, diff.Properties().Count(), "Should have minimal diff entries for mixed types");
			Assert.IsNotNull(diff["_t"], "Should have array type marker");
			Assert.IsNotNull(diff["1"], "Should have insertion at index 1");
		}
	}
}
