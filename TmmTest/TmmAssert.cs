using System.Numerics;
using System.Reflection;
using Xunit;

namespace TmmTest
{
    public static class TmmAssert
    {
        public static void AssertEqualFields<T>(T expected, T actual, string path = "")
        {
            if (expected == null && actual == null)
                return;

            var type = expected.GetType();
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                var fieldPath = $"{path}/{field.Name}"; // helps to identify which field is being compared
                var value1 = field.GetValue(expected);
                var value2 = field.GetValue(actual);
                if (value1 is float float1 && value2 is float float2)
                {

                    var tolerance = 0.01f; // tolerance largely set to account for precision errors in normal encoding
                    Assert.Equal(float1, float2, tolerance);
                }
                else if (field.FieldType.IsPrimitive)
                {
                    Assert.Equal(value1, value2);
                }
                else if (value1 is Array array1 && value2 is Array array2)
                {
                    Assert.Equal(array1.Length, array2.Length);
                    for (int i = 0; i < array1.Length; ++i)
                    {
                        AssertEqualFields(array1.GetValue(i), array2.GetValue(i), fieldPath);
                    }
                }
                else
                {
                    AssertEqualFields(value1, value2, fieldPath);
                }
            }
        }
    }
}
