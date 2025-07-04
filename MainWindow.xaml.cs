using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CafeApp
{
    public partial class MainWindow : Window
    {
        private bool isPhoneValid = false;
        private bool isCodeSent = false;
        private string verificationCode = "";
        private BackgroundWorker loginWorker;
        private bool isLoginCancelled = false;

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}