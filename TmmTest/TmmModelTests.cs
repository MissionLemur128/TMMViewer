using System.Text;
using TMMLibrary;
using TMMLibrary.TMM;
using TMMLibrary.TMMData;
using Xunit;

namespace TmmTest
{
    /// <summary>
    /// Contains tests for encoding and decoding tmm files.
    /// </summary>
    public class TmmModelTests
    {
        // Set to a tmm filepath 
        private const string tmmFilePath = "";
        private const string fbxFilePath = "";

        [Fact]
        public void LoadModelTest()
        {
            if (!File.Exists(tmmFilePath))
            {
                return;
            }

            var input = TmmModel.Decode(null, tmmFilePath);
            var input2 = TmmModel.Decode(null, tmmFilePath);

            byte[] encodedBytes;
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                input2.Update();
                input2.HeaderFile.Encode(bw);
                encodedBytes = ms.ToArray();
            }

            using (var ms = new MemoryStream(encodedBytes))
            using (var br = new BinaryReader(ms))
            {
                var decoded = TmmFile.Decode(br);
                TmmAssert.AssertEqualFields(input.HeaderFile, decoded);
            }
        }

        [Fact]
        public void LoadTMMDataTest()
        {
            if (!File.Exists(tmmFilePath))
            {
                return;
            }

            var input = TmmModel.Decode(null, tmmFilePath);
            var input2 = TmmModel.Decode(null, tmmFilePath);

            byte[] encodedBytes;
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                input2.Update();
                input2.DataFile.Encode(bw);
                encodedBytes = ms.ToArray();
            }

            using (var ms = new MemoryStream(encodedBytes))
            using (var br = new BinaryReader(ms))
            {
                var decoded = TmmDataFile.Decode(input2.HeaderFile.ModelInfo, br);
                TmmAssert.AssertEqualFields(input.DataFile, decoded);
            }
        }

        [Fact]
        public void LoadFBXModelTest()
        {
            if (!File.Exists(tmmFilePath))
            {
                return;
            }

            var input = TmmModel.Decode(null, tmmFilePath);
            var fbxInput = TmmModel.Decode(input, fbxFilePath);
            var input2 = TmmModel.Decode(null, tmmFilePath);
            TmmAssert.AssertEqualFields(input2.HeaderFile, fbxInput.HeaderFile);
        }

        [Fact]
        public void LoadFBXDataTest()
        {
            if (!File.Exists(tmmFilePath))
            {
                return;
            }

            var input = TmmModel.Decode(null, tmmFilePath);
            var fbxInput = TmmModel.Decode(input, fbxFilePath);
            var input2 = TmmModel.Decode(null, tmmFilePath);

            var dataInput = input2.DataFile;
            var fbxDataInput = fbxInput.DataFile;
            TmmAssert.AssertEqualFields(dataInput, fbxDataInput);
        }
    }
}
