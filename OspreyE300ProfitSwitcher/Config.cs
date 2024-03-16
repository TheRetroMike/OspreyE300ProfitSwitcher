using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OspreyE300ProfitSwitcher
{
    public class Config
    {
        public string? HashrateNoApiKey { get; set; }
        public int CoinProfitThreshold { get; set; }
        public List<ConfigMiners>? Miners { get; set; }
    }
}
