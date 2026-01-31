// --- GRAPHICAL AGGREGATE BATTERY MONITOR SCRIPT ---
// v2.1 - Change-only PB logging (matches air vent monitor)

// --- CONFIGURATION ---
const string MONITOR_TAG = "monitored=true";
const int RESCAN_INTERVAL_RUNS = 6;
const string DISPLAY_BLOCK_NAME = "Main Console (Nomad)";
const int SURFACE_INDEX = 0;

// --- PB LOG CONFIG ---
const int MAX_LOG_LINES = 20;

// --- UI CONFIGURATION ---
readonly Color BG_COLOR = new Color(10, 25, 35, 255);
readonly Color BAR_BG_COLOR = new Color(0, 0, 0, 150);
readonly Color CHARGE_COLOR = new Color(120, 190, 40, 255);
readonly Color INPUT_COLOR = new Color(40, 120, 190, 255);
readonly Color OUTPUT_COLOR = new Color(190, 120, 40, 255);
readonly Color TEXT_COLOR = new Color(200, 230, 250, 255);
readonly Color HEADER_COLOR = Color.White;

// --- SCRIPT STATE ---
private readonly List<IMyBatteryBlock> _monitoredBatteries = new List<IMyBatteryBlock>();
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
        rescanChanged = RescanBatteries();
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

    // --- AGGREGATE CALCULATIONS ---
    float totalStored = 0f, totalMaxStored = 0f;
    float totalInput = 0f, totalMaxInput = 0f;
    float totalOutput = 0f, totalMaxOutput = 0f;

    foreach (var battery in _monitoredBatteries)
    {
        if (battery.Closed) continue;

        totalStored += battery.CurrentStoredPower;
        totalMaxStored += battery.MaxStoredPower;
        totalInput += battery.CurrentInput;
        totalMaxInput += battery.MaxInput;
        totalOutput += battery.CurrentOutput;
        totalMaxOutput += battery.MaxOutput;
    }

    DrawUI(lcd, totalStored, totalMaxStored, totalInput, totalMaxInput, totalOutput, totalMaxOutput);

    // Log ONLY if monitored set changed
    if (didRescan && rescanChanged)
    {
        Log($"Monitoring {_monitoredBatteries.Count} battery(s).");
        _lastMonitoredCount = _monitoredBatteries.Count;
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
void DrawUI(IMyTextSurface lcd, float stored, float maxStored, float input, float maxInput, float output, float maxOutput)
{
    lcd.ContentType = ContentType.TEXT_AND_IMAGE;
    lcd.Font = "Monospace";
    float fontScale = Math.Max(0.6f, 1.5f - (lcd.SurfaceSize.Y / 512f));
    lcd.FontSize = fontScale;
    lcd.FontColor = TEXT_COLOR;
    lcd.Alignment = TextAlignment.LEFT;

    float chargePct = (maxStored > 0) ? (stored / maxStored) * 100f : 0f;
    float inputPct = (maxInput > 0) ? (input / maxInput) * 100f : 0f;
    float outputPct = (maxOutput > 0) ? (output / maxOutput) * 100f : 0f;

    StringBuilder sb = new StringBuilder();
    sb.AppendLine("=== Battery Status ===");
    sb.AppendLine($"In:     {inputPct,5:F1}%");
    sb.AppendLine($"Out:    {outputPct,5:F1}%");
    sb.AppendLine($"Charge: {chargePct,5:F1}%");

    if (_monitoredBatteries.Count == 0)
    {
        sb.AppendLine();
        sb.AppendLine("No monitored batteries found!");
        sb.AppendLine($"Tag with '{MONITOR_TAG}'");
    }

    lcd.WriteText(sb.ToString());
}

// --- BATTERY SCANNING ---
// Returns true if monitored battery set changed
bool RescanBatteries()
{
    List<IMyBatteryBlock> allBatteries = new List<IMyBatteryBlock>();
    GridTerminalSystem.GetBlocksOfType(allBatteries);

    List<IMyBatteryBlock> newList = new List<IMyBatteryBlock>();
    foreach (var battery in allBatteries)
    {
        if (battery.CustomData.Contains(MONITOR_TAG))
            newList.Add(battery);
    }

    bool changed = newList.Count != _monitoredBatteries.Count;

    if (changed)
    {
        _monitoredBatteries.Clear();
        _monitoredBatteries.AddRange(newList);
    }

    _runCounter = 0;
    return changed;
}
