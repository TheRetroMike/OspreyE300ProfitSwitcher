using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OspreyE300ProfitSwitcher
{
    public class ConfigMiners
    {
        public bool enabled { get; set; }
        public string? name { get; set; }
        public string? workername { get; set; }
        public string? ip { get; set; }
        public List<ConfigCoin>? coins { get; set; }
    }
}
