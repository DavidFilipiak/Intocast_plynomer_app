using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntocastGasMeterApp.models
{
    class MeasurementsRecord
    {
        public static readonly string[] KNOWN_CSV_HEADERS =
        {
            "Datum",
            "Vm_act (m^3)",
            "Vb_act (Nm^3)",
            "Abs. tlak_act (kPa)",
            "Teplota_act (°C)",
            "Neprep. poč. arch. (m^3)",
            "Prep. poč. arch. (Nm^3)",
            "Dátum archívu"
        };

        public DateTime Date { get; set; }
        public double DeviceRaw { get; set; }
        public double DeviceNormal { get; set; }        
        public double Pressure { get; set; }
        public double Temperature { get; set; }
        public double DeviceRawArchived { get; set; }
        public double DeviceNormalArchived { get; set; }
        public DateTime ArchiveDate { get; set; }

        // computed properties
        public double ActualUsage { get; set; }
        public double AccumulatedUsage { get; set; }

        public MeasurementsRecord(
           string _date, 
           double _deviceRaw, 
           double _deviceModified,            
           double _pressure,
           double _temperature,
           double _deviceRawArchived, 
           double _deviceModifiedArchived, 
           string _archiveDate
        )
        {
            Date = DateTime.ParseExact(_date, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture); ;
            DeviceRaw = _deviceRaw;
            DeviceNormal = _deviceModified;            
            Pressure = _pressure;
            Temperature = _temperature;
            DeviceRawArchived = _deviceRawArchived;
            DeviceNormalArchived = _deviceModifiedArchived;
            ArchiveDate = DateTime.ParseExact(_archiveDate, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            return "{\n" +
                "\"Date\":\"" + this.Date + "\"\n" +
                "\"DeviceRaw\":" + this.DeviceRaw + "\n" +
                "\"DeviceModified\":" + this.DeviceNormal + "\n" +
                "\"Temperature\":" + this.Temperature + "\n" +
                "\"Pressure\":" + this.Pressure + "\n" +
                "\"DeviceRawArchived\":" + this.DeviceRawArchived + "\n" +
                "\"DeviceModifiedArchived\":" + this.DeviceNormalArchived + "\n" +
                "\"ArchiveDate\":\"" + this.ArchiveDate + "\"\n" +
                "}";
        }

        private dynamic HeaderToVarMapper(string header)
        {
            switch (header)
            {
                case "Datum":
                    return Date;
                case "Vm_act (m^3)":
                    return DeviceRaw;
                case "Vb_act (Nm^3)":
                    return DeviceNormal;
                case "Abs. tlak_act (kPa)":
                    return Pressure;
                case "Teplota_act (°C)":
                    return Temperature;
                case "Neprep. poč. arch. (m^3)":
                    return DeviceRawArchived;
                case "Prep. poč. arch. (Nm^3)":
                    return DeviceNormalArchived;
                case "Dátum archívu":
                    return ArchiveDate;
                default:
                    throw new ArgumentException("Unknown header");
            }
        }

        // indexer accessor like in arrays
        public dynamic this[string header]
        {
            get
            {
                return HeaderToVarMapper(header);
            }
        }

        public static string ToCompoundString(MeasurementsRecord[] records)
        {
            // first line is headers and then their respective values in order
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Join("; ", MeasurementsRecord.KNOWN_CSV_HEADERS));
            sb.Append("\n");
            foreach (MeasurementsRecord record in records)
            {
                foreach (string header in MeasurementsRecord.KNOWN_CSV_HEADERS)
                {
                    sb.Append(record[header]);
                    sb.Append("; ");
                }
                sb.Append("\n");
            }

            return sb.ToString();
        }
    }
}
