# Argus EDR v2.1 - Security Model and Known Limitations

## Threat Model

Argus EDR v2.1 is a usermode endpoint detection and response system designed for personal Windows hardening and basic malware detection. It protects against common threats but has architectural limitations compared to enterprise EDRs with kernel drivers.

## What Argus Detects

- File-based malware (YARA rules)
- Fileless/script attacks (AMSI integration)
- Suspicious process chains (ETW process creation monitoring)
- Registry persistence (ETW registry change monitoring)
- Privacy toggle tampering (continuous GuardMonitor)
- Binary tampering of Argus itself (canary files + integrity verification)

## Known Limitations (v2.1)

These are architectural boundaries, not bugs:

- **No kernel driver:** Cannot block file writes before they happen (TOCTTOU gap). FileSystemWatcher detects writes after completion. Planned for v2.2: minifilter driver.
- **No memory injection detection:** Cannot detect WriteProcessMemory, VirtualAllocEx, or process hollowing attacks. Requires kernel callbacks (ObRegisterCallbacks).
- **No DLL sideloading detection:** Does not validate DLL load order or path hijacking.
- **No credential theft prevention:** Does not monitor LSASS, DPAPI key access, or SAM registry dumping (e.g., Mimikatz).
- **No network-level filtering:** Does not inspect or block network traffic. DNS protection is DNS-server-level only (Cloudflare), not packet-level (WFP).
- **No lateral movement detection:** Does not monitor SMB, WMI, or WinRM for reconnaissance or lateral execution patterns.
- **Process protection:** Watchdog service can be killed by admin-level processes. PPL (Protected Process Light) requires Microsoft code signing (not available for open source). SCM auto-restart mitigates this (1-second recovery).
- **Single machine:** No centralized management, GPO integration, or SIEM forwarding. Logs are local only (with Windows Event Log as secondary sink).
- **.NET reversibility:** C#/.NET assemblies can be decompiled. Detection logic in Argus.Engine is visible to attackers.

## v2.2 Roadmap

1. Full ETW file monitoring (replace FileSystemWatcher)
2. Minifilter kernel driver for real-time file blocking
3. Memory injection detection via ETW Microsoft-Windows-Kernel-Audit-API-Calls
4. SIEM integration (syslog forwarding)
5. Signed update channel for YARA rules and binaries
6. Windows Home edition compatibility audit for all 26 Guard toggles

## Security Invariants

Argus enforces these security guarantees:

1. **Fail closed:** If any scanner throws an exception, the result is `ThreatResult.Unknown`, never `Clean`. Unknown results are treated as Suspicious.
2. **IPC authentication:** Every pipe message carries a 32-byte HMAC-SHA256 signature. Invalid messages are rejected and logged.
3. **DPAPI for all keys:** All encryption keys use `DataProtectionScope.LocalMachine`. No plaintext key storage.
4. **Bounded event processing:** All monitors feed into a `Channel<MonitorEvent>` with 50,000-event capacity. Overflow drops oldest events.
5. **SQLite WAL mode:** All database connections use WAL mode to prevent cross-process lock contention.
6. **YARA rule integrity:** Rule files are verified against a DPAPI-protected SHA-256 manifest on every load.

## Privacy Guard

Argus includes a 26-toggle Privacy Guard that hardens Windows telemetry, cloud sync, advertising, and network settings. These toggles are stored in `GuardConfig.json` and can be configured through the GUI.

## Version

Argus EDR v2.1
