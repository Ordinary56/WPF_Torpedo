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

namespace WPF_Torpedo
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
        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Starting the game...");
        }

        private void InstructionsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Útmutató:\n\n- A játékosok felváltva próbálják kitalálni az ellenfél hajóinak helyét.\n- Találatot akkor jegyeznek fel, amikor egy játékos sikeresen kitalálja egy hajó helyét.\n- A játék véget ér, amikor az egyik játékos összes hajója elsüllyedt.");
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}