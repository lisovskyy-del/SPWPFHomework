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
        long bottom;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoggerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ExecuteButton_OnClick(object sender, RoutedEventArgs e)
        {
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
                    long result = ComputePrimes(bottom, top);

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
                            long result = ComputeFibonacci(n);

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

        long ComputePrimes(long bottom, long upTo)
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
                }
            }
            return count;
        }

        long ComputeFibonacci(long n)
        {
            if (n <= 0) return 0;
            if (n == 1) return 1;

            long a = 0;
            long b = 1;
            long c = 0;

            for (int i = 2; i <= n; i++)
            {
                c = a + b;
                a = b;
                b = c;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoggerTextBox.Text += $"\n{c}";
                    LoggerTextBox.ScrollToEnd(); // авто-скрол вниз
                });
            }

            return c;
        }
    }
}