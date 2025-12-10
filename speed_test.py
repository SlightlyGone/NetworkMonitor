#!/usr/bin/env python3
"""Run a quick internet speed test from the command line.

This script relies on the ``speedtest-cli`` package (``pip install speedtest-cli``)
for interacting with the Speedtest.net service. It reports download and upload
rates in Mbps along with ping latency in milliseconds.
"""
from __future__ import annotations

import argparse
import importlib.util
import sys
from dataclasses import dataclass
from datetime import datetime
from typing import Optional


@dataclass
class SpeedTestResult:
    """Container for speed test results."""

    download_mbps: float
    upload_mbps: float
    ping_ms: float
    timestamp: datetime
    server_name: str
    sponsor: str
    share_url: Optional[str] = None


def _ensure_speedtest_dependency():
    """Ensure the ``speedtest`` module is available and return it.

    The import is intentionally performed lazily to provide a clear error
    message when the dependency is missing without wrapping the import in
    try/except.
    """

    if importlib.util.find_spec("speedtest") is None:
        sys.exit(
            "The 'speedtest-cli' package is required. Install it with 'pip install speedtest-cli'."
        )

    import speedtest  # type: ignore

    return speedtest


def _format_mbps(bits_per_second: float) -> float:
    """Convert bits per second to megabits per second with two decimals."""

    return round(bits_per_second / (1000 ** 2), 2)


def _parse_timestamp(timestamp_value) -> datetime:
    """Convert a timestamp from ``speedtest`` results to ``datetime``.

    The ``speedtest`` library returns timestamps as ISO 8601 strings (e.g.,
    ``"2024-06-30T12:34:56.789Z"``). Older or alternative implementations may
    return UNIX epoch numbers. This helper normalizes both cases.
    """

    if isinstance(timestamp_value, (int, float)):
        return datetime.fromtimestamp(timestamp_value)

    if isinstance(timestamp_value, str):
        cleaned = timestamp_value.replace("Z", "+00:00")
        try:
            return datetime.fromisoformat(cleaned)
        except ValueError:
            pass

    raise TypeError("Unexpected timestamp value from speedtest results")


def run_speed_test(share: bool = False, secure: bool = True, timeout: int = 10) -> SpeedTestResult:
    """Execute a speed test and return structured results."""

    speedtest = _ensure_speedtest_dependency()

    tester = speedtest.Speedtest(timeout=timeout, secure=secure)
    tester.get_servers()
    tester.get_best_server()

    download_bps = tester.download()
    upload_bps = tester.upload()

    results = tester.results
    share_url = results.share() if share else None

    server = results.server
    server_name = f"{server.get('name', 'Unknown')} ({server.get('country', '')})"

    return SpeedTestResult(
        download_mbps=_format_mbps(download_bps),
        upload_mbps=_format_mbps(upload_bps),
        ping_ms=results.ping,
        timestamp=_parse_timestamp(results.timestamp),
        server_name=server_name,
        sponsor=server.get("sponsor", "Unknown"),
        share_url=share_url,
    )


def _build_arg_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Measure internet connection speed.")
    parser.add_argument(
        "--insecure",
        action="store_true",
        help="Use HTTP instead of HTTPS when communicating with Speedtest.net servers.",
    )
    parser.add_argument(
        "--share",
        action="store_true",
        help="Generate and display a shareable image URL for the test results.",
    )
    parser.add_argument(
        "--timeout",
        type=int,
        default=10,
        help="HTTP timeout in seconds for requests to Speedtest.net (default: 10).",
    )
    return parser


def main() -> int:
    parser = _build_arg_parser()
    args = parser.parse_args()

    result = run_speed_test(share=args.share, secure=not args.insecure, timeout=args.timeout)

    print("Internet speed test results:")
    print(f"  Download: {result.download_mbps} Mbps")
    print(f"  Upload:   {result.upload_mbps} Mbps")
    print(f"  Ping:     {result.ping_ms} ms")
    print(f"  Server:   {result.server_name} via {result.sponsor}")
    print(f"  Time:     {result.timestamp.isoformat()}\n")

    if result.share_url:
        print(f"Shareable image: {result.share_url}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
