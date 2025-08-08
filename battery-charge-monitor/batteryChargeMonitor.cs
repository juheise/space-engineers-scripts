// --- AGGREGATE BATTERY MONITOR SCRIPT ---
// Displays the combined charge, input, and output of all batteries
// that have the specified tag in their Custom Data.
// This script is optimized to only scan for batteries periodically.

// --- CONFIGURATION ---
// The tag to look for in the Custom Data of each battery.
const string MONITOR_TAG = "monitored=true";
// The name of your LCD panel.
const string LCD_NAME = "LCD Panel Status";

// --- OPTIMIZATION ---
// How often to rescan for new/removed batteries. This is a multiplier for the script's run interval (~1.6s).
// A value of 6 means it will rescan roughly every 10 seconds (6 * 1.6s).
// Set to 1 to scan every time.
const int RESCAN_INTERVAL_RUNS = 6;

// --- PROGRESS BAR SETTINGS ---
const int PROGRESS_BAR_WIDTH = 22;
const char FILLED_CHAR = '=';
const char EMPTY_CHAR = '-';

// --- SCRIPT STATE (DO NOT MODIFY) ---
private readonly List<IMyBatteryBlock> _monitoredBatteries = new List<IMyBatteryBlock>();
private int _runCounter = RESCAN_INTERVAL_RUNS; // Start with a rescan on the first run.

public Program()
{
    // This makes the script run automatically every 100 game ticks (about 1.6 seconds).
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public void Main(string argument, UpdateType updateSource)
{
    // --- SCANNING ---
    // Increment the counter to track when to rescan.
    _runCounter++;

    // Rescan for batteries on schedule or when manually triggered with the "rescan" argument.
    if (_runCounter >= RESCAN_INTERVAL_RUNS || argument.ToLower() == "rescan")
    {
        RescanBatteries();
    }

    // --- BLOCK RETRIEVAL ---
    IMyTextPanel lcd = GridTerminalSystem.GetBlockWithName(LCD_NAME) as IMyTextPanel;
    if (lcd == null)
    {
        Echo($"Error: LCD Panel '{LCD_NAME}' not found.");
        return;
    }

    // --- ERROR HANDLING ---
    if (_monitoredBatteries.Count == 0)
    {
        Echo("Warning: No batteries with the tag '" + MONITOR_TAG + "'were found.");
        lcd.WriteText("Warning: No batteries with the Custom Data tag: '" + MONITOR_TAG + "' were found.", false);
        lcd.ContentType = ContentType.TEXT_AND_IMAGE;
        return;
    }

    // --- AGGREGATE CALCULATIONS ---
    float totalStored = 0f;
    float totalMaxStored = 0f;
    float totalInput = 0f;
    float totalMaxInput = 0f;
    float totalOutput = 0f;
    float totalMaxOutput = 0f;

    foreach (IMyBatteryBlock battery in _monitoredBatteries)
    {
        if (battery.Closed) continue; 
        
        totalStored += battery.CurrentStoredPower;
        totalMaxStored += battery.MaxStoredPower;
        totalInput += battery.CurrentInput;
        totalMaxInput += battery.MaxInput;
        totalOutput += battery.CurrentOutput;
        totalMaxOutput += battery.MaxOutput;
    }
    float chargePercent = (totalMaxStored > 0) ? (totalStored / totalMaxStored) * 100f : 0f;

    // --- STRING BUILDING ---
    string chargeBar = CreateProgressBar(totalStored, totalMaxStored);
    string inputBar = CreateProgressBar(totalInput, totalMaxInput);
    string outputBar = CreateProgressBar(totalOutput, totalMaxOutput);

    string output = ""; 
    output += "AGGREGATE BATTERY STATUS\n";
    output += "========================\n";
    output += "\n";
    output += $"CHARGE: {chargeBar} {chargePercent:F1}%\n";
    output += $"({totalStored:F2}/{totalMaxStored:F2} MWh)\n";
    output += $"\n";
    output += $"INPUT:  {inputBar} {totalInput:F2} MW\n";
    output += $"\n";
    output += $"OUTPUT: {outputBar} {totalOutput:F2} MW\n";

    // --- DISPLAY ---
    lcd.WriteText(output, false);
    lcd.ContentType = ContentType.TEXT_AND_IMAGE;
    
    Echo("Script ran successfully.");
    Echo($"Monitoring {_monitoredBatteries.Count} cached batteries.");
}

// RescanBatteries tries to find batteries tagged to be monitored.
// This could be expensive, depending on the grid's size.
void RescanBatteries()
{
    Echo("Rescanning for tagged batteries...");
    _monitoredBatteries.Clear();
    
    List<IMyBatteryBlock> allBatteries = new List<IMyBatteryBlock>();
    GridTerminalSystem.GetBlocksOfType(allBatteries);
    
    foreach (IMyBatteryBlock battery in allBatteries)
    {
        if (battery.CustomData.Contains(MONITOR_TAG))
        {
            _monitoredBatteries.Add(battery);
        }
    }
    Echo($"Scan complete. Found {_monitoredBatteries.Count} batteries.");
    _runCounter = 0;
}

// CreateProgressBar generates a string-representation of a progress bar.
public string CreateProgressBar(float currentValue, float maxValue)
{
    if (maxValue <= 0)
    {
        return "[" + new String(EMPTY_CHAR, PROGRESS_BAR_WIDTH) + "]";
    }

    float percent = currentValue / maxValue;
    int filledBlocks = (int)Math.Round(percent * PROGRESS_BAR_WIDTH);
    filledBlocks = Math.Min(PROGRESS_BAR_WIDTH, Math.Max(0, filledBlocks));

    return "[" + new String(FILLED_CHAR, filledBlocks) + new String(EMPTY_CHAR, PROGRESS_BAR_WIDTH - filledBlocks) + "]";
}
