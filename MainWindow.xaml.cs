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
            InitializeBackgroundWorker();
        }

        private void InitializeBackgroundWorker()
        {
            loginWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            loginWorker.DoWork += LoginWorker_DoWork;
            loginWorker.ProgressChanged += LoginWorker_ProgressChanged;
            loginWorker.RunWorkerCompleted += LoginWorker_RunWorkerCompleted;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Гарантируем, что все элементы инициализированы
            if (EmployeePanel == null || ClientPanel == null)
            {
                MessageBox.Show("Критическая ошибка: не удалось загрузить элементы интерфейса",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void UserType_Checked(object sender, RoutedEventArgs e)
        {
            if (EmployeePanel == null || ClientPanel == null) return;

            if (rbEmployee.IsChecked == true)
            {
                EmployeePanel.Visibility = Visibility.Visible;
                ClientPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmployeePanel.Visibility = Visibility.Collapsed;
                ClientPanel.Visibility = Visibility.Visible;
            }
        }

        private void PhoneNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);

            // Блокировка ввода 8 или +7 в начале
            if (txtPhoneNumber.Text.Length == 0 && (e.Text == "8" || e.Text == "+" || e.Text == "7"))
            {
                e.Handled = true;
            }
        }

        private void VerificationCode_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void btnSendCode_Click(object sender, RoutedEventArgs e)
        {
            string phoneNumber = txtPhoneNumber.Text?.Trim() ?? "";

            if (phoneNumber.Length < 10)
            {
                MessageBox.Show("Номер телефона должен содержать не менее 10 цифр",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            isPhoneValid = CheckPhoneInDatabase(phoneNumber);

            if (!isPhoneValid)
            {
                var result = MessageBox.Show("Номер не найден в системе. Продолжить?",
                                          "Предупреждение",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) return;
            }

            // Генерация кода
            var rnd = new Random();
            verificationCode = rnd.Next(1000, 9999).ToString();

            // Показ окна с кодом
            var codeWindow = new CodeWindow(verificationCode)
            {
                Owner = this
            };
            codeWindow.Show();

            // Активация полей для ввода кода
            isCodeSent = true;
            lblCode.Visibility = Visibility.Visible;
            txtVerificationCode.Visibility = Visibility.Visible;
            txtVerificationCode.Text = "";
        }

        private bool CheckPhoneInDatabase(string phoneNumber)
        {
            // В реальной системе здесь будет запрос к БД
            // Для примера считаем валидным номер, содержащий "123"
            return phoneNumber.Contains("123");
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (rbEmployee.IsChecked == true)
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text)
                    || string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    ShowError("Введите логин и пароль");
                    return;
                }
            }
            else
            {
                if (!isCodeSent || string.IsNullOrWhiteSpace(txtVerificationCode.Text))
                {
                    ShowError("Введите код подтверждения");
                    return;
                }

                if (txtVerificationCode.Text != verificationCode)
                {
                    ShowError("Неверный код подтверждения");
                    return;
                }
            }

            StartLoginProcess();
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void StartLoginProcess()
        {
            progressBar.Visibility = Visibility.Visible;
            txtProgress.Visibility = Visibility.Visible;
            btnCancel.Visibility = Visibility.Visible;
            btnLogin.IsEnabled = false;

            isLoginCancelled = false;
            progressBar.Value = 0;
            loginWorker.RunWorkerAsync();
        }

        private void LoginWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 1; i <= 10; i++)
            {
                if (loginWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                Thread.Sleep(300);
                loginWorker.ReportProgress(i * 10, $"Загрузка: {i}/10");
            }
        }

        private void LoginWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            txtProgress.Text = e.UserState?.ToString();
        }

        private void LoginWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                progressBar.Foreground = System.Windows.Media.Brushes.Red;
                txtProgress.Text = "Отменено";
            }
            else if (e.Error != null)
            {
                new ErrorWindow($"Ошибка: {e.Error.Message}").ShowDialog();
            }
            else
            {
                OpenMainApplicationWindow();
            }

            ResetLoginUI();
        }

        private void OpenMainApplicationWindow()
        {
            var mainWindow = new MainAppWindow();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();

            foreach (Window window in Application.Current.Windows)
            {
                if (window != mainWindow)
                {
                window.Close();
                }
            }
        }

        private void ResetLoginUI()
        {
            progressBar.Visibility = Visibility.Collapsed;
            txtProgress.Visibility = Visibility.Collapsed;
            btnCancel.Visibility = Visibility.Collapsed;
            btnLogin.IsEnabled = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            isLoginCancelled = true;
            loginWorker.CancelAsync();
        }
    }
}