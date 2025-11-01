// --- AIR VENT MONITOR SCRIPT ---
// v1.0 - Text-based display for low-pressure monitored vents.
//
// Lists all vents with Custom Data containing "monitored=true"
// whose room pressure is below a configurable threshold.
//
// Works with any block that has LCD surfaces (cockpits, consoles, LCD panels).

// --- CONFIGURATION ---
const string MONITOR_TAG = "monitored=true";  // tag in Custom Data
const string DISPLAY_BLOCK_NAME = "Control Seat (Nomad)"; // cockpit, console, or LCD name
const int SURFACE_INDEX = 4; // which screen to use (0–4 typical for cockpits)
const float PRESSURE_THRESHOLD = 90f; // percentage threshold for low pressure display
const int RESCAN_INTERVAL_RUNS = 6; // rescans every ~10 seconds

// --- UI CONFIGURATION ---
readonly Color TEXT_COLOR = new Color(0, 255, 255);
readonly Color WARNING_COLOR = new Color(255, 100, 0);

// --- SCRIPT STATE ---
private readonly List<IMyAirVent> _monitoredVents = new List<IMyAirVent>();
private int _runCounter = RESCAN_INTERVAL_RUNS;

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public void Main(string argument, UpdateType updateSource)
{
    _runCounter++;
    if (_runCounter >= RESCAN_INTERVAL_RUNS || argument.ToLower() == "rescan")
    {
        RescanVents();
    }

    IMyTerminalBlock block = GridTerminalSystem.GetBlockWithName(DISPLAY_BLOCK_NAME);
    if (block == null)
    {
        Echo($"Error: No block named '{DISPLAY_BLOCK_NAME}' found!");
        return;
    }

    IMyTextSurfaceProvider provider = block as IMyTextSurfaceProvider;
    if (provider == null)
    {
        Echo($"Error: the block '{DISPLAY_BLOCK_NAME}' has no LCD surfaces!");
        return;
    }

    IMyTextSurface lcd = provider.GetSurface(SURFACE_INDEX);
    DrawUI(lcd);
    Echo($"Monitoring {_monitoredVents.Count} vents.");
}

void DrawUI(IMyTextSurface lcd)
{
    // Set up text display
    lcd.ContentType = ContentType.TEXT_AND_IMAGE;
    lcd.Font = "Monospace";
    lcd.FontColor = TEXT_COLOR;

    // Auto-scale font for display size
    float fontScale = Math.Max(0.6f, 1.5f - (lcd.SurfaceSize.Y / 512f));
    lcd.FontSize = fontScale;
    lcd.Alignment = TextAlignment.LEFT;

    // Build text
    StringBuilder sb = new StringBuilder();
    sb.AppendLine("=== Air Vent Monitor ===");
    sb.AppendLine($"Threshold: {PRESSURE_THRESHOLD:F0}%");
    sb.AppendLine();

    int lowCount = 0;
    foreach (var vent in _monitoredVents)
    {
        if (vent.Closed) continue;
        float pressure = vent.GetOxygenLevel() * 100f;
        if (pressure < PRESSURE_THRESHOLD)
        {
            sb.AppendLine($"{vent.CustomName}: {pressure:F1}%");
            lowCount++;
        }
    }

    if (lowCount == 0)
    {
        sb.AppendLine("All monitored vents above threshold.");
    }
    else
    {
        sb.AppendLine();
        sb.AppendLine($"{lowCount} vent(s) below {PRESSURE_THRESHOLD:F0}%!");
    }

    if (_monitoredVents.Count == 0)
    {
        sb.AppendLine();
        sb.AppendLine("No monitored vents found!");
        sb.AppendLine($"Tag with '{MONITOR_TAG}'");
    }

    lcd.WriteText(sb.ToString());
}

void RescanVents()
{
    Echo("Rescanning for tagged air vents...");
    _monitoredVents.Clear();
    List<IMyAirVent> allVents = new List<IMyAirVent>();
    GridTerminalSystem.GetBlocksOfType(allVents);

    foreach (var vent in allVents)
    {
        if (vent.CustomData.Contains(MONITOR_TAG))
        {
            _monitoredVents.Add(vent);
        }
    }

    Echo($"Scan complete. Found {_monitoredVents.Count} vents.");
    _runCounter = 0;
}
