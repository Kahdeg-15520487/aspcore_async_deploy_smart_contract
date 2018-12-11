using System;
using System.Collections;
using System.Collections.Generic;

namespace aspcore_bec.UnitTest
{
    internal class HashTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { new string[] { "hash1", "hash2", "hash3", } };
            yield return new object[] { new string[] { null, null, null, } };
            yield return new object[] { new string[] { string.Empty, string.Empty } };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}