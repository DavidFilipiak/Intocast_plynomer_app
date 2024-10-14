
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

        public BarChartControl()
        {
            InitializeComponent();
            DataContext = this;

            _observableValues = new ObservableCollection<ObservableValue>();

            Series = new ObservableCollection<ISeries>
            {
                new ColumnSeries<ObservableValue>
                {
                    Values = _observableValues,

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
                MaxLimit = 240,
                MinLimit = 0,
            }
        };  

[RelayCommand]
        public void addColumn(double value)
        {
            double aggregatedValue = actualValues.Sum() + value;
            actualValues.Add(value);
            _observableValues.Add(new(aggregatedValue));

            Console.WriteLine(value);
            Console.WriteLine(actualValues.Count);
            Console.WriteLine(_observableValues.Count);
        }
    }
}
