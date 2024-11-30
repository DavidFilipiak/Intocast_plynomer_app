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
using System.Windows.Media;

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
                double temperature = 0;
                double pressure = 0;

                foreach (Device device in devices)
                {
                    accumulatedUsage += device.AccumulatedUsage[i];
                    actualUsage += (double)device.ActualUsage[i];
                    temperature += (double)device.Temperature[i];
                    pressure += (double)device.Pressure[i];
                }

                newDevice.AccumulatedUsage.Add(accumulatedUsage);
                newDevice.ActualUsage.Add(actualUsage);
                newDevice.Temperature.Add(temperature);
                newDevice.Pressure.Add(pressure);
            }

            return newDevice;
        }


        public string DeviceNumber { get; set; }
        public string CustomerId { get; set; }

        /*
        private readonly List<double> _accumulatedUsage;
        private readonly List<double> _actualUsage;
        private readonly List<double> _temperature;
        private readonly List<double> _pressure;
        */

        public readonly List<DateTime> MeasuredTimes;

        private const int NUMBER_OF_TIMESLOTS = 24 * 12; // number of 5-minue slots in a day
        public readonly Dictionary<DateTime, MeasurementsRecord> Slots;


        public List<double> AccumulatedUsage => GetProperty(record => record.AccumulatedUsage);
        public List<double> ActualUsage => GetProperty(record => record.ActualUsage, false);
        public List<double?> Temperature => GetNullableProperty(record => record.Temperature);
        public List<double?> Pressure => GetNullableProperty(record => record.Pressure);

        public List<double?> Throughput
        {
            get {
                var list = new List<double?>();
                int i = 0;
                foreach ((DateTime key, MeasurementsRecord record) in Slots)
                {
                    if (record == null)
                    {
                        list.Add(null);
                    }
                    else if (i > 0 && Slots[key.AddMinutes(-5)] == null)
                    {
                        list.Add(null);
                    }
                    else
                    {
                        list.Add(record.ActualUsage);
                    }
                    i++;
                }
                return list;
            }
        }

        public int NumberOfRecords { get
            {
                // the index last slot which is not null
                return Slots.Keys.ToList().IndexOf(LastDataUpdateSlot) + 1;
            }
        }

        public bool IsActive { get; set; }
        public DateTime LastDataUpdateSlot { get; set; }
        public DateTime LastRealDataUpdateSlot { get; set; }
        public DateTime LastDataQuery { get; set; }
        public DateTime LastRealDataUpdate { get; set; }


        public Device(string deviceNumber, string customerId)
        {
            DeviceNumber = deviceNumber;
            CustomerId = customerId;

            /*
            this._accumulatedUsage = new List<double>();
            this._actualUsage = new List<double>();
            this._temperature = new List<double>();
            this._pressure = new List<double>();
            */
            this.MeasuredTimes = new List<DateTime>();

            this.Slots = new Dictionary<DateTime, MeasurementsRecord>();

            DateTime measureStart = DataService.GetInstance().MeasureStart;
            DateTime measureEnd = measureStart.AddHours(24);
            for (DateTime time = measureStart; time < measureEnd; time = time.AddMinutes(5))
            {
                this.Slots.Add(time, null);
                Console.WriteLine(time);
            }
            IsActive = false;
        }

        private List<double> GetProperty(Func<MeasurementsRecord, double> selector, bool overrideDefault = true)
        {
            var list = new List<double>();
            double defaultValue = 0;
            foreach ((DateTime key, MeasurementsRecord record) in Slots)
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

        private List<double?> GetNullableProperty(Func<MeasurementsRecord, double> selector)
        {
            var list = new List<double?>();
            foreach ((DateTime key, MeasurementsRecord record) in Slots)
            {
                if (record == null)
                {
                    list.Add(null);
                }
                else
                {
                    list.Add(selector(record));
                }
            }
            return list;
        }

        public void HandleNewRecord(MeasurementsRecord record)
        {

            DateTime measuredTime = record.Date;
            //DateTime lastUpdate = NumberOfRecords > 0 ? MeasuredTimes.Last() : measuredTime.AddMinutes(-1);

            if (measuredTime > LastRealDataUpdate)
            {
                DateTime slotDateTime = RoundToInterval(measuredTime);

                DateTime previousSlotDateTime = slotDateTime.AddMinutes(-5);
                MeasurementsRecord existingRecord;
                MeasurementsRecord previousSlotRecord;
                bool currentSlotExists = Slots.TryGetValue(slotDateTime, out existingRecord);
                bool previousSlotExists = Slots.TryGetValue(previousSlotDateTime, out previousSlotRecord);

                if (!currentSlotExists) return;

                if (previousSlotDateTime >= Slots.Keys.ToList().First() && previousSlotExists && previousSlotRecord == null && measuredTime.Minute == 0)
                {
                    InsertRecord(previousSlotDateTime, record);
                    return;
                }                

                if (existingRecord == null)
                {
                    InsertRecord(slotDateTime, record);
                }

                //find latest non null record
                double lastUsage = ActualUsage.Last();
                double newUsage = record.ActualUsage;
                if (IsActive && lastUsage == newUsage) IsActive = false;
                else if (!IsActive && lastUsage != newUsage) IsActive = true;
            }
        }


        private void InsertRecord(DateTime slot, MeasurementsRecord record)
        {
            double newUsage = record.DeviceNormal - record.DeviceNormalArchived;
            newUsage = ActualUsage.Count > 0 ? newUsage - Utils.Sum(ActualUsage.GetRange(0, ActualUsage.Count)) : 0;
            double aggregatedValue = Utils.Sum(ActualUsage) + newUsage;

            MeasuredTimes.Add(record.Date);
            //_actualUsage.Add(newUsage);

            record.ActualUsage = newUsage;
            record.AccumulatedUsage = aggregatedValue;

            Slots[slot] = record;
            this.LastDataUpdateSlot = new DateTime(slot.Ticks);
            this.LastRealDataUpdateSlot = new DateTime(slot.Ticks);
            this.LastRealDataUpdate = new DateTime(record.Date.Ticks);
            
            DataService data = DataService.GetInstance();
            if (IsActive)
            {
                data.UpdateStatus("OK", "", Colors.LimeGreen);
            }            
            //data.UpdateStatus("Chýbajúce dáta", "Za posledných 10 minút neprišli žiadne nové dáta. Zobrazené údaje nemusia byť aktuálne.", Colors.Orange);
        }

        // this should be called when the new 24 hour period starts
        public void ResetDevice()
        {
            Dictionary<DateTime, MeasurementsRecord> newSlots = new Dictionary<DateTime, MeasurementsRecord>();
            DateTime measureStart = DataService.GetInstance().MeasureStart.AddHours(24);
            DateTime measureEnd = measureStart.AddHours(24);
            this.Slots.Clear();
            for (DateTime time = measureStart; time < measureEnd; time = time.AddMinutes(5))
            {
                this.Slots.Add(time, null);
                Console.WriteLine("Resetting device:" + time);
            }
            this.MeasuredTimes.Clear();

            DateTime firstTimeSlot = this.Slots.Keys.ToList().First();
            this.LastRealDataUpdate = new DateTime(firstTimeSlot.Ticks);
            this.LastRealDataUpdateSlot = new DateTime(firstTimeSlot.Ticks);
            this.LastDataUpdateSlot = new DateTime(firstTimeSlot.Ticks);
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
