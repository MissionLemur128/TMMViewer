using System.Numerics;
using TMMLibrary.IOUtils;
using TMMLibrary.TMMData.DataTypes;
using Xunit;

namespace TmmTest
{
    public class TmmDataTests
    {
        [Fact]
        public void BoneWeightsTest()
        {
            var input = new BoneWeights()
            {
                BoneIndices = [1, 2, 3, 4],
                Weights = [10, 20, 30, 40],
            };

            byte[] encodedBytes;
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                input.Encode(bw);
                encodedBytes = ms.ToArray();
            }

            using (var ms = new MemoryStream(encodedBytes))
            using (var br = new BinaryReader(ms))
            {
                var maxVertexWeightCount = input.Weights.Length;
                var decoded = BoneWeights.Decode(br, maxVertexWeightCount);
                TmmAssert.AssertEqualFields(input, decoded);
            }
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(1)]
        public void VertexTest(float tangentW)
        {
            var input = new Vertex()
            {
                Origin = new Vector3(1.0f, 2.0f, 3.0f),
                Normal = Vector3.UnitZ,
                Tangent = Vector4.UnitX,
            };
            input.Tangent.W = tangentW; // Ensure W is positive for decoding to work correctly

            byte[] encodedBytes;
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                input.Encode(bw);
                encodedBytes = ms.ToArray();
            }

            using (var ms = new MemoryStream(encodedBytes))
            using (var br = new BinaryReader(ms))
            {
                var decoded = Vertex.Decode(br);
                TmmAssert.AssertEqualFields(input, decoded);
            }
        }

        [Theory]
        [InlineData(-0.5f, 1.0f, 0.2f)]
        [InlineData(1f, 0.0f, 0f)]
        [InlineData(-0.5f, 0.0f, -0.5f)]
        [InlineData(0.5f, 0.0f, -0.5f)]
        public void NormalEncodingTest(float normalX, float normalY, float normalZ, float tangentW = 1)
        {
            var inputNormal = Vector3.Normalize(new Vector3(normalX, normalY, normalZ));
            var inputBitangent = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, inputNormal));
            var inputTangent = new Vector4(Vector3.Cross(inputBitangent, inputNormal), tangentW);

            byte[] encodedBytes = NormalEncoding.EncodeDecodeTangentSpace(inputNormal, inputTangent);
            NormalEncoding.DecodeTangentSpace(encodedBytes, out var outputNormal, out var outputTangent);

            TmmAssert.AssertEqualFields(inputNormal, outputNormal);
            TmmAssert.AssertEqualFields(inputTangent, outputTangent);
            Assert.Equal(tangentW, outputTangent.W);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(1)]
        public void NormalEncodingSphereTest(float tangentW)
        {
            var uDivisions = 10;
            var vDivisions = 10;
            for (int u = 0; u < uDivisions; ++u)
            {
                // omit the poles as they are a bit special
                var s = 2;
                for (int v = s; v < vDivisions-s; ++v)
                {
                    var ufraction = (float)u / uDivisions;
                    var vfraction = (float)v / vDivisions;
                     
                    var yCos = MathF.Cos(vfraction * MathF.PI);
                    var x = MathF.Cos(ufraction * MathF.PI * 2.0f) * yCos;
                    var z = MathF.Sin(ufraction * MathF.PI * 2.0f) * yCos;
                    var y = MathF.Sin(vfraction * MathF.PI);
                    NormalEncodingTest(x, y, z, tangentW);
                }
            }
        }
    }
}
