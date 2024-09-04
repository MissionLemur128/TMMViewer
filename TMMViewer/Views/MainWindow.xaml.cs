using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TMMViewer.ViewModels;
using TMMViewer.ViewModels.MonoGameControls;

namespace TMMViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(MainWindowViewModel viewModel, IMonoGameViewModel modelViewer)
        {
            InitializeComponent();
            DataContext = viewModel;
            Viewer3D.DataContext = modelViewer;
        }
    }
}