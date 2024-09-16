using TMMLibrary.Converters;

namespace TMMViewer.Data.Services
{
    public interface IModelIOService
    {
        string? OpenedModelPath { get; }

        void ImportModel(string loadPath);
        void ExportModel(string savePath);
    }
}