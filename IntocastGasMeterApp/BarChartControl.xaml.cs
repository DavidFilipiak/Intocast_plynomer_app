
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.CompilerServices;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.Defaults;
using System.Collections.ObjectModel;
using System.Collections.Immutable;
using CommunityToolkit.Mvvm.Input;

namespace IntocastGasMeterApp
{
    /// <summary>
    /// Interaction logic for BarChartControl.xaml
    /// </summary>
    public partial class BarChartControl : UserControl
    {
        private List<double> actualValues = new List<double>();
        private readonly ObservableCollection<ObservableValue> _observableValues;
        private readonly ObservableCollection<ObservablePoint> _agreedLine;
        private readonly ObservableCollection<ObservablePoint> _setLine;

        private const int MAX_X = 24 * 12;
        private string[] xLabels = new string[MAX_X];

        public BarChartControl()
        {
            InitializeComponent();
            DataContext = this;

            _observableValues = new ObservableCollection<ObservableValue>();
            _agreedLine = new ObservableCollection<ObservablePoint>();
            _agreedLine.Add(new ObservablePoint(0, 0));
            _setLine = new ObservableCollection<ObservablePoint>();
            _setLine.Add(new ObservablePoint(0, 0));

            Series = new ObservableCollection<ISeries>
            {
                new LineSeries<ObservablePoint>
                {
                    Values = _setLine,
                    GeometryFill = null,
                    GeometryStroke = null,
                    Fill = null,
                    Stroke = new SolidColorPaint(new SKColor(255, 215, 0), 3)

                },
                new LineSeries<ObservablePoint>
                {
                    Values = _agreedLine,
                    GeometryFill = null,
                    GeometryStroke = null,
                    Fill = null,
                    Stroke = new SolidColorPaint(new SKColor(255, 0, 0), 3)
                },
                new ColumnSeries<ObservableValue>
                {
                    Values = _observableValues,
                    Fill = new SolidColorPaint(new SKColor(0, 0, 255)),
                }
            };

            XAxes[0].Labeler = XAxisLabeler;
            BarChart.XAxes = XAxes;
            BarChart.YAxes = YAxes;
        }

        public ObservableCollection<ISeries> Series { get; set; }

        public Axis[] XAxes { get; set; } = new Axis[]
        {
            new Axis
            {
                MaxLimit = MAX_X,
                LabelsRotation = 45,
                ShowSeparatorLines = false,
                CustomSeparators = new double[] {0, 12, 24, 36, 48, 60, 72, 84, 96, 108, 120, 132, 144, 156, 168, 180, 192, 204, 216, 228, 240, 252, 264, 276, 288 },
            }
        };
        public Axis[] YAxes { get; set; } = new Axis[]
        {
            new Axis
            {
                MinLimit = 0,
            }
        };
        
        private string XAxisLabeler(double value)
        {
            int minutes = ((int)value % 12) * 5;
            int hours = (int)value / 12;
            string minutesString = minutes < 10 ? "0" + minutes.ToString() : minutes.ToString();
            return hours.ToString() + ":" + minutesString;
        }

        public void addColumn(double value)
        {
            if (actualValues.Count == MAX_X)
            {
                return;
            }
            double aggregatedValue = actualValues.Sum() + value;
            actualValues.Add(value);
            _observableValues.Add(new(aggregatedValue));
        }

        public void SetAgreedLine(double y)
        {
            int x = MAX_X;
            ObservablePoint NewPoint = new ObservablePoint(x, y);
            if (_agreedLine.Count > 1)
            {
                _agreedLine.RemoveAt(1);
            }
            _agreedLine.Add(NewPoint);
        }

        public void SetSetLine(double y)
        {
            int x = MAX_X;
            ObservablePoint NewPoint = new ObservablePoint(x, y);
            if (_setLine.Count > 1)
            {
                _setLine.RemoveAt(1);
            }
            _setLine.Add(NewPoint);
        }
    }
}
