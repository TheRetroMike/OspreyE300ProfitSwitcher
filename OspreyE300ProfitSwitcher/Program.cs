
using Newtonsoft.Json;
using OspreyE300ProfitSwitcher;
using RestSharp;
using System.Dynamic;
using System.Globalization;
using System.Net;

List<HashrateNoCoin> hashrateNoCoins = new List<HashrateNoCoin>();

var configData = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Miners.json"));


if (configData != null && configData.Miners != null)
{
    var coinProfitThreshold = configData?.CoinProfitThreshold / 100;

    if (!String.IsNullOrEmpty(configData.HashrateNoApiKey))
	{
		if (!File.Exists("hashratedata.json"))
		{
			RefreshData();
		}
		else
		{
			var fileInfo = new FileInfo("hashratedata.json");
			if (fileInfo.CreationTime <= DateTime.Now.AddHours(-1))
			{
				RefreshData();
			}
		}

        if (configData.Miners.Where(x => x.enabled).ToList().Count > 0)
		{
            hashrateNoCoins = JsonConvert.DeserializeObject<List<HashrateNoCoin>>(File.ReadAllText("hashratedata.json")) ?? new List<HashrateNoCoin>();

            foreach (var miner in configData.Miners.Where(x => x.enabled))
			{
				Console.WriteLine(String.Format("Processing Miner: {0}", miner.name));
				List<ProfitData> coinProfits = new List<ProfitData>();
				var configuredCoins = miner?.coins?.Where(x => x.enabled).ToList();

				if (configuredCoins != null)
				{
					foreach (var coin in configuredCoins)
					{
						var hashrateNoCoin = hashrateNoCoins?.Where(x => x.coin == coin.name).FirstOrDefault();
						if (hashrateNoCoin != null)
						{
							string uom = hashrateNoCoin?.coinEstimateUnit ?? "";
							string usdEstimate = hashrateNoCoin?.usdEstimate ?? "";
							var estimatedEarnings = double.Parse(usdEstimate, CultureInfo.InvariantCulture) * Convert.ToDouble(coin.hr);
							var minerFee = Convert.ToDecimal(coin.minerFee) / 100;
							var poolFee = Convert.ToDecimal(coin.poolFee) / 100;
							var powerCost = Convert.ToDecimal(coin.pwr * 24 / 1000) * Convert.ToDecimal(coin.pwrRate);
							var estimatedProfit = Convert.ToDecimal(estimatedEarnings) - (Convert.ToDecimal(estimatedEarnings) * minerFee);
							estimatedProfit = estimatedProfit - (estimatedProfit * poolFee);
							estimatedProfit = estimatedProfit - powerCost;
							coinProfits.Add(new ProfitData() { coin = coin.name, earnings = Convert.ToDecimal(estimatedEarnings), profit = estimatedProfit });
						}
					}
				}

				var topCoin = coinProfits.OrderByDescending(x => x.profit).FirstOrDefault();

				//Get current running bitstream
				RestClient minerClient = new RestClient(String.Format("http://{0}/cgi-bin/qcmap_web_cgi.cgi", miner?.ip ?? "127.0.0.1"));
				RestRequest minerRequest = new RestRequest();
				minerRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");
				minerRequest.AddParameter("Page", "getDracaMinerStatus");
				var minerResponse = minerClient.Post(minerRequest);
				dynamic minerResponseContent = JsonConvert.DeserializeObject(minerResponse.Content ?? "") ?? new ExpandoObject();
				string currentDev = minerResponseContent?.developer ?? "";
				string currentStatus = minerResponseContent?.status ?? "";
                string currentAlgo = minerResponseContent?.info?.a ?? "";
                string currentPool = minerResponseContent?.info?.o ?? "";
                string currentWallet = minerResponseContent?.info?.u ?? "";
                string currentMiner = minerResponseContent?.info?.miner ?? "";
                string currentCClock = minerResponseContent?.info?.fpga_clk_core ?? "";

				var currentCoin = configuredCoins?.Where(x => x.algo==currentAlgo && x.pool==currentPool && x.wallet==currentWallet).FirstOrDefault();

				bool profitSwitch = false;
				if(currentCoin==null)
				{
					profitSwitch = true;
				}
				else
				{
					var currentProfit = coinProfits.Where(x => x.coin==currentCoin.name).FirstOrDefault();
					if (currentProfit == null)
					{
						profitSwitch = true;
					}
					else
					{
						var currentProfitAmt = currentProfit.profit;
						var newProfitAmt = topCoin?.profit;
						if(newProfitAmt > (currentProfit.profit+(currentProfit.profit*coinProfitThreshold)))
						{
							profitSwitch = true;
						}
					}
				}

                var newCoin = configuredCoins?.Where(x => x.name == topCoin?.coin).FirstOrDefault();
				if (newCoin != null && profitSwitch)
				{
					var newDev = newCoin.dev;
					var newAlgo = newCoin.algo;

					if (newAlgo != Convert.ToString(currentAlgo) || Convert.ToInt32(currentStatus) == 0)
					{
						if (currentStatus != "0")
						{
							//call api to stop miner
							minerClient = new RestClient(String.Format("http://{0}/cgi-bin/qcmap_web_cgi.cgi", miner?.ip));
							minerRequest = new RestRequest();
							minerRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");
							minerRequest.AddParameter("Page", "setDracaenaMiner");
							minerRequest.AddParameter("status", "0");
							minerRequest.AddParameter("info_a", currentDev);
							minerRequest.AddParameter("info_o", currentPool);
							minerRequest.AddParameter("info_u", currentWallet);
							minerRequest.AddParameter("info_miner", currentMiner);
							minerRequest.AddParameter("info_fpga_clk_core", currentCClock);
							minerRequest.AddParameter("info_fpga_clk_core1", currentCClock);
							minerRequest.AddParameter("info_fpga_clk_core2", currentCClock);
							minerRequest.AddParameter("info_fpga_clk_mem", "1100");
							minerRequest.AddParameter("info_fpga_clk_mem1", "1100");
							minerRequest.AddParameter("info_fpga_clk_mem2", "1100");
							minerRequest.AddParameter("auto_start", "1");
							minerRequest.AddParameter("running_mode", "0");
							minerRequest.AddParameter("input_in_oneline", "+");
							minerRequest.AddParameter("input_in_oneline1", "+");
							minerRequest.AddParameter("input_algo", currentAlgo);
							minerRequest.AddParameter("token", "1");
							minerResponse = minerClient.Post(minerRequest);

						}

						//call api to set voltage
						minerClient = new RestClient(String.Format("http://{0}:8200/controller/setVoltage", miner?.ip));
						minerRequest = new RestRequest();
						minerRequest.AddJsonBody("{\"voltage_vccint\":" + newCoin.vccint + ", \"voltage_hbm\": " + newCoin.vcchbm + ", \"boardId\": 3}");
						minerResponse = minerClient.Post(minerRequest);

						//call api to change settings and start miner
						minerClient = new RestClient(String.Format("http://{0}/cgi-bin/qcmap_web_cgi.cgi", miner?.ip));
						minerRequest = new RestRequest();
						minerRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");
						minerRequest.AddParameter("Page", "setDracaenaMiner");
						minerRequest.AddParameter("status", "1");
						minerRequest.AddParameter("info_a", newCoin.dev);
						minerRequest.AddParameter("info_o", newCoin.pool);
						minerRequest.AddParameter("info_u", newCoin.wallet);
						minerRequest.AddParameter("info_miner", miner?.workername);
						minerRequest.AddParameter("info_fpga_clk_core", newCoin.cclock);
						minerRequest.AddParameter("info_fpga_clk_core1", newCoin.cclock);
						minerRequest.AddParameter("info_fpga_clk_core2", newCoin.cclock);
						minerRequest.AddParameter("info_fpga_clk_mem", newCoin.hbmclock);
						minerRequest.AddParameter("info_fpga_clk_mem1", newCoin.hbmclock);
						minerRequest.AddParameter("info_fpga_clk_mem2", newCoin.hbmclock);
						minerRequest.AddParameter("auto_start", "1");
						minerRequest.AddParameter("running_mode", "0");
						minerRequest.AddParameter("input_in_oneline", "+");
						minerRequest.AddParameter("input_in_oneline1", "+");
						minerRequest.AddParameter("input_algo", newCoin.algo);
						minerRequest.AddParameter("token", "1");
						minerResponse = minerClient.Post(minerRequest);

                        Console.WriteLine(String.Format("Changing Miner {0} to hash {1}. Projected Profit: {2}", miner?.name, topCoin?.coin, topCoin?.profit));
                    }
                }
			}
		}
		else
		{
			Console.WriteLine("No enabled miners configured");
		}
	}
	else
	{
        Console.Error.WriteLine("Missing Hashrate.no API Key");
    }
}
else
{
    Console.Error.WriteLine("Unable to parse config file");
}

void RefreshData()
{
	if (!String.IsNullOrEmpty(configData.HashrateNoApiKey))
	{
		WebClient webClient = new WebClient();
		webClient.DownloadFile(String.Format("https://api.hashrate.no/v1/coins?apiKey={0}", configData.HashrateNoApiKey), "hashratedata.json");
    }
}