# Graphical Aggregate Battery Monitor (v2.0)

A script for the Programmable Block that displays the combined status of multiple batteries on a single LCD panel using a graphical, sprite-based UI.

---

## Features

-   **Graphical UI**: Displays status using clean, colored progress bars instead of text characters.
-   **Highly Customizable**: Easily change all colors, fonts, and layout elements at the top of the script.
-   **Aggregate Display**: Shows the combined total charge, current input, and current output for all selected batteries.
-   **Optimized Performance**: Caches the list of monitored batteries and only rescans periodically to reduce performance impact.
-   **Tag-Based System**: Easily add or remove batteries from the display by adding a simple tag to their `Custom Data`.
-   **Manual Rescan**: Ability to force a rescan of all batteries on the grid via a toolbar action.

---

## Setup Instructions

1.  **Place Blocks**: Place a **Programmable Block** and an **LCD Panel** on your grid.
2.  **Load the Script**:
    * Access the control panel of the Programmable Block and click "Edit".
    * Copy the entire C# script and paste it into the editor.
    * Click "Check Code" and then "Remember & Exit".
3.  **Configure the LCD Panel**:
    * Access the control panel for your LCD.
    * Click the "LCDs" dropdown at the top and select your panel if you have more than one.
    * In the list of content types on the right, select **"Script"**.
    * A new dropdown will appear below it. Select this script by its name from the list. The display should now activate.
4.  **Tag Your Batteries**:
    * For every battery you want to monitor, access its control panel.
    * Go to the **"Custom Data"** field.
    * Type `monitored=true` into the text box.
    * The script will automatically find and add this battery to the display on its next scan.

---

## Configuration

All customization is done by editing the variables in the "UI CONFIGURATION" section at the top of the script.

-   **Colors**: All colors are defined in `(R, G, B, Alpha)` format, where each value is from 0 to 255.
-   **Layout**: You can change font sizes, bar heights, and the padding from the edge of the screen.

## How to Manually Trigger a Rescan

If you've just tagged a new battery and want it to show up immediately, you can force a rescan.

1.  Set up a button or toolbar action for the Programmable Block.
2.  Select the **"Run"** action.
3.  In the argument box, type `rescan`.
4.  Activating this will force the script to immediately search for all tagged batteries.
