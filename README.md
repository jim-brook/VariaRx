# VariaRx
Receive data from Varia bike radar to PC using .NET (x86). You will need to have Windows, ANT+ dongle, and driver to run. The project uses
boilerplate code from Ant SDK to manage connection to device. The radar device profile is found in ANT+ProfileLib.dll. If anybody has 
the source for this .dll or a port to Linux please let me know and open an issue.

To do's:
Validate functionality of ThreatSideTargetX data from device

Validate against multiple targets

Validate target display form using multiple targets

Validate target display using valid ThreatSideTargetX data

Cover corner cases when "Stop" button is pressed

Add 'LED' to main form indicating device connection status

Lot's more stability testing

