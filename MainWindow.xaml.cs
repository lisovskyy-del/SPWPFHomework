using System.Diagnostics;
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

namespace SPWPFHomework
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StringBuilder _primeLogBuffer = new StringBuilder();
        private int _primeUpdateCount = 0;

        private StringBuilder _fibonacciLogBuffer = new StringBuilder();
        private int _fibonacciUpdateCount = 0;

        private const int BatchSize = 100; // Оновлювати кожні 100 чисел

        private CancellationTokenSource _primeCts;
        private CancellationTokenSource _fibCts;

        private ManualResetEventSlim _primePauseEvent = new ManualResetEventSlim(true); // true = початковий стан "відновлено"
        private ManualResetEventSlim _fibonacciPauseEvent = new ManualResetEventSlim(true);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoggerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ExecuteButton_OnClick(object sender, RoutedEventArgs e)
        {
            ExecuteCode();
        }

        private async void ExecuteCode()
        {
            PrimeLoggerTextBox.Clear();
            FibonacciLoggerTextBox.Clear();

            bool isTopSpecified = long.TryParse(TopTextBox.Text, out long top);

            if (!long.TryParse(BottomTextBox.Text, out long bottom) ||
                !long.TryParse(FibonacciTextBox.Text, out long n))
            {
                // Обробка помилки вводу для bottom
                Application.Current.Dispatcher.Invoke(() => LoggerTextBox.Text += "Введіть правильні числа!\n");
                return;
            }

            _primeCts = new CancellationTokenSource();
            _fibCts = new CancellationTokenSource();

            var primeTask = Task.Run(() => ComputePrimes(bottom, isTopSpecified ? top : long.MaxValue, _primeCts.Token), _primeCts.Token);
            var fibonacciTask = Task.Run(() => ComputeFibonacci(n, _fibCts.Token), _fibCts.Token);

            try
            {
                await Task.WhenAll(primeTask, fibonacciTask); // Чекаємо завершення обох
                LoggerTextBox.Text += "Обидва потоки завершено.\n";
            }
            catch (OperationCanceledException)
            {
                LoggerTextBox.Text += "Операція була скасована.\n";
            }
            catch (Exception ex)
            {
                LoggerTextBox.Text += $"Виникла помилка: {ex.Message}\n";
            }
        }

        private void RestartButton_OnClick(object sender, RoutedEventArgs e)
        {
            _primeCts?.Cancel();
            _fibCts?.Cancel();

            _primePauseEvent.Set();
            _fibonacciPauseEvent.Set();

            ExecuteCode();
        }

        private void StopPrimeButton_OnClick(object sender, RoutedEventArgs e)
        {
            _primeCts?.Cancel();
        }

        private void StopFibonacciButton_OnClick(object sender, RoutedEventArgs e)
        {
            _fibCts?.Cancel();
        }

        private void PausePrimeButton_OnClick(object sender, RoutedEventArgs e)
        {
            _primePauseEvent.Reset();
        }

        private void PauseFibonacciButton_OnClick(object sender, RoutedEventArgs e)
        {
            _fibonacciPauseEvent.Reset();
        }

        private void ResumePrimeButton_OnClick(object sender, RoutedEventArgs e)
        {
            _primePauseEvent.Set();
        }

        private void ResumeFibonacciButton_OnClick(object sender, RoutedEventArgs e)
        {
            _fibonacciPauseEvent.Set();
        }

        long ComputePrimes(long bottom, long upTo, CancellationToken token)
        {

            long count = 0;

            if (bottom < 2) // bottom потрібно вказати в
                                            // любому випадку.
                                            // просто зробив перевірку
            {
                bottom = 2;
            }

            for (long n = bottom; n <= upTo; n++)
            {
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }

                _primePauseEvent.Wait(token);
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }

                bool isPrime = true;
                for (long d = 2; d * d <= n; d++)
                {
                    if (n % d == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }

                if (isPrime)
                {
                    count++;

                    lock (_primeLogBuffer)
                    {
                        _primeLogBuffer.AppendLine(n.ToString());
                        _primeUpdateCount++;
                        if (_primeUpdateCount >= BatchSize)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                PrimeLoggerTextBox.AppendText(_primeLogBuffer.ToString());
                                PrimeLoggerTextBox.ScrollToEnd();
                            });
                            _primeLogBuffer.Clear();
                            _primeUpdateCount = 0;
                        }
                    }
                }
            }

            if (_primeLogBuffer.Length > 0)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PrimeLoggerTextBox.AppendText(_primeLogBuffer.ToString());
                    PrimeLoggerTextBox.ScrollToEnd();
                });
                _primeLogBuffer.Clear();
                _primeUpdateCount = 0;
            }

            return count;
        }

        long ComputeFibonacci(long n, CancellationToken token)
        {
            if (n <= 0) return 0;
            if (n == 1) return 1;

            long a = 0;
            long b = 1;
            long c = 0;

            for (int i = 2; i <= n; i++)
            {
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }

                _fibonacciPauseEvent.Wait(token);
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }

                c = a + b;
                a = b;
                b = c;

                lock (_fibonacciLogBuffer)
                {
                    _fibonacciLogBuffer.AppendLine(c.ToString());
                    _fibonacciUpdateCount++;
                    if (_fibonacciUpdateCount >= BatchSize)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            FibonacciLoggerTextBox.AppendText(_fibonacciLogBuffer.ToString());
                            FibonacciLoggerTextBox.ScrollToEnd();
                        });
                        _fibonacciLogBuffer.Clear();
                        _fibonacciUpdateCount = 0;
                    }
                }
            }

            if (_fibonacciLogBuffer.Length > 0)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    FibonacciLoggerTextBox.AppendText(_fibonacciLogBuffer.ToString());
                    FibonacciLoggerTextBox.ScrollToEnd();
                });
                _fibonacciLogBuffer.Clear();
                _fibonacciUpdateCount = 0;
            }

            return c;
        }
    }
}