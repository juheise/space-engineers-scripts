// --- AIR VENT MONITOR SCRIPT ---
// v1.4 - Change-only logging (silent rescans)

// --- CONFIGURATION ---
const string MONITOR_TAG = "monitored=true";
const string DISPLAY_BLOCK_NAME = "Main Console (Nomad)";
const int SURFACE_INDEX = 1;
const float PRESSURE_THRESHOLD = 90f;
const int RESCAN_INTERVAL_RUNS = 6;

// --- PB LOG CONFIG ---
const int MAX_LOG_LINES = 20;

// --- UI CONFIGURATION ---
readonly Color TEXT_COLOR = new Color(0, 255, 255);
readonly Color WARNING_COLOR = new Color(255, 100, 0);

// --- SCRIPT STATE ---
private readonly List<IMyAirVent> _monitoredVents = new List<IMyAirVent>();
private int _runCounter = RESCAN_INTERVAL_RUNS;
private int _lastMonitoredCount = -1;

// --- PB DISPLAY STATE ---
IMyTextSurface _pbScreen;
Queue<string> _logLines = new Queue<string>();

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update100;

    var provider = Me as IMyTextSurfaceProvider;
    _pbScreen = provider.GetSurface(0);

    _pbScreen.ContentType = ContentType.TEXT_AND_IMAGE;
    _pbScreen.Font = "Monospace";
    _pbScreen.FontSize = 0.6f;
    _pbScreen.Alignment = TextAlignment.LEFT;
}

public void Main(string argument, UpdateType updateSource)
{
    _runCounter++;
    bool didRescan = false;
    bool rescanChanged = false;

    if (_runCounter >= RESCAN_INTERVAL_RUNS || argument.ToLower() == "rescan")
    {
        didRescan = true;
        rescanChanged = RescanVents();
    }

    IMyTerminalBlock block = GridTerminalSystem.GetBlockWithName(DISPLAY_BLOCK_NAME);
    if (block == null)
    {
        Log($"Error: No block named '{DISPLAY_BLOCK_NAME}' found!");
        FlushLog();
        return;
    }

    IMyTextSurfaceProvider provider = block as IMyTextSurfaceProvider;
    if (provider == null)
    {
        Log($"Error: the block '{DISPLAY_BLOCK_NAME}' has no LCD surfaces!");
        FlushLog();
        return;
    }

    IMyTextSurface lcd = provider.GetSurface(SURFACE_INDEX);
    DrawUI(lcd);

    // Log ONLY if the monitored set changed
    if (didRescan && rescanChanged)
    {
        Log($"Monitoring {_monitoredVents.Count} vent(s).");
        _lastMonitoredCount = _monitoredVents.Count;
    }

    FlushLog();
}

// --- PB LOGGING ---
void Log(string text)
{
    _logLines.Enqueue(text);
    while (_logLines.Count > MAX_LOG_LINES)
        _logLines.Dequeue();
}

void FlushLog()
{
    StringBuilder sb = new StringBuilder();
    foreach (var line in _logLines)
        sb.AppendLine(line);

    _pbScreen.WriteText(sb.ToString());
}

// --- UI DRAWING ---
void DrawUI(IMyTextSurface lcd)
{
    lcd.ContentType = ContentType.TEXT_AND_IMAGE;
    lcd.Font = "Monospace";
    lcd.FontColor = TEXT_COLOR;
    lcd.Alignment = TextAlignment.CENTER;

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

// --- VENT SCANNING ---
// Returns true if the monitored vent set changed
bool RescanVents()
{
    List<IMyAirVent> allVents = new List<IMyAirVent>();
    GridTerminalSystem.GetBlocksOfType(allVents);

    List<IMyAirVent> newList = new List<IMyAirVent>();
    foreach (var vent in allVents)
    {
        if (vent.CustomData.Contains(MONITOR_TAG))
            newList.Add(vent);
    }

    bool changed = newList.Count != _monitoredVents.Count;

    if (changed)
    {
        _monitoredVents.Clear();
        _monitoredVents.AddRange(newList);
    }

    _runCounter = 0;
    return changed;
}
