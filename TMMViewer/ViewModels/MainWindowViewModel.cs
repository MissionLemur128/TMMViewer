using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TMMViewer.Data.Services;

namespace TMMViewer.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private IDialogService _dialogService;
        private IModelIOService _modelIO;

        public MainWindowViewModel(IDialogService dialogService, IModelIOService modelIO)
        {
            _dialogService = dialogService;
            _modelIO = modelIO;
        }

        [RelayCommand]
        public void ImportModel()
        {
            var _modelPath = string.Empty;
            var success = _dialogService.GetOpenFilePath(ref _modelPath, ".tmm", "TMM files (.tmm)|*.tmm");
            if (success)
            {
                _modelIO.ImportModel(_modelPath);
            }
        }

        [RelayCommand]
        public void ExportModel()
        {
            // Load the data
        }
    }
}
