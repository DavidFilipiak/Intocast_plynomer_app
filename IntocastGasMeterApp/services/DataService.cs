using LiveChartsCore.Defaults;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntocastGasMeterApp.services
{
    class DataService
    {

        private static DataService instance = null;

        public readonly ObservableCollection<ObservableValue> accumulatedUsage;
        public readonly ObservableCollection<ObservableValue> actualUsage;
        public readonly ObservableCollection<ObservableValue> throughput;
        public readonly ObservableCollection<ObservableValue> temperature;
        public readonly ObservableCollection<ObservableValue> pressure;

        private DataService()
        {
            this.accumulatedUsage = new ObservableCollection<ObservableValue>();
            this.actualUsage = new ObservableCollection<ObservableValue>();
            this.throughput = new ObservableCollection<ObservableValue>();
            this.temperature = new ObservableCollection<ObservableValue>();
            this.pressure = new ObservableCollection<ObservableValue>();
        }

        public static DataService GetInstance()
        {
            if (instance == null)
            {
                instance = new DataService();
            }
            return instance;
        }
    }
}
