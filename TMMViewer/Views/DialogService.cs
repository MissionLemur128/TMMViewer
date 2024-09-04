using TMMViewer.ViewModels;

namespace TMMViewer.Views
{
    public class DialogService : IDialogService
    {
        public bool GetOpenFilePath(ref string filename, string ext, string filter)
        {
            filename = string.Empty;

            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = filename; // Default file name
            dialog.DefaultExt = ext;// ".txt"; // Default file extension
            dialog.Filter = filter;// "Text documents (.txt)|*.txt"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                filename = dialog.FileName;
            }
            return result == true;
        }

        public bool GetSaveFilePath(ref string filename, string ext, string filter)
        {
            // Configure save file dialog box
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = System.IO.Path.GetFileName(filename); // Default file name
            dialog.DefaultDirectory = System.IO.Path.GetDirectoryName(filename);
            dialog.DefaultExt = System.IO.Path.GetExtension(filename); // Default file extension
            dialog.Filter = filter;// "Text documents (.txt)|*.txt"; // Filter files by extension

            // Show save file dialog box
            bool? result = dialog.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                filename = dialog.FileName;
            }
            return result == true;
        }
    }
}
