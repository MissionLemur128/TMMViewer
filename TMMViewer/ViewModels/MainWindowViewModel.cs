using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using TMMLibrary.Converters;
using TMMViewer.Data.Render;
using TMMViewer.Data.Services;
using RenderMode = TMMViewer.Data.RenderMode;

namespace TMMViewer.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private IDialogService _dialogService;
        private IModelIOService _modelIO;
        private Scene _scene;

        public RenderMode[] RenderModes => Enum.GetValues<RenderMode>();

        public RenderMode SelectedRenderMode
        {
            get => _scene.RenderMode;
            set => _scene.RenderMode = value;
        }

        public MainWindowViewModel(IDialogService dialogService, IModelIOService modelIO, Scene scene)
        {
            _dialogService = dialogService;
            _modelIO = modelIO;
            _scene = scene;
        }

        [RelayCommand]
        public void ImportModel()
        {
            var _modelPath = string.Empty;
            var extensions = SupportedFiles.Import.Select(x => x.Extension).Aggregate((a, b) => $"{a}|{b}");
            var filters = SupportedFiles.Import.Select(x => x.Filter).Aggregate((a, b) => $"{a}|{b}");

            var success = _dialogService.GetOpenFilePath(ref _modelPath, extensions, filters);
            if (success)
            {
                _modelIO.ImportModel(_modelPath);
            }
        }

        [RelayCommand]
        public void ExportModel()
        {
            var _modelPath = Path.GetFileNameWithoutExtension(_modelIO.OpenedModelPath) ?? string.Empty;
            var extensions = SupportedFiles.Export.Select(x => x.Extension).Aggregate((a,b) => $"{a}|{b}");
            var filters = SupportedFiles.Export.Select(x => x.Filter).Aggregate((a, b) => $"{a}|{b}");

            var success = _dialogService.GetSaveFilePath(ref _modelPath, extensions, filters);
            if (success)
            {
                _modelIO.ExportModel(_modelPath);
            }
        }
    }
}
