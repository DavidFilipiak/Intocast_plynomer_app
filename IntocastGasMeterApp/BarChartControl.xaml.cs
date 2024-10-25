
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

            BarChart.XAxes = XAxes;
            BarChart.YAxes = YAxes;
        }

        public ObservableCollection<ISeries> Series { get; set; }

        public Axis[] XAxes { get; set; } = new Axis[]
        {
            new Axis
            {
                MaxLimit = 24,
            }
        };
        public Axis[] YAxes { get; set; } = new Axis[]
        {
            new Axis
            {
                //MaxLimit = 240,
                MinLimit = 0,
            }
        };  

        public void addColumn(double value)
        {
            if (actualValues.Count == 24)
            {
                return;
            }
            double aggregatedValue = actualValues.Sum() + value;
            actualValues.Add(value);
            _observableValues.Add(new(aggregatedValue));
        }

        public void SetAgreedLine(double y)
        {
            int x = 24;
            ObservablePoint NewPoint = new ObservablePoint(x, y);
            if (_agreedLine.Count > 1)
            {
                _agreedLine.RemoveAt(1);
            }
            _agreedLine.Add(NewPoint);
        }

        public void SetSetLine(double y)
        {
            int x = 24;
            ObservablePoint NewPoint = new ObservablePoint(x, y);
            if (_setLine.Count > 1)
            {
                _setLine.RemoveAt(1);
            }
            _setLine.Add(NewPoint);
        }
    }
}
