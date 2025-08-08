# Aggregate Battery Monitor

A script for the Programmable Block that displays the combined status of multiple batteries on a single LCD panel. It's optimized to be performance-friendly on large grids by only scanning for new batteries periodically.

## Features

* **Aggregate Display**: Shows the combined total charge, current input, and current output for all selected batteries.

* **Terminal-Style Progress Bars**: Clean, visual representation of charge, input, and output levels.

* **Optimized Performance**: Caches the list of monitored batteries and only rescans periodically to reduce performance impact.

* **Tag-Based System**: Easily add or remove batteries from the display by adding a simple tag to their `Custom Data`.

* **Manual Rescan**: Ability to force a rescan of all batteries on the grid via a toolbar action.

## Setup Instructions

1. **Place Blocks**: Place a **Programmable Block** and an **LCD Panel** on your grid.

2. **Load the Script**:

   * Access the control panel of the Programmable Block and click "Edit".

   * Copy the entire C# script and paste it into the editor.

   * Click "Check Code" and then "Remember & Exit". The script will now run automatically.

3. **Name the LCD**: Rename your LCD Panel to match the `LCD_NAME` variable in the script (default is `"LCD Panel Status"`).

4. **Tag Your Batteries**:

   * For every battery you want to monitor, access its control panel.

   * Go to the **"Custom Data"** field.

   * Type `monitored=true` into the text box.

   * The script will automatically find and add this battery to the display on its next scan.

## Configuration

You can change the script's behavior by editing the variables at the top of the code.

* `MONITOR_TAG`: The text the script looks for in a battery's `Custom Data`. You can change this if you want.

  * Default: `"monitored=true"`

* `LCD_NAME`: The exact name of the LCD panel where the status will be displayed.

  * Default: `"LCD Panel Status"`

* `RESCAN_INTERVAL_RUNS`: How often the script checks for new batteries. It runs about every 1.6 seconds, so a value of `6` means it rescans roughly every 10 seconds.

  * Default: `6`

* `PROGRESS_BAR_WIDTH`: The character width of the progress bars.

  * Default: `22`

* `FILLED_CHAR` / `EMPTY_CHAR`: The characters used to draw the progress bars.

  * Default: `'='` and `'-'`

## How to Manually Trigger a Rescan

The script automatically finds new batteries, but it only does so every few seconds. If you've just built or tagged a new battery and want it to show up immediately, you can force a rescan.

1. Sit in a control seat or go to a button panel.

2. Press `G` to open the toolbar config.

3. Find the Programmable Block running the script.

4. Drag the **"Run"** action to an empty slot on your toolbar.

5. In the text box that appears, type the argument `rescan`.

6. Pressing the hotkey for that slot will now force the script to immediately search for all tagged batteries.

