
using System.Numerics;

namespace TMMLibrary.IOUtils
{
    public class NormalEncoding
    {
        public static void DecodeTangentSpace(byte[] data, out Vector3 normal, out Vector4 tangent)
        {
            float x = GetPackedComponent(data, 0);
            float y = GetPackedComponent(data, 2);
            float z = GetPackedComponent(data, 4);
            float w = MathF.Sqrt(MathF.Max(0.0f, 1.0f - x * x - y * y - z * z));

            var quaternion = new Quaternion(x, y, z, w);
            quaternion = Quaternion.Normalize(quaternion);
            var matrix = Matrix4x4.CreateFromQuaternion(quaternion);   

            normal = Vector3.Normalize(new Vector3(matrix.M31, matrix.M32, matrix.M33));
            var tan3 = Vector3.Normalize(new Vector3(matrix.M11, matrix.M12, matrix.M13));
            float tangentW = ( data[1] & (1 << 7)) == 0 ? 1.0f : -1.0f;
            tangent = new Vector4(tan3.X, tan3.Y, tan3.Z, tangentW);
        }

        public static byte[] EncodeDecodeTangentSpace(Vector3 normal, Vector4 tangent)
        {
            normal = Vector3.Normalize(normal);
            var tangent3 = Vector3.Normalize(new Vector3(tangent.X, tangent.Y, tangent.Z));
            var bitangent = tangent.W * Vector3.Normalize(Vector3.Cross(normal, tangent3));

            var matrix = new Matrix4x4(
                tangent3.X, bitangent.X, normal.X, 0,
                tangent3.Y, bitangent.Y, normal.Y, 0,
                tangent3.Z, bitangent.Z, normal.Z, 0,
                0, 0, 0, 1
            );
            matrix = Matrix4x4.Transpose(matrix);

            var quaterion = Quaternion.CreateFromRotationMatrix(matrix);
            if (quaterion.W < 0) quaterion = -quaterion; // Ensure W is positive for decoding to work correctly
            quaterion = Quaternion.Normalize(quaterion);

            var data = new byte[6];
            SetPackedComponent(quaterion.X, data, 0);
            SetPackedComponent(quaterion.Y, data, 2);
            SetPackedComponent(quaterion.Z, data, 4);

            // Tangent Bit
            var tangentW = tangent.W > 0 ? 0 : 1;
            data[1] &= 0x7F; // Clear the 8th bit
            data[1] |= (byte)(tangentW << 7);
            return data;
        }

        internal static Vector4 GetTangent(Vector3 normal, Vector3 tangent, Vector3 bitangent)
        {
            // Ensure the tangent is orthogonal to the normal
            normal = Vector3.Normalize(normal);
            tangent = Vector3.Normalize(tangent);
            bitangent = Vector3.Normalize(bitangent);

            // Calculate the tangent vector
            var w = Vector3.Dot(Vector3.Cross(normal, tangent), bitangent) < 0 ? 1.0f : -1.0f;
            return new Vector4(tangent.X, tangent.Y, tangent.Z, w);
        }

        private static float GetPackedComponent(byte[] bytes, int startIndex)
        {
            // Data is packed into 15 bit
            const float decodeScale = 1.0f / 16383.0f;
            var value = BitConverter.ToUInt16(bytes, startIndex) & 0x7FFF;
            return value * decodeScale - 1;
        }

        private static void SetPackedComponent(float x, byte[] data, int startIndex)
        {
            // Data is packed into 15 bit
            const float encodeScale = 16383.0f;
            var value = (uint)((x + 1) * encodeScale);
            data[startIndex] = (byte)(value & 0xFF);
            data[startIndex + 1] = (byte)((value >> 8) & 0xFF);
        }
    }
}