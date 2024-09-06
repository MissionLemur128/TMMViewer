using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Windows.Interop;
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

        public MainWindowViewModel()
        {
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
            var success = _dialogService.GetOpenFilePath(ref _modelPath, ".tmm", "TMM files (.tmm)|*.tmm");
            if (success)
            {
                _modelIO.ImportModel(_modelPath);
            }
        }

        [RelayCommand]
        public void ExportModel()
        {
            var _modelPath = Path.GetFileNameWithoutExtension(_modelIO.OpenedModelPath);
            var success = _dialogService.GetSaveFilePath(ref _modelPath, ".obj", "OBJ files (.obj)|*.obj");
            if (success)
            {
                _modelIO.ExportModel(_modelPath, _modelExportMapping[Path.GetExtension(_modelPath)]);
            }
        }

        [RelayCommand]
        public void ExportModelDebug()
        {
            var _modelPath = Path.GetFileNameWithoutExtension(_modelIO.OpenedModelPath);
            var success = _dialogService.GetSaveFilePath(ref _modelPath, ".tmm.data", "tmm.data files (.tmm.data)|*.tmm.data");
            if (success)
            {
                _modelIO.ExportModelDebug(_modelPath);
            }
        }

        private Dictionary<string, string> _modelExportMapping = new()
        {
            { ".obj", "objnomtl" },
            { ".fbx", "FBX" },
            { ".dae", "Collada" },
            { ".gltf", "glTF" },
            { ".glb", "glTF Binary" },
        };
    }
}
