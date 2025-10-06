using System.Windows;
using TMMViewer.ViewModels;

namespace TMMViewer.Views
{
    public interface IDialogService
    {
        void ShowDialog(string message);
        bool GetOpenFilePath(ref string filename, string filter, ref int filterIndex, string title);
        bool GetSaveFilePath(ref string filename, string filter, ref int filterIndex, string title);
    }

    public class DialogService : IDialogService
    {
        public void ShowDialog(string message)
        {
            MessageBox.Show(message);
        }

        public bool GetOpenFilePath(ref string filename, string filter, ref int filterIndex, string title)
        {
            filename = string.Empty;

            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = filename; // Default file name
            dialog.Filter = filter;// "Text documents (.txt)|*.txt"; // Filter files by extension
            dialog.Title = title;
            dialog.FilterIndex = filterIndex;

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                filename = dialog.FileName;
                filterIndex = dialog.FilterIndex;
            }
            return result == true;
        }

        public bool GetSaveFilePath(ref string filename, string filter, ref int filterIndex, string title)
        {
            // Configure save file dialog box
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = System.IO.Path.GetFileName(filename); // Default file name
            dialog.DefaultDirectory = System.IO.Path.GetDirectoryName(filename);
            dialog.Filter = filter;// "Text documents (.txt)|*.txt"; // Filter files by extension
            dialog.Title = title;
            dialog.FilterIndex = filterIndex;

            // Show save file dialog box
            bool? result = dialog.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                filename = dialog.FileName;
                filterIndex = dialog.FilterIndex;
            }
            return result == true;
        }
    }
}
