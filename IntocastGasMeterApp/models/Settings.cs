using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntocastGasMeterApp.models
{
    internal class Settings
    {
        public string sessionId { get; set; } = "";
        public string language { get; set; } = "sk";

        public bool runOnStartup { get; set; } = true;
    }
}
