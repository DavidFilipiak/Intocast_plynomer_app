using Xunit;
using IntocastGasMeterApp.models;
using IntocastGasMeterApp.services;

namespace UnitTests
{
    public class DeviceCombineTests
    {
        [Fact]
        public void CombineOneWorkingOneNotWorking()
        {
            Device d_working = new Device(Consts.deviceId1, Consts.customerId, Consts.measureStart);
            d_working.StartsActive = true;
            string workingStream = Consts.GetDataStream(null, true, null);
            MeasurementsRecord[] workingRecords = Utils.parseCSVMeasurements(workingStream, ";");
            d_working.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), workingRecords);


            Device d_notWorking = new Device(Consts.deviceId2, Consts.customerId, Consts.measureStart);
            string notWorkingStream = Consts.GetDataStream(null, false, null);
            MeasurementsRecord[] notWorkingRecords = Utils.parseCSVMeasurements(notWorkingStream, ";");
            d_notWorking.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), notWorkingRecords);

            Device d = Device.Combine([d_working, d_notWorking]);

            Assert.False(d.IsActive);
            Assert.Equal(d_working.AccumulatedUsage.Sum() + d_notWorking.AccumulatedUsage.Sum(), d.AccumulatedUsage.Sum());
            Assert.Equal(d_working.ActualUsage.Sum() + d_notWorking.ActualUsage.Sum(), d.ActualUsage.Sum());
            Assert.Equal(d_working.Temperature, d.Temperature);
            Assert.Equal(d_working.Pressure, d.Pressure);
            Assert.Equal(d_working.Throughput, d.Throughput);
            Assert.Equal(d_working.LastRealDataUpdateSlot, d.LastRealDataUpdateSlot);
            Assert.Equal(d_working.LastRealDataUpdate, d.LastRealDataUpdate);
            Assert.Equal(d_working.LastDataUpdateSlot, d.LastDataUpdateSlot);
            Assert.Equal(d_working.NumberOfRecords, d.NumberOfRecords);

            bool[] partials = d.Slots.Values.Take(5).Select(value => value.IsPartial).ToArray();

            Assert.Equal([false, false, false, false, false], partials);
        }

        [Fact]
        public void CombineOneMiddleSwitch()
        {
            Device d_working = new Device(Consts.deviceId1, Consts.customerId, Consts.measureStart);
            d_working.StartsActive = true;
            string workingStream = Consts.GetDataStream(null, true, [2]);
            MeasurementsRecord[] workingRecords = Utils.parseCSVMeasurements(workingStream, ";");
            d_working.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), workingRecords);


            Device d_notWorking = new Device(Consts.deviceId2, Consts.customerId, Consts.measureStart);
            string notWorkingStream = Consts.GetDataStream(null, false, [2]);
            MeasurementsRecord[] notWorkingRecords = Utils.parseCSVMeasurements(notWorkingStream, ";");
            d_notWorking.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), notWorkingRecords);

            Device d = Device.Combine([d_working, d_notWorking]);

            Assert.False(d.IsActive);
            Assert.Equal(d_working.AccumulatedUsage.Sum() + d_notWorking.AccumulatedUsage.Sum(), d.AccumulatedUsage.Sum());
            Assert.Equal(d_working.ActualUsage.Sum() + d_notWorking.ActualUsage.Sum(), d.ActualUsage.Sum());
            Assert.Equal([20, 21, 6, 7, 8], d.Temperature.Take(5).ToArray());
            Assert.Equal([460, 480, 420, 440, 460], d.Pressure.Take(5).ToArray());
            Assert.Equal([10, 10, 10, 10, 10], d.Throughput.Take(5).ToArray());
            Assert.Equal(d_notWorking.LastRealDataUpdateSlot, d.LastRealDataUpdateSlot);
            Assert.Equal(d_notWorking.LastRealDataUpdate, d.LastRealDataUpdate);
            Assert.Equal(d_notWorking.LastDataUpdateSlot, d.LastDataUpdateSlot);
            Assert.Equal(d_notWorking.NumberOfRecords, d.NumberOfRecords);

            bool[] partials = d.Slots.Values.Take(5).Select(value => value.IsPartial).ToArray();

            Assert.Equal([false, false, false, false, false], partials);
        }

        [Fact]
        public void CombinePrimaryMiddleMissing()
        {
            Device d_working = new Device(Consts.deviceId1, Consts.customerId, Consts.measureStart);
            d_working.StartsActive = true;
            string workingStream = Consts.GetDataStream(2, true, null);
            MeasurementsRecord[] workingRecords = Utils.parseCSVMeasurements(workingStream, ";");
            d_working.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), workingRecords);


            Device d_notWorking = new Device(Consts.deviceId2, Consts.customerId, Consts.measureStart);
            string notWorkingStream = Consts.GetDataStream(null, false, null);
            MeasurementsRecord[] notWorkingRecords = Utils.parseCSVMeasurements(notWorkingStream, ";");
            d_notWorking.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), notWorkingRecords);

            Device d = Device.Combine([d_working, d_notWorking]);

            Assert.False(d.IsActive);
            Assert.Equal(d_working.AccumulatedUsage.Sum() + d_notWorking.AccumulatedUsage.Sum(), d.AccumulatedUsage.Sum());
            Assert.Equal(d_working.ActualUsage.Sum() + d_notWorking.ActualUsage.Sum(), d.ActualUsage.Sum());
            Assert.Equal(d_working.Temperature, d.Temperature);
            Assert.Equal(d_working.Pressure, d.Pressure);
            Assert.Equal(d_working.Throughput, d.Throughput);
            Assert.Equal(d_working.LastRealDataUpdateSlot, d.LastRealDataUpdateSlot);
            Assert.Equal(d_working.LastRealDataUpdate, d.LastRealDataUpdate);
            Assert.Equal(d_working.LastDataUpdateSlot, d.LastDataUpdateSlot);
            Assert.Equal(d_working.NumberOfRecords, d.NumberOfRecords);

            bool[] partials = d.Slots.Values.Take(5).Select(value => value.IsPartial).ToArray();

            Assert.Equal([false, false, true, false, false], partials);
        }

        [Fact]
        public void CombineSecondaryMiddleMissing()
        {
            Device d_working = new Device(Consts.deviceId1, Consts.customerId, Consts.measureStart);
            d_working.StartsActive = true;
            string workingStream = Consts.GetDataStream(null, true, null);
            MeasurementsRecord[] workingRecords = Utils.parseCSVMeasurements(workingStream, ";");
            d_working.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), workingRecords);


            Device d_notWorking = new Device(Consts.deviceId2, Consts.customerId, Consts.measureStart);
            string notWorkingStream = Consts.GetDataStream(2, false, null);
            MeasurementsRecord[] notWorkingRecords = Utils.parseCSVMeasurements(notWorkingStream, ";");
            d_notWorking.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), notWorkingRecords);

            Device d = Device.Combine([d_working, d_notWorking]);

            Assert.False(d.IsActive);
            Assert.Equal(d_working.AccumulatedUsage.Sum() + d_notWorking.AccumulatedUsage.Sum(), d.AccumulatedUsage.Sum());
            Assert.Equal(d_working.ActualUsage.Sum() + d_notWorking.ActualUsage.Sum(), d.ActualUsage.Sum());
            Assert.Equal(d_working.Temperature, d.Temperature);
            Assert.Equal(d_working.Pressure, d.Pressure);
            Assert.Equal(d_working.Throughput, d.Throughput);
            Assert.Equal(d_working.LastRealDataUpdateSlot, d.LastRealDataUpdateSlot);
            Assert.Equal(d_working.LastRealDataUpdate, d.LastRealDataUpdate);
            Assert.Equal(d_working.LastDataUpdateSlot, d.LastDataUpdateSlot);
            Assert.Equal(d_working.NumberOfRecords, d.NumberOfRecords);

            bool[] partials = d.Slots.Values.Take(5).Select(value => value.IsPartial).ToArray();

            Assert.Equal([false, false, false, false, false], partials);
        }

        [Fact]
        public void CombinePrimaryMiddleMissingMiddleSwitch()
        {
            Device d_working = new Device(Consts.deviceId1, Consts.customerId, Consts.measureStart);
            d_working.StartsActive = true;
            string workingStream = Consts.GetDataStream(2, true, [2]);
            MeasurementsRecord[] workingRecords = Utils.parseCSVMeasurements(workingStream, ";");
            d_working.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), workingRecords);


            Device d_notWorking = new Device(Consts.deviceId2, Consts.customerId, Consts.measureStart);
            string notWorkingStream = Consts.GetDataStream(null, false, [2]);
            MeasurementsRecord[] notWorkingRecords = Utils.parseCSVMeasurements(notWorkingStream, ";");
            d_notWorking.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), notWorkingRecords);

            Device d = Device.Combine([d_working, d_notWorking]);

            Assert.False(d.IsActive);
            Assert.Equal(d_working.AccumulatedUsage.Sum() + d_notWorking.AccumulatedUsage.Sum(), d.AccumulatedUsage.Sum());
            Assert.Equal(d_working.ActualUsage.Sum() + d_notWorking.ActualUsage.Sum(), d.ActualUsage.Sum());
            Assert.Equal([20, 21, 6, 7, 8], d.Temperature.Take(5).ToArray());
            Assert.Equal([460, 480, 420, 440, 460], d.Pressure.Take(5).ToArray());
            Assert.Equal([10, 10, 10, 10, 10], d.Throughput.Take(5).ToArray());
            Assert.Equal(d_notWorking.LastRealDataUpdateSlot, d.LastRealDataUpdateSlot);
            Assert.Equal(d_notWorking.LastRealDataUpdate, d.LastRealDataUpdate);
            Assert.Equal(d_notWorking.LastDataUpdateSlot, d.LastDataUpdateSlot);
            Assert.Equal(d_notWorking.NumberOfRecords, d.NumberOfRecords);

            bool[] partials = d.Slots.Values.Take(5).Select(value => value.IsPartial).ToArray();

            Assert.Equal([false, false, false, false, false], partials);
        }

        [Fact]
        public void CombinePrimaryMiddleMissingBeforeSwitch()
        {
            Device d_working = new Device(Consts.deviceId1, Consts.customerId, Consts.measureStart);
            d_working.StartsActive = true;
            string workingStream = Consts.GetDataStream(1, true, [2]);
            MeasurementsRecord[] workingRecords = Utils.parseCSVMeasurements(workingStream, ";");
            d_working.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), workingRecords);


            Device d_notWorking = new Device(Consts.deviceId2, Consts.customerId, Consts.measureStart);
            string notWorkingStream = Consts.GetDataStream(null, false, [2]);
            MeasurementsRecord[] notWorkingRecords = Utils.parseCSVMeasurements(notWorkingStream, ";");
            d_notWorking.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), notWorkingRecords);

            Device d = Device.Combine([d_working, d_notWorking]);

            Assert.False(d.IsActive);
            Assert.Equal(d_working.AccumulatedUsage.Sum() + d_notWorking.AccumulatedUsage.Sum(), d.AccumulatedUsage.Sum());
            Assert.Equal(d_working.ActualUsage.Sum() + d_notWorking.ActualUsage.Sum(), d.ActualUsage.Sum());
            Assert.Equal([20, null, 6, 7, 8], d.Temperature.Take(5).ToArray());
            Assert.Equal([460, null, 420, 440, 460], d.Pressure.Take(5).ToArray());
            Assert.Equal([10, null, null, 10, 10], d.Throughput.Take(5).ToArray());
            Assert.Equal(d_notWorking.LastRealDataUpdateSlot, d.LastRealDataUpdateSlot);
            Assert.Equal(d_notWorking.LastRealDataUpdate, d.LastRealDataUpdate);
            Assert.Equal(d_notWorking.LastDataUpdateSlot, d.LastDataUpdateSlot);
            Assert.Equal(d_notWorking.NumberOfRecords, d.NumberOfRecords);

            bool[] partials = d.Slots.Values.Take(5).Select(value => value.IsPartial).ToArray();

            Assert.Equal([false, true, false, false, false], partials);
        }

        [Fact]
        public void CombineTwoMissingDevices()
        {
            Device d1 = new Device(Consts.deviceId1, Consts.customerId, Consts.measureStart);
            d1.StartsActive = false;
            string stream1 = Consts.GetDataStream(null, true, null);
            MeasurementsRecord[] records1 = Utils.parseCSVMeasurements(stream1, ";");
            d1.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records1);

            Device d2 = new Device(Consts.deviceId2, Consts.customerId, Consts.measureStart);
            d2.StartsActive = false;
            string stream2 = Consts.GetDataStream(null, false, null);
            MeasurementsRecord[] records2 = Utils.parseCSVMeasurements(stream2, ";");
            d2.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records2);
            
            Device d = Device.Combine([d1, d2]);

            Assert.False(d.IsActive);
        }
    }
}
