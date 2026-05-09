using cursova_code.Interpolation;
using cursova_code.IO;
using cursova_code.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace cursova_code.Desktop
{
    static class Program
    {
        private static List<PointModel> _points = new List<PointModel>();
        private static readonly FileService _fileService = new FileService();
        private static readonly string _filePath = "data.json";
        private static string _lastCalculationResult = "";
        private static string _lastUsedMethod = "";
        private static bool _hasUnsavedResult = false;
        private static object _lastCoefficients = null;

        private static readonly List<IInterpolator> _methods = new List<IInterpolator>
        {
            new NewtonMethod(),
            new SplineModel()
        };

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            InitDefaultData();

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            while (true)
            {
                try
                {
                    Console.Clear();
                    PrintHeader();
                    PrintCurrentPoints();

                    if (_hasUnsavedResult)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($" [!] Є незбережений результат ({_lastUsedMethod})");
                        Console.ResetColor();
                    }

                    Console.WriteLine("\n [1] Ввести нові точки");
                    Console.WriteLine(" [2] Обчислити значення");
                    Console.WriteLine(" [3] Зберегти останній результат у файл (JSON)");
                    Console.WriteLine(" [4] Завантажити вузли з файлу");
                    Console.WriteLine(" [5] Переглянути останній запис в архіві");
                    Console.WriteLine(" [6] Показати графік (Вікно)");
                    Console.WriteLine(" [0] Вийти");
                    Console.Write("\n Обери опцію: ");

                    string choice = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(choice)) continue;

                    switch (choice)
                    {
                        case "1": InputPoints(); break;
                        case "2": CalculateWithValidation(); break;
                        case "3": SaveLastResult(); break;
                        case "4": LoadData(); break;
                        case "5": ShowArchive(); break;
                        case "6": ShowGraphWindow(); break;
                        case "0": return;
                        default:
                            Console.WriteLine("\n [!] Невірна опція. Спробуйте ще раз.");
                            System.Threading.Thread.Sleep(1000);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n====================================================");
                    Console.WriteLine(" [КРИТИЧНА ПОМИЛКА ЗАХИСТУ]");
                    Console.WriteLine($" Повідомлення: {ex.Message}");

                    if (ex.InnerException != null)
                        Console.WriteLine($" Деталі: {ex.InnerException.Message}");

                    Console.WriteLine("====================================================");
                    Console.ResetColor();
                    Console.WriteLine("\n Програма стабілізована. Натисніть будь-яку клавішу...");
                    Console.ReadKey();
                }
            }
        }

        private static void CalculateWithValidation()
        {
            if (_points == null || _points.Count < 2)
            {
                Console.WriteLine("\n [!] Помилка: Недостатньо точок для будь-якого методу (мінімум 2).");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\n Оберіть метод інтерполяції:");
            for (int i = 0; i < _methods.Count; i++)
            {
                Console.WriteLine($" [{i + 1}] {_methods[i].Name}");
            }
            Console.Write(" Ваш вибір: ");

            if (int.TryParse(Console.ReadLine(), out int methodChoice) && methodChoice > 0 && methodChoice <= _methods.Count)
            {
                var method = _methods[methodChoice - 1];
                int count = _points.Count;
                bool isValid = true;

                if (method is NewtonMethod && (count < 2 || count > 20))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n [!] Помилка ТЗ: Метод Ньютона підтримує від 2 до 20 точок. У вас: {count}");
                    isValid = false;
                }
                else if (method is SplineModel && (count < 3 || count > 1000))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n [!] Помилка ТЗ: Кубічні сплайни потребують від 3 до 1000 точок. У вас: {count}");
                    isValid = false;
                }
                Console.ResetColor();

                if (!isValid)
                {
                    Console.ReadKey();
                    return;
                }

                Console.Write("\n Введіть X для інтерполяції: ");
                string inputX = Console.ReadLine()?.Replace(',', '.');

                if (double.TryParse(inputX, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double targetX))
                {
                    double minX = _points.Min(p => p.X);
                    double maxX = _points.Max(p => p.X);

                    if (targetX < minX || targetX > maxX)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($" [!] Попередження: X={targetX} поза межами інтервалу [{minX}; {maxX}].");
                        Console.WriteLine(" Виконується екстраполяція, точність може бути низькою.");
                        Console.ResetColor();
                    }

                    try
                    {
                        Stopwatch sw = Stopwatch.StartNew();

                        double res = method.Interpolate(targetX, _points);
                        string expression = method.GetAnalyticExpression();

                        if (method is SplineModel spline) _lastCoefficients = spline.GetCoefficients();
                        else if (method is NewtonMethod newton) _lastCoefficients = newton.GetDifferenceTable();

                        sw.Stop();

                        _lastUsedMethod = method.Name;
                        _lastCalculationResult = $"X={targetX}: Y={res:G8}. Час: {sw.Elapsed.TotalMilliseconds:G8} ms";
                        _hasUnsavedResult = true;

                        Console.WriteLine("\n--- Результати (успішно пройшли валідацію) ---");
                        Console.WriteLine($" > Метод: {_lastUsedMethod}");
                        Console.WriteLine($" > Результат: Y = {res:G8}");
                        Console.WriteLine($" > Час виконання: {sw.Elapsed.TotalMilliseconds:F4} мс (Ресурсомісткість)");
                        Console.WriteLine($" > Складність: {(method is NewtonMethod ? "O(N?)" : "O(N)")}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\n [!] Помилка обчислень: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine(" [!] Помилка: Невірний формат числа X.");
                }
            }
            Console.ReadKey();
        }

        private static void InputPoints()
        {
            Console.Clear();
            Console.WriteLine(" --- Введення нових точок (Макс. точність: 8 знаків після коми) ---");
            Console.Write(" Скільки точок? ");

            if (int.TryParse(Console.ReadLine(), out int count) && count >= 2 && count <= 100)
            {
                var newPoints = new List<PointModel>();
                for (int i = 0; i < count; i++)
                {
                    Console.Write($" Точка {i + 1} (X Y): ");
                    string input = Console.ReadLine()?.Replace(',', '.');
                    var parts = input?.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts?.Length == 2 &&
                        double.TryParse(parts[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double x) &&
                        double.TryParse(parts[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double y))
                    {
                        if ((parts[0].Contains(".") && parts[0].Split('.')[1].Length > 8) ||
                            (parts[1].Contains(".") && parts[1].Split('.')[1].Length > 8))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(" [!] Попередження: Точність обмежена 8 знаками згідно ТЗ.");
                            Console.ResetColor();
                        }

                        if (newPoints.Any(p => Math.Abs(p.X - x) < 1e-12))
                        {
                            Console.WriteLine(" [!] Помилка: X має бути унікальним!");
                            i--;
                        }
                        else newPoints.Add(new PointModel(x, y));
                    }
                    else
                    {
                        Console.WriteLine(" [!] Невірний формат. Спробуйте ще раз.");
                        i--;
                    }
                }
                _points = newPoints.OrderBy(p => p.X).ToList();
                _hasUnsavedResult = false;
            }
            else
            {
                Console.WriteLine(" [!] Невірна кількість точок (вкажіть від 2 до 100).");
                System.Threading.Thread.Sleep(1000);
            }
        }

        private static void ShowGraphWindow()
        {
            if (_points == null || _points.Count < 2) return;
            using (var form = new Form1(_points)) { form.ShowDialog(); }
        }

        private static void SaveLastResult()
        {
            if (!_hasUnsavedResult) return;
            FileService.SaveResult(_filePath, _points, _lastUsedMethod, _lastCalculationResult, _lastCoefficients);
            _hasUnsavedResult = false;
            Console.WriteLine("\n [OK] Результат в архіві.");
            Console.ReadKey();
        }

        private static void LoadData()
        {
            Console.Write(" Введіть шлях до файлу (JSON): ");
            string path = Console.ReadLine();

            if (!File.Exists(path))
            {
                Console.WriteLine(" [!] Помилка: Файл не знайдено.");
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                {
                    throw new Exception("Файл порожній.");
                }

                var loadedPoints = System.Text.Json.JsonSerializer.Deserialize<List<PointModel>>(json);

                if (loadedPoints == null || loadedPoints.Count < 2)
                {
                    throw new Exception("Недостатньо точок у файлі (мінімум 2).");
                }

                _points = loadedPoints.OrderBy(p => p.X).ToList();
                _hasUnsavedResult = false;
                Console.WriteLine($" [OK] Завантажено {_points.Count} точок.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" [!] Помилка завантаження: {ex.Message}");
            }
            Console.ReadKey();
        }

        private static void ShowArchive()
        {
            var archive = _fileService.LoadAllArchive(_filePath);
            if (archive != null) Console.WriteLine($"\n Метод: {archive.Method}\n {archive.Result}");
            Console.ReadKey();
        }

        private static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("====================================================");
            Console.WriteLine("    ПРОГРАМА ІНТЕРПОЛЯЦІЇ (КПІ ІПІ 2026)          ");
            Console.WriteLine("====================================================");
            Console.ResetColor();
        }

        private static void PrintCurrentPoints()
        {
            Console.WriteLine($" Кількість точок: {_points.Count}");
            if (_points.Count > 0 && _points.Count <= 10)
            {
                foreach (var p in _points) Console.Write($"({p.X:G8}; {p.Y:G8}) ");
                Console.WriteLine();
            }
            else if (_points.Count > 10) Console.WriteLine(" [Забагато точок для відображення в рядку]");
        }

        private static void InitDefaultData()
        {
            _points = new List<PointModel> { new PointModel(0, 0), new PointModel(1, 1), new PointModel(2, 4), new PointModel(3, 9) };
        }
    }
}