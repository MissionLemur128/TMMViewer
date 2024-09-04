namespace TMMViewer.Data.Services
{
    public interface IModelIOService
    {
        void ExportModel(string path, string format);
        void ImportModel(string path);
    }
}