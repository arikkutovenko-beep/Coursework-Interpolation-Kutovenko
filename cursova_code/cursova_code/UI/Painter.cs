using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using cursova_code.Models;

namespace cursova_code.UI
{
    public class Painter
    {
        private readonly Chart _chart;
        private const string AreaName = "ChartArea1";

        public Painter(Chart chart)
        {
            _chart = chart;
            InitChart();
        }

        private void InitChart()
        {
            _chart.Series.Clear();
            _chart.ChartAreas.Clear();
            _chart.Legends.Clear();

            var area = new ChartArea("ChartArea1");
            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.AxisX.Title = "Координата X";
            area.AxisY.Title = "Координата Y";
            _chart.ChartAreas.Add(area);

            var legend = new Legend("Legend1");
            _chart.Legends.Add(legend);
        }

        public void Draw(List<PointModel> nodes, List<PointModel> curve, PointModel target = null)
        {
            _chart.Series.Clear();

            var seriesCurve = new Series("Інтерполяційна крива")
            {
                ChartArea = AreaName,
                ChartType = SeriesChartType.Spline,
                BorderWidth = 3,
                Color = Color.DodgerBlue
            };
            foreach (var p in curve) seriesCurve.Points.AddXY(p.X, p.Y);
            _chart.Series.Add(seriesCurve);

            var seriesNodes = new Series("Вузли (X, Y)")
            {
                ChartArea = AreaName,
                ChartType = SeriesChartType.Point,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 10,
                MarkerColor = Color.Red,
                Label = "#VALY"
            };
            foreach (var p in nodes) seriesNodes.Points.AddXY(p.X, p.Y);
            _chart.Series.Add(seriesNodes);

            if (target != null)
            {
                var seriesTarget = new Series("Результат")
                {
                    ChartArea = AreaName, 
                    ChartType = SeriesChartType.Point,
                    MarkerStyle = MarkerStyle.Star10,
                    MarkerSize = 18,
                    MarkerColor = Color.Gold,
                    BorderColor = Color.Black,
                    BorderWidth = 1
                };
                seriesTarget.Points.AddXY(target.X, target.Y);
                seriesTarget.Points[0].Label = $"Y = {target.Y:F6}";
                _chart.Series.Add(seriesTarget);

                DrawTargetLines(target);
            }
        }

        private void DrawTargetLines(PointModel target)
        {
            var vLine = new Series("VLine")
            {
                ChartArea = AreaName,
                ChartType = SeriesChartType.Line,
                Color = Color.DimGray,
                BorderDashStyle = ChartDashStyle.Dash
            };
            vLine.Points.AddXY(target.X, 0);
            vLine.Points.AddXY(target.X, target.Y);

            var hLine = new Series("HLine")
            {
                ChartArea = AreaName, 
                ChartType = SeriesChartType.Line,
                Color = Color.DimGray,
                BorderDashStyle = ChartDashStyle.Dash
            };
            hLine.Points.AddXY(0, target.Y);
            hLine.Points.AddXY(target.X, target.Y);

            vLine.IsVisibleInLegend = false;
            hLine.IsVisibleInLegend = false;

            _chart.Series.Add(vLine);
            _chart.Series.Add(hLine);
        }
    }
}