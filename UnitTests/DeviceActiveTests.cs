
using Xunit;
using IntocastGasMeterApp.models;
using IntocastGasMeterApp.services;

namespace UnitTests
{
    public class DeviceActiveTests
    {

        [Fact]
        public void WorkingMeasurements()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            d.StartsActive = true;
            string workingStream = Consts.GetDataStream(null, true, null);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.True(d.IsActive);
            Assert.Equal([true, true, true, true, true], fromActiveDevice);
        }

        [Fact]
        public void NotWorkingMeasurements()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            d.StartsActive = false;
            string notWorkingStream = Consts.GetDataStream(null, false, null);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(notWorkingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.False(d.IsActive);
            Assert.Equal([false, false, false, false, false], fromActiveDevice);
        }

        [Fact]
        public void WorkingMeasurementsFirstMissing()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            d.StartsActive = true;
            string workingStream = Consts.GetDataStream(0, true, null);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.True(d.IsActive);
            Assert.Equal([true, true, true, true, true], fromActiveDevice);
        }

        [Fact]
        public void WorkingMeasurementsMiddleMissing()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            d.StartsActive = true;
            string workingStream = Consts.GetDataStream(2, true, null);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.True(d.IsActive);
            Assert.Equal([true, true, true, true, true], fromActiveDevice);
        }

        [Fact]
        public void WorkingMeasurementsLastMissing()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            d.StartsActive = true;
            string workingStream = Consts.GetDataStream(4, true, null);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.True(d.IsActive);
            Assert.Equal([true, true, true, true, true], fromActiveDevice);
        }

        [Fact]
        public void NotWorkingMeasurementsFirstMissing()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            string workingStream = Consts.GetDataStream(0, false, null);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.False(d.IsActive);
            Assert.Equal([false, false, false, false, false], fromActiveDevice);
        }

        [Fact]
        public void NotWorkingMeasurementsMiddleMissing()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            string workingStream = Consts.GetDataStream(2, false, null);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.False(d.IsActive);
            Assert.Equal([false, false, false, false, false], fromActiveDevice);
        }

        [Fact]
        public void NotWorkingMeasurementsLastMissing()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            string workingStream = Consts.GetDataStream(4, false, null);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.False(d.IsActive);
            Assert.Equal([false, false, false, false, false], fromActiveDevice);
        }

        [Fact]
        public void WorkingMeasurementsAllMissing()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            string workingStream = Consts.GetDataStream(Int32.MaxValue, true, null);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.False(d.IsActive);
            Assert.Equal([false, false, false, false, false], fromActiveDevice);
        }

        [Fact]
        public void WorkingMeasurementsMiddleSwitch()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            d.IsActive = true;
            string workingStream = Consts.GetDataStream(null, true, [2]);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.False(d.IsActive);
            Assert.Equal([true, true, false, false, false], fromActiveDevice);
        }

        [Fact]
        public void WorkingMeasurementsEndSwitch()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            d.StartsActive = true;
            string workingStream = Consts.GetDataStream(null, true, [4]);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.False(d.IsActive);
            Assert.Equal([true, true, true, true, false], fromActiveDevice);
        }

        [Fact]
        public void NotWorkingMeasurementsMiddleSwitch()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            string workingStream = Consts.GetDataStream(null, false, [2]);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.True(d.IsActive);
            Assert.Equal([false, false, true, true, true], fromActiveDevice);
        }

        [Fact]
        public void NotWorkingMeasurementsEndSwitch()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            string workingStream = Consts.GetDataStream(null, false, [4]);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.True(d.IsActive);
            Assert.Equal([false, false, false, false, true], fromActiveDevice);
        }

        [Fact]
        public void WorkingMeasurementsFirstMissingSwitch()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            d.StartsActive = true;
            string workingStream = Consts.GetDataStream(0, true, [0]);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.False(d.IsActive);
            Assert.Equal([true, false, false, false, false], fromActiveDevice);
        }

        [Fact]
        public void WorkingMeasurementsMiddleMissingSwitch()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            d.StartsActive = true;
            string workingStream = Consts.GetDataStream(2, true, [2]);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.False(d.IsActive);
            Assert.Equal([true, true, true, false, false], fromActiveDevice);
        }

        [Fact]
        public void WorkingMeasurementsLastMissingSwitch()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            d.StartsActive = true;
            string workingStream = Consts.GetDataStream(4, true, [4]);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.True(d.IsActive);
            Assert.Equal([true, true, true, true, true], fromActiveDevice);
        }

        [Fact]
        public void NotWorkingMeasurementsFirstMissingSwitch()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            string workingStream = Consts.GetDataStream(0, false, [0]);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.True(d.IsActive);
            Assert.Equal([false, true, true, true, true], fromActiveDevice);
        }

        [Fact]
        public void NotWorkingMeasurementsMiddleMissingSwitch()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            string workingStream = Consts.GetDataStream(2, false, [2]);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.True(d.IsActive);
            Assert.Equal([false, false, false, true, true], fromActiveDevice);
        }

        [Fact]
        public void NotWorkingMeasurementsLastMissingSwitch()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            string workingStream = Consts.GetDataStream(4, false, [4]);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.False(d.IsActive);
            Assert.Equal([false, false, false, false, false], fromActiveDevice);
        }

        [Fact]
        public void WorkingMeasurementsTwiceSwitch()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            d.StartsActive = true;
            string workingStream = Consts.GetDataStream(null, true, [1, 3]);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.True(d.IsActive);
            Assert.Equal([true, false, false, true, true], fromActiveDevice);
        }

        [Fact]
        public void NotWorkingMeasurementsTwiceSwitch()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            string workingStream = Consts.GetDataStream(null, false, [1, 3]);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.False(d.IsActive);
            Assert.Equal([false, true, true, false, false], fromActiveDevice);
        }

        [Fact]
        public void WorkingMeasurementsTwiceSwitchMiddleMissing()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            d.StartsActive = true;
            string workingStream = Consts.GetDataStream(2, true, [1, 3]);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.True(d.IsActive);
            Assert.Equal([true, false, false, true, true], fromActiveDevice);
        }

        [Fact]
        public void WorkingMeasurementsTwiceSwitchSecondSwitchMissing()
        {
            Device d = new Device(Consts.customerId, Consts.deviceId1, Consts.measureStart);
            d.StartsActive = true;
            string workingStream = Consts.GetDataStream(3, true, [1, 3]);
            Console.WriteLine(workingStream);
            MeasurementsRecord[] records = Utils.parseCSVMeasurements(workingStream, ";");

            d.HandleNewRecords(Consts.measureStart.AddMinutes(5 * 5), records);
            bool[] fromActiveDevice = d.Slots.Values.Take(5).Select(value => value.IsFromActiveDevice).ToArray();

            Assert.True(d.IsActive);
            Assert.Equal([true, false, false, false, true], fromActiveDevice);
        }
    }
}