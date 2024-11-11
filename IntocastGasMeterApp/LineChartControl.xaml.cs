using IntocastGasMeterApp.services;
using LiveChartsCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using System.Collections.Specialized;

namespace IntocastGasMeterApp
{
    /// <summary>
    /// Interaction logic for LineChartControl.xaml
    /// </summary>
    public partial class LineChartControl : UserControl
    {
        private DataService data;
        private const int MAX_X = 24 * 12;
        private string[] xLabels = new string[MAX_X];

        ObservableCollection<ObservableValue> dataToShow = new ObservableCollection<ObservableValue>();

        // Define the Title dependency property
        public static readonly DependencyProperty ShowDataProperty =
            DependencyProperty.Register("ShowData", typeof(string), typeof(LineChartControl), new PropertyMetadata("halala", OnShowDataChanged));

        // Property wrapper for the dependency property
        // throughput | temperature | pressure
        public string ShowData
        {
            get => (string)GetValue(ShowDataProperty);
            set => SetValue(ShowDataProperty, value);
        }

        public LineChartControl()
        {
            this.data = DataService.GetInstance();           

            InitializeComponent();
            DataContext = this;

            XAxes[0].Labeler = XAxisLabeler;
            LineChart.XAxes = XAxes;
            LineChart.YAxes = YAxes;
        }

        // Callback to detect changes to the ShowData property
        private static void OnShowDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LineChartControl)d;
            var newValue = (string)e.NewValue;

            control.UpdateChart(newValue);
        }

        private void UpdateChart(string showData)
        {
            if (String.Equals(showData, "throughput"))
            {
                dataToShow = this.data.throughput;
            }
            else if (String.Equals(showData, "temperature"))
            {
                dataToShow = this.data.temperature;
            }
            else if (String.Equals(showData, "pressure"))
            {
                dataToShow = this.data.pressure;
            }

            Series = new ObservableCollection<ISeries>
            {
                new LineSeries<ObservableValue>
                {
                    Values = dataToShow,
                    GeometryFill = null,
                    GeometryStroke = null,
                    Fill = null,
                    LineSmoothness = 0.3,
                    Stroke = new SolidColorPaint(new SKColor(0, 0, 0), 1),
                }
            };
        }

        public ObservableCollection<ISeries> Series { get; set; }

        public Axis[] XAxes { get; set; } = new Axis[]
{
            new Axis
            {
                MaxLimit = MAX_X,
                LabelsRotation = 60,
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
            int minutes = ((int)value % 12) * 5;
            int hours = (int)value / 12;
            string minutesString = minutes < 10 ? "0" + minutes.ToString() : minutes.ToString();
            return hours.ToString() + ":" + minutesString;
        }
    }
}
