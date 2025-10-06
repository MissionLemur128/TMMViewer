using TMMLibrary.TMM;
using TMMLibrary.TMMData;

namespace TMMLibrary;

public class TmmModel(TmmFile headerFile, TmmDataFile dataFile)
{
    public string FilePath { get; set; } = string.Empty;

    public TmmFile HeaderFile => headerFile;

    public TmmDataFile DataFile => dataFile;

    public static TmmModel Decode(TmmModel? existingTmmModel, string filePath)
    {
        if (IsTmmModel(filePath))
        {
            var header = TmmFile.Decode(filePath);
            var data = TmmDataFile.Decode(header.ModelInfo, $"{filePath}.data");
            return new TmmModel(header, data)
            {
                FilePath = filePath
            };
        }
        else
        {
            return TmmAssimpReader.ReadFromFile(existingTmmModel!, filePath);
        }
    }

    public void Encode(string filePath)
    {
        if (IsTmmModel(filePath))
        {
            Update();
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using var binaryWriter = new BinaryWriter(fileStream);
                HeaderFile.Encode(binaryWriter);
            }

            var modelPath = $"{filePath}.data";
            using (var fileStream = new FileStream(modelPath, FileMode.Create, FileAccess.Write))
            {
                using var binaryWriter = new BinaryWriter(fileStream);
                DataFile.Encode(binaryWriter);
            }
        }
        else
        {
            TmmAssimpWriter.WriteToFile(this, filePath);
        }
    }

    private static bool IsTmmModel(string filePath)
    {
        return Path.GetExtension(filePath) == ".tmm";
    }

    public void Update()
    {
        HeaderFile.Update(this);
    }
}
