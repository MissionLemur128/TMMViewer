namespace TMMViewer.ViewModels
{
    public interface IDialogService
    {
        bool GetOpenFilePath(ref string filename, string ext, string filter);
        bool GetSaveFilePath(ref string filename, string ext, string filter);
    }
}