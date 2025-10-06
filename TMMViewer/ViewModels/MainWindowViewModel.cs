using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using TMMLibrary;
using TMMViewer.Data.Render;
using TMMViewer.Data.Render.Nodes;
using TMMViewer.ViewModels.MonoGameControls;
using TMMViewer.Views;
using RenderMode = TMMViewer.Data.RenderMode;

namespace TMMViewer.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public static RenderMode[] RenderModes => Enum.GetValues<RenderMode>();

        public ModelViewer ModelViewer { get; private set; }

        public TmmModel? TmmModel { get; private set; }

        public Scene Scene
        {
            get => _scene;
            private set
            {
                _scene = value;
                OnPropertyChanged(nameof(Scene));
                OnPropertyChanged(nameof(SceneNodes));
                OnPropertyChanged(nameof(SelectedRenderMode));
            }
        }
        private Scene _scene = Scene.CreateEmpty();

        public RenderMode SelectedRenderMode
        {
            get => Scene.RenderMode;
            set => Scene.RenderMode = value;
        }

        public ICollection<ISceneNode> SceneNodes => Scene.Children;

        private readonly IDialogService _dialogService;
        private int _lastUsedExtensionIndex = 0;

        public MainWindowViewModel(IDialogService dialogService)
        {
            this._dialogService = dialogService;
            ModelViewer = new ModelViewer(this);
        }

        [RelayCommand]
        public void ImportModel()
        {
            try
            {
                // Lock input to prevent accidental user interaction during import
                ModelViewer.LockInput = true;
                var _modelPath = string.Empty;
                var filters = SupportedFiles.Import.Aggregate((a, b) => $"{a}|{b}");

                const string title = "Import File";
                var success = _dialogService.GetOpenFilePath(ref _modelPath, filters, ref _lastUsedExtensionIndex, title);
                if (success)
                {
                    TmmModel = TmmModel.Decode(TmmModel, _modelPath);
                    Scene = Scene.CreateFromTmmModel(TmmModel, ModelViewer);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowDialog(ex.ToString());
            }
            finally
            {
                ModelViewer.LockInput = false;
            }
        }

        [RelayCommand]
        public void ExportModel()
        {
            if (TmmModel == null)
                return;

            try
            {
                // Lock input to prevent accidental user interaction during export
                ModelViewer.LockInput = true;
                var _modelPath = Path.GetFileNameWithoutExtension(TmmModel.FilePath) ?? string.Empty;
                var filters = SupportedFiles.Export.Aggregate((a, b) => $"{a}|{b}");

                const string title = "Export File";
                var success = _dialogService.GetSaveFilePath(ref _modelPath, filters, ref _lastUsedExtensionIndex, title);
                if (success)
                {
                    TmmModel.Encode(_modelPath);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowDialog(ex.ToString());
            }
            finally
            {
                ModelViewer.LockInput = false;
            }
        }
    }
}
