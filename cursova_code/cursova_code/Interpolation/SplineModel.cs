using cursova_code.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cursova_code.Interpolation
{
    public class SplineModel : IInterpolator
    {
        public string Name { get { return "Splines"; } }

        public class SplineSegment
        {
            public double A { get; set; }
            public double B { get; set; }
            public double C { get; set; }
            public double D { get; set; }
            public double X { get; set; }

            public SplineSegment(double a, double b, double c, double d, double x)
            {
                A = a;
                B = b;
                C = c;
                D = d;
                X = x;
            }

            public double Calculate(double x)
            {
                double dx = x - X;
                return A + (B + (C / 2.0 + D * dx / 6.0) * dx) * dx;
            }
        }

        private List<SplineSegment> _segments;
        public object GetCoefficients()
        {
            return _segments;
        }

        public double Interpolate(double x, List<PointModel> points)
        {
            if (points == null || points.Count < 3)
                throw new Exception("Для побудови кубічних сплайнів необхідно мінімум 3 точки.");

            if (_segments == null || _segments.Count == 0)
            {
                BuildSplines(points);
            }

            int n = points.Count;

            if (x <= points[0].X) return points[0].Y;
            if (x >= points[n - 1].X) return points[n - 1].Y;

            int segmentIndex = 0;
            for (int i = 0; i < n - 1; i++)
            {
                if (x >= points[i].X && x <= points[i + 1].X)
                {
                    segmentIndex = i;
                    break;
                }
            }
            return _segments[segmentIndex].Calculate(x);
        }

        private void BuildSplines(List<PointModel> points)
        {
            int n = points.Count;

            double[] x = new double[n];
            double[] y = new double[n];

            for (int i = 0; i < n; i++)
            {
                x[i] = points[i].X;
                y[i] = points[i].Y;
            }

            double[] h = new double[n];

            for (int i = 1; i < n; i++)
            {
                h[i] = x[i] - x[i - 1];
                if (h[i] < 1e-12)
                    throw new Exception($"Помилка: занадто мала відстань між вузлами X[{i - 1}] та X[{i}].");
            }

            double[] c = new double[n];
            double[] alpha = new double[n];
            double[] beta = new double[n];

            c[0] = 0;
            c[n - 1] = 0;

            for (int i = 1; i < n - 1; i++)
            {
                double A_mat = h[i];
                double B_mat = 2.0 * (h[i] + h[i + 1]);
                double C_mat = h[i + 1];
                double F_mat = 3.0 * ((y[i + 1] - y[i]) / h[i + 1] - (y[i] - y[i - 1]) / h[i]);

                double m = A_mat * alpha[i - 1] + B_mat;

                if (Math.Abs(m) < 1e-15)
                    throw new Exception("Система рівнянь для сплайнів вироджена (ділення на нуль).");

                alpha[i] = -C_mat / m;
                beta[i] = (F_mat - A_mat * beta[i - 1]) / m;
            }

            for (int i = n - 2; i > 0; i--)
            {
                c[i] = alpha[i] * c[i + 1] + beta[i];
            }

            _segments = new List<SplineSegment>();
            for (int i = 0; i < n - 1; i++)
            {
                int next = i + 1;

                double a_val = y[i];
                double d_val = (c[next] - c[i]) / (3.0 * h[next]);
                double b_val = (y[next] - y[i]) / h[next] - h[next] * (c[next] + 2.0 * c[i]) / 3.0;

                _segments.Add(new SplineSegment(a_val, b_val, c[i], d_val, x[i]));
            }
            _segments.Add(new SplineSegment(y[n - 1], 0, c[n - 1], 0, x[n - 1]));
        }

        public List<PointModel> GetCurvePoints(List<PointModel> points, double step)
        {
            List<PointModel> curve = new List<PointModel>();
            if (points == null || points.Count < 2) return curve;

            BuildSplines(points);

            double startX = points[0].X;
            double endX = points[points.Count - 1].X;
            double range = endX - startX;

            if (step <= 0 || step > range) step = range / 100.0;

            for (double x_val = startX; x_val <= endX + (step / 10.0); x_val += step)
            {
                if (curve.Count > 10000) break;

                double y_res = Interpolate(x_val, points);

                if (!double.IsNaN(y_res) && !double.IsInfinity(y_res))
                {
                    curve.Add(new PointModel(x_val, y_res));
                }
            }

            return curve;
        }

        public string GetAnalyticExpression()
        {
            return "S(x) - набір кубічних поліномів для кожного інтервалу.";
        }
    }
}