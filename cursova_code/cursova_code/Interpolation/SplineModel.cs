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

            for (int i = 1; i< n;i++)
            {
                h[i] = x[i] - x[i - 1];
            }
            double[] c = new double[n];
            double[] alpha = new double[n];
            double[] beta = new double[n];

            c[0] = 0;
            c[n - 1] = 0;

            for(int i = 1; i< n-1;i++)
            {
                double A_mat = h[i];
                double B_mat = 2.0 * (h[i] + h[i + 1]);
                double C_mat = h[i + 1];
                double F_mat = 3.0 * ((y[i + 1] - y[i]) / h[i + 1] - (y[i] - y[i - 1]) / h[i]);


                double m = A_mat * alpha[i - 1] + B_mat;
                alpha[i] = -C_mat / m;
                beta[i] = (F_mat - A_mat * beta[i-1]) / m;
            }

            for(int i = n - 2; i> 0; i--)
            {
                c[i] = alpha[i] * c[i + 1] + beta[i];
            }

            _segments = new List<SplineSegment>();
            for (int i = 0; i<n-1;i++)
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
            BuildSplines(points);
            List<PointModel> curve = new List<PointModel>();
            double startX = points[0].X;
            double endX = points[points.Count - 1].X;

            for (double x_val = startX; x_val <= endX + (step / 2.0); x_val += step)
                curve.Add(new PointModel(x_val, Interpolate(x_val, points)));

            return curve;
        }

        public string GetAnalyticExpression()
        {
            return "S(x) - набір кубічних поліномів для кожного інтервалу.";
        }
    }
}
