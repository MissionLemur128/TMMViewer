using System.Numerics;
using TMMLibrary;
using TMMLibrary.TMM;
using TMMLibrary.TMM.DataTypes;
using TMMLibrary.TMM.Sections;
using TMMLibrary.TMMData;
using Xunit;

namespace TmmTest
{
    public class TmmTests
    {
        [Fact]
        public void ModelBoundingInfoTest()
        {
            var input = new ModelBoundingInfo()
            {
                BoundingBoxes = new BoundingBox(new[] { new Vector3(1, 2, 3), new Vector3(4, 5, 6) }),
                LooseBoundingBoxes = new BoundingBox(new[] { new Vector3(7, 8, 9), new Vector3(10, 11, 12) }),
                BoundingRadius = 13.0f
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
                var decoded = ModelBoundingInfo.Decode(br);
                TmmAssert.AssertEqualFields(input, decoded);
            }
        }

        [Fact]
        public void ModelHeaderTest()
        {
            var input = new ModelHeader()
            {
                ModelName = "TestModel",
                Year = 2023,
                Unknown = [1, 2, 3, 4, 5, 6, 7]
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
                var decoded = ModelHeader.Decode(br);
                TmmAssert.AssertEqualFields(input, decoded);
            }
        }

        [Fact]
        public void ModelInfoTest()
        {
            var header = new TmmHeader { FileVersion = 35 };
            uint vertexCount = 7; // Some values below should align with the vertex count
            uint indexCount = vertexCount * 3;
            var input = new ModelInfo()
            {
                AssignedMaterialCount = 1,
                MaterialCount = 2,
                ShaderCount = 3,
                BoneCount = 4,
                UnknownCount1 = 5,
                AttachPointCount = 6,
                VertexCount = vertexCount,
                IndexCount = indexCount,
                VertexOffset = 9,
                IndexOffset = 10,
                IndexOffsetCopy = 10,
                IndexByteCount = indexCount * 2,
                BoneWeightsOffset = 11,
                BoneWeightsByteCount = vertexCount * 8,
                SubObjectsIdsOffset = 13,
                SubObjectsIdsByteCount = 14,
                Unknown2Count = 15,
                Unknown2Offset = 16,
                MaskDataOffset = 17,
                MaskDataByteCount = vertexCount * 2,
                EndByte = 256
            };

            byte[] encodedBytes;
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                input.Encode(header, bw);
                encodedBytes = ms.ToArray();
            }

            using (var ms = new MemoryStream(encodedBytes))
            using (var br = new BinaryReader(ms))
            {
                var decoded = ModelInfo.Decode(header, br);
                TmmAssert.AssertEqualFields(input, decoded);
            }
        }

        [Fact]
        public void TmmHeaderTest()
        {
            var input = new TmmHeader()
            {
                MagicId = TmmHeader.TMM_MAGIC,
                FileVersion = 35,
                MagicDp = TmmHeader.MAGIC_DP,
                DataOffset = 100
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
                var decoded = TmmHeader.Decode(br);
                TmmAssert.AssertEqualFields(input, decoded);
            }
        }

        [Fact]
        public void AttachPointTest()
        {
            var input = new AttachPoint()
            {
                BoneParent = 5,
                Name1 = "TestAttachPoint2",
                Name2 = "TestAttachPoint1",
                LocalTransform = Matrix4x4.CreateLookTo(new Vector3(11, 12, 13), new Vector3(14, 15, 16), new Vector3(17, 18, 19)),
                LocalTransformCopy = Matrix4x4.CreateLookTo(new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9)),
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
                var decoded = AttachPoint.Decode(br);
                TmmAssert.AssertEqualFields(input, decoded);
            }
        }

        [Fact]
        public void BoneTest()
        {
            var input = new Bone()
            {
                Name = "TestBone",
                BoneParentIndex = 1,
                Unknown = new[] { 1f, 2f, 3f, 4f },
                GlobalTransform = Matrix4x4.CreateLookTo(new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9)),
                LocalTransform = Matrix4x4.CreateLookTo(new Vector3(11, 12, 13), new Vector3(14, 15, 16), new Vector3(17, 18, 19)),
                InverseGlobalTransform = Matrix4x4.CreateLookTo(new Vector3(21, 22, 23), new Vector3(24, 25, 26), new Vector3(27, 28, 29)),
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
                var decoded = Bone.Decode(br);
                TmmAssert.AssertEqualFields(input, decoded);
            }
        }

        [Fact]
        public void BoundingBoxTest()
        {
            var input = new BoundingBox()
            {
                Min = new Vector3(1.0f, 2.0f, 3.0f),
                Max = new Vector3(4.0f, 5.0f, 6.0f)
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
                var decoded = BoundingBox.Decode(br);
                TmmAssert.AssertEqualFields(input, decoded);
            }
        }

        [Fact]
        public void MaterialVertexGroupTest()
        {
            var input = new MaterialVertexGroup()
            {
                VertexOffset = 0,
                IndexOffset = 1,
                VertexCount = 2,
                IndexCount = 3,
                MaterialIndex = 4,
                ShaderIndex = 5
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
                var decoded = MaterialVertexGroup.Decode(br);
                TmmAssert.AssertEqualFields(input, decoded);
            }
        }


        [Fact]
        public void SubObjectTest()
        {
            var input = new SubObject()
            {
                Name = "TestSubObject",
                WorldTransform = Matrix4x4.CreateLookTo(new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9)),
                InverseWorldTransform = Matrix4x4.CreateLookTo(new Vector3(11, 12, 13), new Vector3(14, 15, 16), new Vector3(17, 18, 19)),
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
                var decoded = SubObject.Decode(br);
                TmmAssert.AssertEqualFields(input, decoded);
            }
        }
    }
}
