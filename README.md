# NetworkMonitor

This repository now also includes a small Python helper script, `speed_test.py`,
that you can run locally to measure your current internet connection speed using
[Speedtest.net](https://www.speedtest.net/).

## Running the speed test

1. Install the dependency once:
   ```bash
   pip install speedtest-cli
   ```
2. Run the script:
   ```bash
   python speed_test.py
   ```
   By default the script uses HTTPS to communicate with Speedtest.net servers.
3. Optional flags:
   - `--insecure`: use HTTP instead of HTTPS.
   - `--share`: print a shareable image URL for the test results.
   - `--timeout SECONDS`: set the HTTP timeout (default: 10 seconds).

Example output:
```
Internet speed test results:
  Download: 123.45 Mbps
  Upload:   67.89 Mbps
  Ping:     12.34 ms
  Server:   Example City (Country) via Example ISP
  Time:     2024-05-01T12:34:56
```

> Note: Running the test requires network access from the machine executing the
> script and may take up to a minute depending on connection quality.
