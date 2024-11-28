using IntocastGasMeterApp.models;
using LiveChartsCore.Defaults;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IntocastGasMeterApp.services
{
    class Utils
    {
        private static Utils instance = null;
        private Utils()
        {

        }

        // singleton service
        public static Utils GetInstance()
        {
            if (instance == null)
            {
                instance = new Utils();
            }
            return instance;
        }

        public static bool AreArraysEqual(string[] array1, string[] array2)
        {
            // Check if lengths are the same
            if (array1.Length != array2.Length)
                return false;

            // Ccompare
            return array1.SequenceEqual(array2);
        }

        public static MeasurementsRecord[] parseCSVMeasurements(string csv, string separator)
        {
            string[] lines = csv.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            string[] headers = lines[0].Split(separator);

            if (!AreArraysEqual(headers, MeasurementsRecord.KNOWN_CSV_HEADERS))
            {
                throw new Exception("Unknown CSV format");
            }

            List<MeasurementsRecord> records = new List<MeasurementsRecord>();
            foreach (string line in lines) {
                if (line == lines[0] || String.Equals(line, String.Empty))
                {
                    continue;
                }
                string[] values = line.Split(separator);
                MeasurementsRecord record = new MeasurementsRecord(
                    values[0],
                    double.Parse(values[1], CultureInfo.InvariantCulture),
                    double.Parse(values[2], CultureInfo.InvariantCulture),
                    double.Parse(values[3], CultureInfo.InvariantCulture),
                    double.Parse(values[4], CultureInfo.InvariantCulture),
                    double.Parse(values[5], CultureInfo.InvariantCulture),
                    double.Parse(values[6], CultureInfo.InvariantCulture),
                    values[7]
                );

                records.Add(record);
            }

            return records.ToArray();
        }

        public static double Sum(IEnumerable<double> values)
        {
            double sum = 0;
            foreach (double value in values)
            {
                sum += value;
            }
            return sum;
        }
    }
}
