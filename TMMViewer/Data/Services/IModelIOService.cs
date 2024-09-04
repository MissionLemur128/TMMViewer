namespace TMMViewer.Data.Services
{
    public interface IModelIOService
    {
        string? OpenedModelPath { get; }

        void ExportModel(string path, string format);
        void ImportModel(string path);
    }
}