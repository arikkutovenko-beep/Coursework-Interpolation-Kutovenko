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
            BuildDiffTable(points);
            _lastPoints = points;

            List<PointModel> curve = new List<PointModel>();
            if (points.Count == 0) return curve;

            double startX = points[0].X;
            double endX = points[points.Count - 1].X;

            for (double x = startX; x <= endX + (step / 2.0); x += step)
            {
                curve.Add(new PointModel(x, Interpolate(x, points)));
            }
            return curve;
        }

        public string GetAnalyticExpression()
        {
            return "P(x) побудовано на основі розділених різниць.";
        }
    }
}