# Osprey E300 Profit Switcher

This is a cross-platform app that can run on Windows, Linux, or MacOS that will leverage the Hashrate.no calculator, determine what is the most profitable coin for the Osprey lineup of standalone FPGA units, apply voltage changes, apply clock and pool settings, and auto switch bitstreams based on a given threshold.

All settings are stored in a Miners.json file. Simply adjust that file to meet your needs.

## Linux Installer
Run the below script to install the dependencies, download the binaries, and set a scheduled job to run every hour. If you want to change the frequency of the runs, you can do so by adjusting the crontab record that gets generated.
Install Video / Walkthrough: TBD

```
wget -O - https://raw.githubusercontent.com/TheRetroMike/OspreyE300ProfitSwitcher/main/install.sh | bash
```

## Windows
Download the release zip file and run the exe. Recommended method is to setup a scheduled task in windows.
