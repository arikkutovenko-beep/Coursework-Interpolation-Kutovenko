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
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            while (true)
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
                Console.WriteLine(" [2] Обчислити значення (у пам'ять)");
                Console.WriteLine(" [3] Зберегти останній результат у файл (JSON)");
                Console.WriteLine(" [4] Завантажити вузли з файлу");
                Console.WriteLine(" [5] Переглянути останній запис в архіві");
                Console.WriteLine(" [6] Показати графік (Вікно)");
                Console.WriteLine(" [0] Вийти");
                Console.Write("\n Обери опцію: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": InputPoints(); break;
                    case "2": CalculateOnly(); break;
                    case "3": SaveLastResult(); break;
                    case "4": LoadData(); break;
                    case "5": ShowArchive(); break;
                    case "6": ShowGraphWindow(); break;
                    case "0": return;
                }
            }
        }

        private static void ShowGraphWindow()
        {
            if (_points == null || _points.Count < 2)
            {
                Console.WriteLine("\n [!] Недостатньо точок для побудови графіка.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\n Запуск графічного модуля... Зачекайте.");

            using (var form = new Form1(_points))
            {
                form.ShowDialog();
            }
        }

        private static object _lastCoefficients = null;

        private static void CalculateOnly()
        {
            if (_points == null || _points.Count < 2)
            {
                Console.WriteLine("\n [!] Спочатку введіть або завантажте точки!");
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

                Console.Write("\n Введіть значення X для інтерполяції: ");
                if (double.TryParse(Console.ReadLine(), out double targetX))
                {
                    try
                    {
                        double res = method.Interpolate(targetX, _points);
                        string expression = method.GetAnalyticExpression();

                        if (method is SplineModel spline) _lastCoefficients = spline.GetCoefficients();
                        else if (method is NewtonMethod newton) _lastCoefficients = newton.GetDifferenceTable();

                        _lastUsedMethod = method.Name;
                        _lastCalculationResult = $"X={targetX}: Y={res:F6}. Формула: {expression}";
                        _hasUnsavedResult = true;

                        Console.WriteLine("\n--- Результати обчислень (у пам'яті) ---");
                        Console.WriteLine($" > Метод: {_lastUsedMethod}");
                        Console.WriteLine($" > Результат: Y = {res:F6}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\n [!] Помилка: {ex.Message}");
                    }
                }
            }
            Console.ReadKey();
        }

        private static void SaveLastResult()
        {
            if (!_hasUnsavedResult)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n [!] Немає нових результатів!");
                Console.ResetColor();
            }
            else
            {
                FileService.SaveResult(_filePath, _points, _lastUsedMethod, _lastCalculationResult, _lastCoefficients);
                _hasUnsavedResult = false;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n [OK] Розрахунок успішно заархівовано з коефіцієнтами!");
                Console.ResetColor();
            }
            Console.ReadKey();
        }

        private static void LoadData()
        {
            _points = _fileService.LoadPoints(_filePath);

            if (_points.Any())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n [OK] Дані успішно завантажені!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n [!] Файл порожній або не знайдений.");
            }

            Console.ResetColor();
            Console.WriteLine(" Натисніть будь-яку клавішу...");
            Console.ReadKey();
        }

        private static void ShowArchive()
        {
            var archive = _fileService.LoadAllArchive(_filePath);
            if (archive != null)
            {
                Console.WriteLine($"\n--- Архівний запис від {archive.CalculationDate} ---");
                Console.WriteLine($" Метод: {archive.Method}");
                Console.WriteLine($" Опис: {archive.Result}");
            }
            else Console.WriteLine(" Архів не знайдено.");
            Console.ReadKey();
        }

        private static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("====================================================");
            Console.WriteLine("    ГІБРИДНИЙ ЗАСТОСУНОК (КОНСОЛЬ + ВІКНА)       ");
            Console.WriteLine("====================================================");
            Console.ResetColor();
        }

        private static void PrintCurrentPoints()
        {
            Console.WriteLine(" Поточні точки (вузли):");
            if (_points.Count == 0) Console.WriteLine(" [Порожньо]");
            else
            {
                foreach (var p in _points)
                    Console.Write($"({p.X:F2}; {p.Y:F2}) ");
                Console.WriteLine();
            }
        }

        private static void InputPoints()
        {
            Console.Clear();
            Console.WriteLine(" --- Введення нових точок ---");
            Console.Write(" Скільки точок? ");

            if (int.TryParse(Console.ReadLine(), out int count) && count > 1)
            {
                var newPoints = new List<PointModel>();
                for (int i = 0; i < count; i++)
                {
                    Console.Write($" Точка {i + 1} (X Y): ");
                    var input = Console.ReadLine();
                   
                    var parts = input?.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts?.Length == 2 && double.TryParse(parts[0], out double x) && double.TryParse(parts[1], out double y))
                    {
                        
                        if (newPoints.Any(p => Math.Abs(p.X - x) < 1e-9))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(" [!] Помилка: Вузли повинні бути унікальними!");
                            Console.ResetColor();
                            i--; 
                        }
                        else
                        {
                            newPoints.Add(new PointModel(x, y));
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(" [!] Помилка! Введіть дійсне число!");
                        Console.ResetColor();
                        i--;
                    }
                }
                _points = newPoints.OrderBy(p => p.X).ToList();
            }
        }

        private static void InitDefaultData()
        {
            _points = new List<PointModel>
            {
                new PointModel(0, 0), new PointModel(1, 1),
                new PointModel(2, 4), new PointModel(3, 9)
            };
        }
    }
}