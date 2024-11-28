using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntocastGasMeterApp.models
{
    class MasterData
    {
        public int id { get; set; }
        public int parentId { get; set; }
        public string name { get; set; }
        public int type { get; set; }
        public string placeGuid { get; set; }
        public string dealerId { get; set; }
        public string customer { get; set; }
        public string customerId { get; set; }
        public string street { get; set; }
        public string customerNumber { get; set; }
        public string city { get; set; }
        public string deviceNumber { get; set; }
        public bool consumptionTerminated { get; set; }
        public string dataPrepaidTo { get; set; }
        public string tariff { get; set; }
        public bool leaf { get; set; }
        
        public MasterData() { }


        public override string ToString()
        {
            // list all properties in json format
            return "{\n" +
                "\"id\":" + this.id + "\n" +
                "\"parentId\":" + this.parentId + "\n" +
                "\"name\":\"" + this.name + "\"\n" +
                "\"type\":" + this.type + "\n" +
                "\"placeGuid\":\"" + this.placeGuid + "\"\n" +
                "\"dealerId\":\"" + this.dealerId + "\"\n" +
                "\"customer\":\"" + this.customer + "\"\n" +
                "\"customerId\":\"" + this.customerId + "\"\n" +
                "\"street\":\"" + this.street + "\"\n" +
                "\"customerNumber\":\"" + this.customerNumber + "\"\n" +
                "\"city\":\"" + this.city + "\"\n" +
                "\"deviceNumber\":\"" + this.deviceNumber + "\"\n" +
                "\"consumptionTerminated\":" + this.consumptionTerminated + "\n" +
                "\"dataPrepaidTo\":\"" + this.dataPrepaidTo + "\"\n" +
                "\"tariff\":\"" + this.tariff + "\"\n" +
                "\"leaf\":\"" + this.leaf + "\"\n" +
                "}";
        }
    }
}
