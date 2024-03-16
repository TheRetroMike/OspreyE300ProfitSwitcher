using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OspreyE300ProfitSwitcher
{
    public class ConfigCoin
    {
        public bool enabled { get; set; }
        public string? name { get; set; }
        public decimal hr { get; set; }
        public int pwr { get; set; }
        public decimal pwrRate { get; set; }
        public int vccint { get; set; }
        public int vcchbm { get; set; }
        public int cclock { get; set; }
        public int hbmclock { get; set; }
        public decimal minerFee { get; set; }
        public decimal poolFee { get; set; }
        public string? dev { get; set; }
        public string? algo { get; set; }
        public string? pool { get; set; }
        public string? wallet { get; set; }
    }
}
