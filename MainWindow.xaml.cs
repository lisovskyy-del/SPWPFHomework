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
        private CancellationTokenSource _primeCts;
        private CancellationTokenSource _fibCts;

        private bool _primeIsPaused = true; // воно на true для того щоб показати як працює пауза.
        private bool _fibonacciIsPaused = true;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoggerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ExecuteButton_OnClick(object sender, RoutedEventArgs e)
        {
            _primeCts = new CancellationTokenSource();
            _fibCts = new CancellationTokenSource();

            LoggerTextBox.Clear();

            if (!long.TryParse(BottomTextBox.Text, out long bottom) ||
                !long.TryParse(TopTextBox.Text, out long top) ||
                !long.TryParse(PrimeTextBox.Text, out long n))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoggerTextBox.Text += "\nВведіть правильні числа!";
                });

                return;
            }

            var primeThread = new Thread(() =>
            {

                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoggerTextBox.Text += "Починаємо підрахунок простих чисел...";
                });

                try
                {
                    var sw = Stopwatch.StartNew();
                    long result = ComputePrimes(bottom, top, _primeCts.Token);

                    sw.Stop();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LoggerTextBox.Text += $"\nРезультат: {result} простих чисел за {sw.ElapsedMilliseconds}ms";


                        // Запуск Fibonacci після того, як прості числа завершились, виглядає костильно но що поробиш.
                        var fibonacciThread = new Thread(() =>
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                LoggerTextBox.Text += "\nПочинаємо підрахунок чисел фібоначчі...";
                            });

                            var sw2 = Stopwatch.StartNew();
                            long result = ComputeFibonacci(n, _fibCts.Token);

                            sw2.Stop();

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                LoggerTextBox.Text += $"\nРезультат: {result} за {sw2.ElapsedMilliseconds}ms";
                            });
                        });

                        fibonacciThread.IsBackground = true; // без цього буде deadlock
                        fibonacciThread.Start();
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nПомилка: {ex}");
                }
            });

            primeThread.IsBackground = true; // без цього буде deadlock
            primeThread.Start();
        }

        private void RestartButton_OnClick(object sender, RoutedEventArgs e)
        {
            _primeCts?.Cancel();
            _fibCts?.Cancel();

            _primeCts = new CancellationTokenSource();
            _fibCts = new CancellationTokenSource();

            _primeIsPaused = false;
            _fibonacciIsPaused = false;

            ExecuteButton_OnClick(sender, e);
        }

        private void SuspendPrimeButton_OnClick(object sender, RoutedEventArgs e)
        {
            _primeCts?.Cancel();
        }

        private void SuspendFibonacciButton_OnClick(object sender, RoutedEventArgs e)
        {
            _fibCts?.Cancel();
        }

        private void StopPrimeButton_OnClick(object sender, RoutedEventArgs e)
        {
            _primeIsPaused = true;
        }

        private void ResumePrimeButton_OnClick(object sender, RoutedEventArgs e)
        {
            _primeIsPaused = false;
        }

        private void StopFibonacciButton_OnClick(object sender, RoutedEventArgs e)
        {
            _fibonacciIsPaused = true;
        }

        private void ResumeFibonacciButton_OnClick(object sender, RoutedEventArgs e)
        {
            _fibonacciIsPaused = false;
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
                    break;
                }

                while (_primeIsPaused)
                {
                    Thread.Sleep(100);
                }

                bool isPrime = true;
                for (long d = bottom; d * d <= n; d++)
                {
                    if (n % d == 0) { isPrime = false; break; }
                }
                if (isPrime)
                {
                    count++;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LoggerTextBox.Text += $"\n{n}";
                        LoggerTextBox.ScrollToEnd(); // авто-скрол вниз
                    });

                    Thread.Sleep(10); // для ефективності можна прибрати
                }
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
                    break;
                }

                while (_fibonacciIsPaused)
                {
                    Thread.Sleep(100);
                }

                c = a + b;
                a = b;
                b = c;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoggerTextBox.Text += $"\n{c}";
                    LoggerTextBox.ScrollToEnd(); // авто-скрол вниз
                });

                Thread.Sleep(10); // для ефективності можна прибрати
            }

            return c;
        }
    }
}