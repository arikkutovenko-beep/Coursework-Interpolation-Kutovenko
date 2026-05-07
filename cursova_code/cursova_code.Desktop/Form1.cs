using System;
using System.Collections.Generic;
using System.Windows.Forms;
using cursova_code.UI;      // для Painter
using cursova_code.Models;  // для PointModel
using cursova_code.Interpolation; // для NewtonMethod

namespace cursova_code.Desktop
{
    public partial class Form1 : Form
    {
        private Painter _painter;
        private List<PointModel> _nodes;

        // Конструктор за замовчуванням (якщо захочеш запустити окремо)
        public Form1()
        {
            InitializeComponent();
            _painter = new Painter(chart1);
            _nodes = new List<PointModel>();
        }

        // КОРЕКТНИЙ КОНСТРУКТОР ДЛЯ КПІ: приймає точки з консолі
        public Form1(List<PointModel> nodes)
        {
            InitializeComponent();
            _nodes = nodes;
            _painter = new Painter(chart1);

            // Автоматично малюємо графік при завантаженні форми
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DrawGraph();
        }

        private void btnDraw_Click(object sender, EventArgs e)
        {
            // Дозволяємо перемалювати або знайти конкретну точку X
            DrawGraph();
        }

        private void DrawGraph()
        {
            try
            {
                if (_nodes == null || _nodes.Count < 2)
                {
                    // Якщо точок немає, можна використати твої тестові
                    _nodes = new List<PointModel>
                    {
                        new PointModel(0, 2),
                        new PointModel(2, 5),
                        new PointModel(4, 1),
                        new PointModel(6, 4)
                    };
                }

                var method = new NewtonMethod();

                // Генеруємо криву (крок 0.1 для гладкості)
                var curve = method.GetCurvePoints(_nodes, 0.1);

                // Перевіряємо, чи ввів користувач X для пошуку значення
                PointModel target = null;
                if (txtX != null && double.TryParse(txtX.Text, out double x))
                {
                    double y = method.Interpolate(x, _nodes);
                    target = new PointModel(x, y);
                }

                // Викликаємо метод малювання з нашого Painter
                _painter.Draw(_nodes, curve, target);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка візуалізації: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}