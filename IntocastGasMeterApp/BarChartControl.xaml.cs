
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
using System.Printing;
using IntocastGasMeterApp.services;
using IntocastGasMeterApp.models;
using LiveChartsCore.Kernel;
using LiveChartsCore.ConditionalDraw;

namespace IntocastGasMeterApp
{
    /// <summary>
    /// Interaction logic for BarChartControl.xaml
    /// </summary>
    public partial class BarChartControl : UserControl
    {
        private readonly ObservableCollection<ObservablePoint> _agreedLine;
        private readonly ObservableCollection<ObservablePoint> _setLine;

        private const int MAX_X = 24 * 12;
        private string[] xLabels = new string[MAX_X];
        private readonly string measureStart = "00:00";

        private DataService data;
        private ApiService api;

        public BarChartControl()
        {
            this.data = DataService.GetInstance();
            this.api = ApiService.GetInstance();
            this.measureStart = Properties.Settings.Default.measure_start;

            InitializeComponent();
            DataContext = this;

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
                    Stroke = new SolidColorPaint(new SKColor(255, 215, 0), 3),
                    XToolTipLabelFormatter = null,
                    YToolTipLabelFormatter = null
                },
                new LineSeries<ObservablePoint>
                {
                    Values = _agreedLine,
                    GeometryFill = null,
                    GeometryStroke = null,
                    Fill = null,
                    Stroke = new SolidColorPaint(new SKColor(255, 0, 0), 3),
                    XToolTipLabelFormatter = null,
                    YToolTipLabelFormatter = null
                },
                new ColumnSeries<ObservableValue>
                {
                    Values = data.AccumulatedUsage,
                    Fill = new SolidColorPaint(new SKColor(0, 0, 255)),
                }.OnPointMeasured(point =>
                {
                    if (point.Visual is null) return;
                    int index = point.Index;
                    Device selectedDevice = Device.Get(api.SelectedDevice);
                    MeasurementsRecord record = selectedDevice.Slots.Values.ElementAt(index);
                    if (record is null || record.IsPartial)
                    {
                        point.Visual.Fill = new SolidColorPaint(new SKColor(84, 84, 84));
                    }
                    else
                    {
                        point.Visual.Fill = new SolidColorPaint(new SKColor(0, 0, 255));
                    }

                })
            };

            XAxes[0].Labeler = XAxisLabeler;
            BarChart.XAxes = XAxes;
            BarChart.YAxes = YAxes;
            BarChart.FontSize = 5;
        }

        public ObservableCollection<ISeries> Series { get; set; }

        public Axis[] XAxes { get; set; } = new Axis[]
        {
            new Axis
            {
                MaxLimit = MAX_X,
                LabelsRotation = 30,
                ShowSeparatorLines = false,
                TextSize = 10,
                CustomSeparators = new double[] {0, 24, 48, 72, 96, 120, 144, 168, 192, 216, 240, 264, 288 },
            }
        };
        public Axis[] YAxes { get; set; } = new Axis[]
        {
            new Axis
            {
                MinLimit = 0,
                TextSize = 12,
            }
        };
        
        private string XAxisLabeler(double value)
        {
            int measureStartHours = Int32.Parse(measureStart.Substring(0, 2));
            value += measureStartHours * 12;
            int minutes = ((int)value % 12) * 5;
            int hours = value >= 24 * 12 ? (int)(value - 24 * 12) / 12 : (int)value / 12;
            string minutesString = minutes < 10 ? "0" + minutes.ToString() : minutes.ToString();

            return hours.ToString() + ":" + minutesString;
        }

        public void addColumn(double value)
        {
            if (data.ActualUsage.Count == MAX_X)
            {
                return;
            }
            double aggregatedValue = 2 + value;
            data.ActualUsage.Add(new(value));
            data.AccumulatedUsage.Add(new(aggregatedValue));
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
