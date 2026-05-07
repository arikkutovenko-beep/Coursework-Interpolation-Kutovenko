using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace cursova_code.Models
{
    public class InterpolationResult
    {
        private string _methodName;
        private List<PointModel> _inputPoints;
        private double _targetX;
        private double _targetY;
        private double _executionTimeMs;
        private string _coeficients;

        public InterpolationResult()
        {

        }
        public InterpolationResult(string methodName, List<PointModel> inputPoints)
        {
            _methodName = methodName;
            _inputPoints = inputPoints;
        }

        public string MethodName
        {
            get { return _methodName; }
            set { _methodName = value; }
        }

        public List<PointModel> InputPoints
        {
            get { return _inputPoints; }
            set { _inputPoints = value; }
        }

        public double TargetX
        {
            get { return _targetX; }
            set { _targetX = value; }
        }

        public double TargetY
        {
            get { return _targetY; }
            set { _targetY = value; }
        }

        public double ExecutionTimeMs
        {
            get { return _executionTimeMs; }
            set { _executionTimeMs = value; }
        }

        public string Coeficients
        {
            get { return _coeficients; }
            set { _coeficients = value; }
        }
    }
}
