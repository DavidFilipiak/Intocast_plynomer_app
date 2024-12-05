using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    internal static class Consts
    {
        public static DateTime measureStart = new DateTime(2024, 12, 4, 6, 0, 0);
        public static string customerId = "Tester";
        public static string deviceId1 = "Device_1";
        public static string deviceId2 = "Device_2";

        public static string dataStream_headers = "Datum;Vm_act (m^3);Vb_act (Nm^3);Abs. tlak_act (kPa);Teplota_act (°C);Neprep. poč. arch. (m^3);Prep. poč. arch. (Nm^3);Dátum archívu";

        private static int[] working_increment_values = new int[6] { 100, 10, 20, 1, 0, 0 };
        private static int[] notWorking_increment_values = new int[6] { 0, 0, 0, 0, 0, 0 };
        private static string archiveDate = "12/04/2024 06:00:00";
        public static string GetDataStream(int? missingIndex, bool startWorking, int?[] switchIndex)
        {
            int[] working_default_values = working_default_values = new int[6] { 1500, 700, 440, 19, 1500, 700 };
            int[] notWorking_default_values = new int[6] { 1000, 500, 400, 5, 1000, 500 };
            DateTime date = new DateTime(2024, 12, 4, 6, 2, 0);

            StringBuilder sb = new StringBuilder();
            sb.Append(dataStream_headers);
            sb.Append("\r\n");

            if (missingIndex == Int32.MaxValue)
            {
                return sb.ToString();
            }

            int[] values = startWorking ? working_default_values : notWorking_default_values;
            for (int i = 0; i < 5; i++)
            {

                if (switchIndex is not null && switchIndex.Contains(i)) startWorking = !startWorking;
                if (missingIndex == i)
                {
                    date = date.AddMinutes(5);
                    continue;
                }

                sb.Append(date.ToString("dd.MM.yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(";");
                for (int j = 0; j < 6; j++)
                {
                    if (startWorking)
                    {
                        values[j] += working_increment_values[j];
                    }
                    else
                    {
                        values[j] += notWorking_increment_values[j];
                    }

                    sb.Append(values[j]);
                    sb.Append(";");
                }

                sb.Append(archiveDate);
                sb.Append("\r\n");
                date = date.AddMinutes(5);
            }

            return sb.ToString();
        }
    }
}
