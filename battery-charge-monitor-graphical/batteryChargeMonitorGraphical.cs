// --- GRAPHICAL AGGREGATE BATTERY MONITOR SCRIPT ---
// v2.0 - Uses sprites for a graphical UI instead of text.
// Displays the combined charge, input, and output of all batteries
// that have the specified tag in their Custom Data.

// --- CONFIGURATION ---
const string MONITOR_TAG = "monitored=true";
const string LCD_NAME = "LCD Panel Status";
const int RESCAN_INTERVAL_RUNS = 6; // Rescans roughly every 10 seconds.

// --- UI CONFIGURATION ---
// --- Colors (R, G, B, Alpha) ---
readonly Color BG_COLOR = new Color(10, 25, 35, 255);       // Panel background color
readonly Color BAR_BG_COLOR = new Color(0, 0, 0, 150);         // Progress bar background
readonly Color CHARGE_COLOR = new Color(120, 190, 40, 255);    // Green
readonly Color INPUT_COLOR = new Color(40, 120, 190, 255);     // Blue
readonly Color OUTPUT_COLOR = new Color(190, 120, 40, 255);    // Orange
readonly Color TEXT_COLOR = new Color(200, 230, 250, 255);    // Light blue text
readonly Color HEADER_COLOR = Color.White;

// --- Layout ---
const float FONT_SIZE = 1.3f;
const float HEADER_FONT_SIZE = 1.8f;
const float BAR_HEIGHT = 30f; // Height of the progress bars in pixels
const float PADDING = 25f;    // Padding from the edges of the screen

// --- SCRIPT STATE (DO NOT MODIFY) ---
private readonly List<IMyBatteryBlock> _monitoredBatteries = new List<IMyBatteryBlock>();
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
        RescanBatteries();
    }

    IMyTextPanel lcd = GridTerminalSystem.GetBlockWithName(LCD_NAME) as IMyTextPanel;
    if (lcd == null)
    {
        Echo($"Error: LCD Panel '{LCD_NAME}' not found.");
        return;
    }

    // --- AGGREGATE CALCULATIONS ---
    float totalStored = 0f, totalMaxStored = 0f, totalInput = 0f;
    float totalMaxInput = 0f, totalOutput = 0f, totalMaxOutput = 0f;

    if (_monitoredBatteries.Count > 0)
    {
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
    }

    // --- DRAWING ---
    DrawUI(lcd, totalStored, totalMaxStored, totalInput, totalMaxInput, totalOutput, totalMaxOutput);

    Echo("Script ran successfully.");
    Echo($"Monitoring {_monitoredBatteries.Count} cached batteries.");
}

void DrawUI(IMyTextPanel lcd, float stored, float maxStored, float input, float maxInput, float output, float maxOutput)
{
    // Prepare the LCD for drawing
    lcd.ContentType = ContentType.SCRIPT;
    lcd.Script = ""; // Ensures the display is handled by the script

    // Get the drawing surface and viewport
    MySpriteDrawFrame frame = lcd.DrawFrame();
    RectangleF viewport = new RectangleF((lcd.TextureSize - lcd.SurfaceSize) / 2f, lcd.SurfaceSize);
    
    // Add a background to the entire panel
    MySprite panelBg = new MySprite(SpriteType.TEXTURE, "SquareSimple", color: BG_COLOR,
        position: viewport.Center, size: viewport.Size);
    frame.Add(panelBg);

    // --- Calculate Bar Positions ---
    float barWidth = viewport.Width - (PADDING * 2);
    Vector2 chargeBarPos = new Vector2(viewport.X + PADDING, viewport.Y + 120);
    Vector2 inputBarPos = new Vector2(viewport.X + PADDING, viewport.Y + 240);
    Vector2 outputBarPos = new Vector2(viewport.X + PADDING, viewport.Y + 360);

    // --- Draw Header ---
    DrawText(ref frame, "AGGREGATE BATTERY STATUS", new Vector2(viewport.Center.X, viewport.Y + PADDING), HEADER_FONT_SIZE, HEADER_COLOR, TextAlignment.CENTER);

    // --- Draw Charge Bar ---
    DrawText(ref frame, "CHARGE", chargeBarPos - new Vector2(0, 25), FONT_SIZE, TEXT_COLOR);
    DrawProgressBar(ref frame, chargeBarPos, barWidth, stored, maxStored, CHARGE_COLOR);
    string chargeText = $"{stored:F2} / {maxStored:F2} MWh ({(maxStored > 0 ? (stored / maxStored) * 100f : 0):F1}%)";
    DrawText(ref frame, chargeText, chargeBarPos + new Vector2(barWidth, 0), FONT_SIZE, TEXT_COLOR, TextAlignment.RIGHT);

    // --- Draw Input Bar ---
    DrawText(ref frame, "INPUT", inputBarPos - new Vector2(0, 25), FONT_SIZE, TEXT_COLOR);
    DrawProgressBar(ref frame, inputBarPos, barWidth, input, maxInput, INPUT_COLOR);
    DrawText(ref frame, $"{input:F2} MW", inputBarPos + new Vector2(barWidth, 0), FONT_SIZE, TEXT_COLOR, TextAlignment.RIGHT);

    // --- Draw Output Bar ---
    DrawText(ref frame, "OUTPUT", outputBarPos - new Vector2(0, 25), FONT_SIZE, TEXT_COLOR);
    DrawProgressBar(ref frame, outputBarPos, barWidth, output, maxOutput, OUTPUT_COLOR);
    DrawText(ref frame, $"{output:F2} MW", outputBarPos + new Vector2(barWidth, 0), FONT_SIZE, TEXT_COLOR, TextAlignment.RIGHT);

    // --- Handle No Batteries Found ---
    if (_monitoredBatteries.Count == 0)
    {
        DrawText(ref frame, "NO MONITORED BATTERIES FOUND", viewport.Center, 1.5f, Color.Red, TextAlignment.CENTER);
        DrawText(ref frame, $"Tag batteries with '{MONITOR_TAG}'", viewport.Center + new Vector2(0, 40), 1.2f, Color.Yellow, TextAlignment.CENTER);
    }

    // Finalize the frame
    frame.Dispose();
}

void DrawProgressBar(ref MySpriteDrawFrame frame, Vector2 position, float width, float current, float max, Color color)
{
    // Calculate the width of the filled portion
    float fillWidth = (max > 0) ? width * (current / max) : 0;

    // Background bar
    MySprite bg = new MySprite(SpriteType.TEXTURE, "SquareSimple", color: BAR_BG_COLOR,
        position: position + new Vector2(width / 2f, 0), size: new Vector2(width, BAR_HEIGHT));
    frame.Add(bg);

    // Foreground (filled) bar
    MySprite fill = new MySprite(SpriteType.TEXTURE, "SquareSimple", color: color,
        position: position + new Vector2(fillWidth / 2f, 0), size: new Vector2(fillWidth, BAR_HEIGHT));
    frame.Add(fill);
}

void DrawText(ref MySpriteDrawFrame frame, string text, Vector2 position, float size, Color color, TextAlignment align = TextAlignment.LEFT)
{
    MySprite textSprite = MySprite.CreateText(text, "White", color, size, align);
    textSprite.Position = position;
    frame.Add(textSprite);
}

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
