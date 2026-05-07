using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cursova_code.Models;


namespace cursova_code.Interpolation
{
    public interface IInterpolator
    {
        string Name { get; }

        double Interpolate(double x, List<PointModel> points);

        List<PointModel> GetCurvePoints(List<PointModel> points, double step);

        string GetAnalyticExpression();
    }
}
