using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DERMS_Server
{
    public class Production
    {
        public string Id { get; set; }
        public double ActivePower { get; set; }
        public double ReactivePower { get; set; }

        public string Type
        {
            get
            {
                if (Id.StartsWith("SP"))
                    return "Solar";
                else if (Id.StartsWith("WT"))
                    return "Wind";
                else
                    return "Unknown";
            }
        }
    }
}
