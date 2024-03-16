#!/usr/bin/env bash

wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
apt-get update
apt-get install -y dotnet-sdk-6.0 unzip
wget https://github.com/TheRetroMike/OspreyE300ProfitSwitcher/releases/download/0.0.1/OspreyE300ProfitSwitcher.zip
unzip OspreyE300ProfitSwitcher.zip -d /OspreyE300ProfitSwitcher
rm OspreyE300ProfitSwitcher.zip
(crontab -l 2>/dev/null; echo "0 * * * * cd /OspreyE300ProfitSwitcher && dotnet OspreyE300ProfitSwitcher.dll") | crontab -
cd /OspreyE300ProfitSwitcher && dotnet OspreyE300ProfitSwitcher.dll
