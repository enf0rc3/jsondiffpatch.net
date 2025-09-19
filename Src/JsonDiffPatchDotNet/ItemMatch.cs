using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonDiffPatchDotNet
{
    public abstract class ItemMatch
    {
        internal Func<JToken, object> ObjectHash;

        protected ItemMatch()
        {

        }

        protected ItemMatch(Func<JToken, object> objectHash)
        {
            ObjectHash = objectHash;
        }

        public virtual bool Match(JToken object1, JToken object2)
        {
            return Match(object1, object2, ObjectHash);
        }

		public virtual bool MatchArrayElement(JToken object1, int index1, JToken object2, int index2, bool matchByPosition = true)
		{
			// Step 1: Direct reference equality check (JavaScript === equivalent)
			if (ReferenceEquals(object1, object2))
			{
				return true;
			}

			// Step 2: Handle primitive types - in C#, use value equality for non-objects
			// (JavaScript would return false for type mismatch, but C# JToken equality is more appropriate)
			if (object1.Type != JTokenType.Object && object1.Type != JTokenType.Array &&
			    object2.Type != JTokenType.Object && object2.Type != JTokenType.Array)
			{
				return JToken.DeepEquals(object1, object2);
			}

			// Step 3: If either is primitive but other is object, no match (JavaScript behavior)
			if ((object1.Type != JTokenType.Object && object1.Type != JTokenType.Array) ||
			    (object2.Type != JTokenType.Object && object2.Type != JTokenType.Array))
			{
				return false;
			}

			// Step 4: Both are objects - check if ObjectHash is available
			if (ObjectHash != null)
			{
				// Use hash-based matching (without caching for simplicity)
				var hash1 = ObjectHash.Invoke(object1);
				var hash2 = ObjectHash.Invoke(object2);

				// If either hash is null, no match
				if (hash1 == null || hash2 == null)
				{
					return false;
				}

				return hash1.Equals(hash2);
			}

			// Step 5: No ObjectHash provided
			if (matchByPosition)
			{
				// Position-based matching (JavaScript behavior)
				return index1 == index2;
			}
			else
			{
				// Content-based matching for C# scenarios (when auto-detection suggests it)
				return JToken.DeepEquals(object1, object2);
			}
		}


        public virtual bool Match(JToken object1, JToken object2, Func<JToken, object> objectHash)
        {
			if(objectHash == null || object1.Type != JTokenType.Object)
			{
				return JToken.DeepEquals(object1, object2);
			}

			var hash1 = objectHash.Invoke(object1);
            if(hash1 == null)
            {
                return false;
            }
            var hash2 = objectHash.Invoke(object2);
            if(hash2 == null)
            {
                return false;
            }

            return hash1.Equals(hash2);
        }
    }
}
