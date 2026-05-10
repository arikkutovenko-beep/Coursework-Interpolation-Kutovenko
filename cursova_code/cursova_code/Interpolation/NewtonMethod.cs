using System;
using System.Collections.Generic;
using System.Linq;
using cursova_code.Models;

namespace cursova_code.Interpolation
{
    public class NewtonMethod : IInterpolator
    {
        public string Name { get { return "Interpolation method Newton"; } }

        private double[,]? _diffTable;
        private List<PointModel> _lastPoints;

        public object GetDifferenceTable()
        {
            return _diffTable;
        }

        public double Interpolate(double x, List<PointModel> points)
        {
            int n = points.Count;
            if (n == 0) return 0;

            if (_diffTable == null || _lastPoints != points)
            {
                BuildDiffTable(points);
                _lastPoints = points;
            }

            double result = _diffTable[0, n - 1];
            for (int i = n - 2; i >= 0; i--)
            {
                result = _diffTable[0, i] + (x - points[i].X) * result;
            }
            return result;
        }

        private void BuildDiffTable(List<PointModel> points)
        {
            int n = points.Count;
            _diffTable = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                _diffTable[i, 0] = points[i].Y;
            }

            for (int j = 1; j < n; j++)
            {
                for (int i = 0; i < n - j; i++)
                {
                    double denominator = points[i + j].X - points[i].X;
                    if (Math.Abs(denominator) < 1e-15)
                    {
                        throw new DivideByZeroException($"Вузли інтерполяції {i} та {i + j} збігаються або надто близькі (X={points[i].X}). Розрахунок неможливий.");
                    }

                    _diffTable[i, j] = (_diffTable[i + 1, j - 1] - _diffTable[i, j - 1]) / denominator;
                }
            }
        }

        public List<PointModel> GetCurvePoints(List<PointModel> points, double step)
        {
            List<PointModel> curve = new List<PointModel>();
            if (points == null || points.Count < 2) return curve;

            BuildDiffTable(points);
            _lastPoints = points;

            double startX = points[0].X;
            double endX = points[points.Count - 1].X;
            double range = endX - startX;

            if (step <= 0 || step > range) step = range / 100.0;

            for (double x = startX; x <= endX + (step / 10.0); x += step)
            {
                if (curve.Count > 10000) break;

                double y = Interpolate(x, points);

                if (!double.IsNaN(y) && !double.IsInfinity(y))
                {
                    curve.Add(new PointModel(x, y));
                }
            }
            return curve;
        }

        public string GetAnalyticExpression()
        {
            return "P(x) побудовано на основі розділених різниць.";
        }
    }
}