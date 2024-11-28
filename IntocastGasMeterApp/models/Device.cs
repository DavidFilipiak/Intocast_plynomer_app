using IntocastGasMeterApp.services;
using LiveChartsCore.Defaults;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Provider;
using System.Windows.Documents;

namespace IntocastGasMeterApp.models
{
    class Device
    {
        public static List<Device> devices = new List<Device>();
        public static Device Get(string deviceNumber)
        {
            foreach (Device device in devices)
            {
                if (device.DeviceNumber == deviceNumber)
                {
                    return device;
                }
            }
            return null;
        }

        public static Device Combine(Device[] devices)
        {
            Device newDevice = new Device("combined", "combined");
            Device sample = devices[0];

            // check that all devices must have the same number of records
            foreach (Device device in devices)
            {
                if (device.NumberOfRecords != sample.NumberOfRecords)
                {
                    throw new Exception("Devices must have the same number of records");
                }
            }

            for (int i = 0; i < sample.NumberOfRecords; i++)
            {
                double accumulatedUsage = 0;
                double actualUsage = 0;
                //double throughput = 0;
                double temperature = 0;
                double pressure = 0;

                foreach (Device device in devices)
                {
                    accumulatedUsage += device.AccumulatedUsage[i];
                    actualUsage += (double)device.ActualUsage[i];
                    //throughput += (double)device.Throughput[i];
                    temperature += (double)device.Temperature[i];
                    pressure += (double)device.Pressure[i];
                }

                newDevice.AccumulatedUsage.Add(accumulatedUsage);
                newDevice.ActualUsage.Add(actualUsage);
                //newDevice.Throughput.Add(throughput);
                newDevice.Temperature.Add(temperature);
                newDevice.Pressure.Add(pressure);
            }

            return newDevice;
        }


        public string DeviceNumber { get; set; }
        public string CustomerId { get; set; }

        private readonly List<double> _accumulatedUsage;
        private readonly List<double> _actualUsage;
        private readonly List<double> _throughput;
        private readonly List<double> _temperature;
        private readonly List<double> _pressure;

        public readonly List<DateTime> _measuredTimes;

        private const int NUMBER_OF_TIMESLOTS = 24 * 12; // number of 5-minue slots in a day
        public readonly Dictionary<DateTime, MeasurementsRecord> _slots;
        private DateTime LastOccupiedSlot;


        public List<double> AccumulatedUsage => GetProperty(record => record.AccumulatedUsage);
        public List<double> ActualUsage => GetProperty(record => record.ActualUsage, false);
        public List<double> Throughput { get { return new List<double>(); } }
        public List<double> Temperature => GetProperty(record => record.Temperature);
        public List<double> Pressure => GetProperty(record => record.Pressure);

        public int NumberOfRecords { get
            {
                // the index last slot which is not null
                return _slots.Keys.ToList().IndexOf(LastOccupiedSlot) + 1;
            }
        }

        private readonly Dictionary<DateTime, string> _changeLog;
        public bool IsActive { get; set; }

        public Device(string deviceNumber, string customerId)
        {
            DeviceNumber = deviceNumber;
            CustomerId = customerId;

            this._accumulatedUsage = new List<double>();
            this._actualUsage = new List<double>();
            this._throughput = new List<double>();
            this._temperature = new List<double>();
            this._pressure = new List<double>();
            this._measuredTimes = new List<DateTime>();

            this._slots = new Dictionary<DateTime, MeasurementsRecord>();
            this._changeLog = new Dictionary<DateTime, string>();

            DateTime measureStart = DataService.GetInstance().MeasureStart;
            DateTime measureEnd = measureStart.AddHours(24);
            for (DateTime time = measureStart; time < measureEnd; time = time.AddMinutes(5))
            {
                this._slots.Add(time, null);
                this._changeLog.Add(time, "");
            }
        }

        private List<double> GetProperty(Func<MeasurementsRecord, double> selector, bool overrideDefault = true)
        {
            var list = new List<double>();
            double defaultValue = 0;
            foreach ((DateTime key, MeasurementsRecord record) in _slots)
            {
                if (record == null)
                {
                    list.Add(defaultValue);
                }
                else
                {
                    double value = selector(record);
                    list.Add(value);
                    if (overrideDefault) defaultValue = value;
                }
            }
            return list;
        }

        public void HandleNewRecord(MeasurementsRecord record, int increase = 0)
        {

            DateTime measuredTime = record.Date;
            DateTime lastUpdate = NumberOfRecords > 0 ? _measuredTimes.Last() : measuredTime.AddMinutes(-1);

            if (measuredTime > lastUpdate)
            {
                DateTime slotDateTime = RoundToInterval(measuredTime);
                if (increase > 0)
                {
                    slotDateTime = slotDateTime.AddMinutes(increase);
                }
                MeasurementsRecord existingRecord = _slots[slotDateTime];

                if (slotDateTime > _slots.Keys.Last())
                {
                    // all data should reset - end condition
                }

                if (existingRecord == null)
                {
                    InsertRecord(slotDateTime, record);

                    double archivedUsage = record.DeviceNormalArchived;
                    double normalUsage = record.DeviceNormal;
                    if (archivedUsage == normalUsage)
                    {
                        this._changeLog[slotDateTime] = "no change";
                        IsActive = false;
                    }
                    else
                    {
                        double[] last5Usage = ActualUsage.ToArray().Reverse().Take(5).Reverse().ToArray();
                        bool isAllUsageZero = last5Usage.All(x => x == 0);
                        Console.WriteLine("Device " + DeviceNumber + " is all usage zero "+ isAllUsageZero);
                        if (last5Usage.Length > 0 && isAllUsageZero)
                        {

                        }
                    }
                }                
                else
                {
                    HandleNewRecord(existingRecord, increase + 5);
                }
            }
        }


        private void InsertRecord(DateTime slot, MeasurementsRecord record)
        {
            double newUsage = record.DeviceNormal - record.DeviceNormalArchived;
            newUsage = ActualUsage.Count > 0 ? newUsage - Utils.Sum(ActualUsage.GetRange(0, ActualUsage.Count)) : 0;
            double aggregatedValue = Utils.Sum(ActualUsage) + newUsage;

            //data.throughput.Add(new (item.Throughput));

            _measuredTimes.Add(record.Date);

            record.ActualUsage = newUsage;
            record.AccumulatedUsage = aggregatedValue;

            _slots[slot] = record;
            this.LastOccupiedSlot = new DateTime(slot.Ticks);
        }

        private DateTime RoundToInterval(DateTime timestamp)
        {
            // round to closest 5 minutes before
            int minutes = timestamp.Minute;
            int remainder = minutes % 5;
            int minutesToSubtract = remainder == 0 ? 0 : remainder;
            DateTime rounded = timestamp.AddMinutes(-minutesToSubtract);
            return new DateTime(rounded.Year, rounded.Month, rounded.Day, rounded.Hour, rounded.Minute, 0);
        }
    }
}
