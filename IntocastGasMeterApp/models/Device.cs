using IntocastGasMeterApp.services;
using LiveChartsCore.Defaults;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Provider;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace IntocastGasMeterApp.models
{
    class Device
    {
        public static List<Device> devices = new List<Device>();
        public static readonly string COMBINED_DEVICE_NUMBER = "Spolu";
        public static Device combinedDevice = null;
        public static Device Get(string deviceNumber)
        {
            if (deviceNumber == COMBINED_DEVICE_NUMBER)
            {
                return combinedDevice;
            }

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
            Device newDevice = new Device(COMBINED_DEVICE_NUMBER, COMBINED_DEVICE_NUMBER);
            Device sample = devices[0];

            // get values for each device
            int numberOfRecords = devices.Max(device => device.NumberOfRecords);

            // combine values according to the requirements
            DateTime timeStart = sample.Slots.Keys.ToList().First();
            DateTime timeEnd = sample.Slots.Keys.ElementAt(numberOfRecords - 1);
            int index = 0;
            for (DateTime key = timeStart; key <= timeEnd; key = key.AddMinutes(5))
            {
                List<MeasurementsRecord> records = new List<MeasurementsRecord>();

                foreach (Device device in devices)
                {
                    records.Add(device.Slots[key]);
                }

                MeasurementsRecord combinedRecord = GetCombinedRecord(records[0], records[1]);
                newDevice.Slots[key] = combinedRecord;
            }

            foreach (Device device in devices)
            {
                if (device.IsActive)
                {
                    newDevice.LastDataQuery = device.LastDataQuery;
                    newDevice.LastDataUpdateSlot = device.LastDataUpdateSlot;
                    newDevice.LastRealDataUpdate = device.LastRealDataUpdate;
                    newDevice.LastRealDataUpdateSlot = device.LastRealDataUpdateSlot;
                }
            }

            return newDevice;
        }

        private static MeasurementsRecord GetCombinedRecord(MeasurementsRecord r1, MeasurementsRecord r2)
        {
            // we assume that primary record comes from an Active device, while secondary is from a non-active device
            MeasurementsRecord primary = r1.IsFromActiveDevice ? r1 : r2;
            MeasurementsRecord secondary = r1.IsFromActiveDevice ? r2 : r1;

            MeasurementsRecord output = new MeasurementsRecord();

            output.AccumulatedUsage = primary.AccumulatedUsage + secondary.AccumulatedUsage;
            output.ActualUsage = primary.ActualUsage + secondary.ActualUsage;
            output.Throughput = primary.Throughput + secondary.Throughput;

            output.DeviceRaw = primary.DeviceRaw + secondary.DeviceRaw;
            output.DeviceNormal = primary.DeviceNormal + secondary.DeviceNormal;
            output.DeviceRawArchived = primary.DeviceRawArchived + secondary.DeviceRawArchived;
            output.DeviceNormalArchived = primary.DeviceNormalArchived + secondary.DeviceNormalArchived;

            output.Pressure = primary.Pressure;
            output.Temperature = primary.Temperature;
            output.Date = new DateTime(primary.Date.Ticks);
            output.ArchiveDate = new DateTime(primary.ArchiveDate.Ticks);

            output.IsPartial = primary.IsPartial;

            return output;
        }


        public string DeviceNumber { get; set; }
        public string CustomerId { get; set; }

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
                        continue;
                    }
                    MeasurementsRecord previous = null;
                    Slots.TryGetValue(key.AddMinutes(-5), out previous);
                    if (i > 0 && (previous == null || previous.IsPartial))
                    {
                        list.Add(null);
                    }
                    else
                    {
                        list.Add(record.Throughput);
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
                if (record is null || (!overrideDefault && record.IsPartial))
                {
                    list.Add(defaultValue);
                    continue;
                }

                double value = selector(record);
                list.Add(value);
                if (overrideDefault) defaultValue = value;
            }
            return list;
        }

        private List<double?> GetNullableProperty(Func<MeasurementsRecord, double?> selector)
        {
            var list = new List<double?>();
            foreach ((DateTime key, MeasurementsRecord record) in Slots)
            {
                if (record is null) 
                { 
                    list.Add(null);
                    continue;
                }
                list.Add(selector(record));
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
            if (record.Date >= new DateTime(2024, 12, 2, 20, 22, 0) && IsActive)
            {

            }
            double dailyUsage = record.DeviceNormal - record.DeviceNormalArchived;
            double sumUsage = Utils.Sum(ActualUsage.GetRange(0, NumberOfRecords));
            double newUsage = dailyUsage - sumUsage;
            //newUsage = ActualUsage.Count > 0 ? newUsage - Utils.Sum(ActualUsage.GetRange(0, ActualUsage.Count)) : 0;
            double aggregatedValue = Utils.Sum(ActualUsage) + newUsage;

            if (IsActive) record.IsFromActiveDevice = true;
            else record.IsFromActiveDevice = false;

            record.ActualUsage = newUsage;
            record.Throughput = newUsage;
            record.AccumulatedUsage = dailyUsage;

            Slots[slot] = record;
            this.LastDataUpdateSlot = new DateTime(slot.Ticks);
            this.LastRealDataUpdateSlot = new DateTime(slot.Ticks);
            this.LastRealDataUpdate = new DateTime(record.Date.Ticks);

            DataService data = DataService.GetInstance();
            if (IsActive)
            {
                data.UpdateStatus("OK", "", Colors.LimeGreen);
            }
        }

        public void AddPartialRecords()
        {
            for (DateTime time = Slots.Keys.ToList().First(); time <= LastDataUpdateSlot; time = time.AddMinutes(5))
            {
                if (Slots[time] == null)
                {
                    MeasurementsRecord previous = Slots[time.AddMinutes(-5)];
                    MeasurementsRecord record = new MeasurementsRecord();
                    record.IsPartial = true;
                    record.Date = new DateTime(time.Ticks);
                    record.DeviceRaw = previous.DeviceRaw;
                    record.DeviceNormal = previous.DeviceNormal;
                    record.DeviceRawArchived = previous.DeviceRawArchived;
                    record.DeviceNormalArchived = previous.DeviceNormalArchived;
                    record.ArchiveDate = previous.ArchiveDate;
                    record.AccumulatedUsage = previous.AccumulatedUsage;
                    record.ActualUsage = previous.ActualUsage;
                    record.IsFromActiveDevice = previous.IsFromActiveDevice;

                    record.Temperature = null;
                    record.Pressure = null;
                    record.Throughput = null;

                    Slots[time] = record;
                }
            }
        }

        // this should be called when the new 24 hour period starts
        public void ResetDevice()
        {
            DateTime measureStart = DataService.GetInstance().MeasureStart.AddHours(24);
            this.ResetDevice(measureStart);
        }

        public void ResetDevice(DateTime resetDate)
        {
            Dictionary<DateTime, MeasurementsRecord> newSlots = new Dictionary<DateTime, MeasurementsRecord>();
            DateTime measureEnd = resetDate.AddHours(24);
            this.Slots.Clear();
            for (DateTime time = resetDate; time < measureEnd; time = time.AddMinutes(5))
            {
                this.Slots.Add(time, null);
                Console.WriteLine("Resetting device:" + time);
            }

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
