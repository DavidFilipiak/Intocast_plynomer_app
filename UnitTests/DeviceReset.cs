using Xunit;
using IntocastGasMeterApp.models;
using IntocastGasMeterApp.services;

namespace UnitTests
{
    public class DeviceReset
    {
        [Fact]
        public void ManualReset()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            d.StartsActive = true;
            string workingStream = Consts.GetDataStream(null, true, null);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            DateTime resetTime = Consts.measureStart.AddDays(1);
            d.ResetDevice(resetTime);

            List<MeasurementsRecord> deviceRecords = d.Slots.Values.ToList();

            Assert.True(d.IsActive);
            Assert.Equal(1, d.NumberOfRecords);
            Assert.Equal(resetTime, d.LastDataUpdateSlot);
            Assert.All(deviceRecords, deviceRecord => Assert.Null(deviceRecord));
        }

        [Fact]
        public void AutoResetWorkingDevice()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            d.StartsActive = true;
            string workingStream = Consts.GetDataStream(null, true, null);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");
            DateTime time = Consts.measureStart.AddHours(24).AddMinutes(-5);
            foreach (MeasurementsRecord record in records)
            {
                if (time >= Consts.measureStart.AddHours(24))
                {
                    record.ArchiveDate = Consts.measureStart.AddHours(24);
                }
                record.Date = time;
                time = time.AddMinutes(5);
            }

            d.ResetDevice(Consts.measureStart.AddHours(24));
             
            d.HandleNewRecords(time, records);

            Assert.True(d.IsActive);
            Assert.Equal(4, d.NumberOfRecords);
            Assert.Equal(time.AddMinutes(-5), d.LastDataUpdateSlot);
        }

        [Fact]
        public void AutoResetNotWorkingDevice()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            string workingStream = Consts.GetDataStream(null, false, null);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");
            DateTime time = Consts.measureStart.AddHours(24).AddMinutes(-5);
            foreach (MeasurementsRecord record in records)
            {
                if (time >= Consts.measureStart.AddHours(24))
                {
                    record.ArchiveDate = Consts.measureStart.AddHours(24);
                }
                record.Date = time;
                time = time.AddMinutes(5);
            }

            d.ResetDevice(Consts.measureStart.AddHours(24));

            d.HandleNewRecords(time, records);

            Assert.False(d.IsActive);
            Assert.Equal(4, d.NumberOfRecords);
            Assert.Equal(time.AddMinutes(-5), d.LastDataUpdateSlot);
        }
    }
}
