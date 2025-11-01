# 🌬️ Air Vent Monitor (Text-Only LCD Version)

A simple text-based script for **Space Engineers** that monitors the oxygen pressure of tagged air vents and displays a list of vents below a specified pressure threshold.

Designed for use on **cockpits**, **control seats**, **consoles**, and **LCD panels** — especially useful on small screens.

---

## 📖 Overview

This script continuously checks all air vents tagged with `monitored=true` in their Custom Data and lists any that have a room pressure below the configured threshold (default: **90%**).

If all vents are above the threshold, it displays a simple "All monitored vents above threshold." message.

Example output:

```
=== Air Vent Monitor ===
Threshold: 90%

Cargo Bay Vent: 78.2%
Hangar Vent 1: 64.9%

2 vent(s) below 90%!
```

---

## ⚙️ Setup Instructions

1. **Add a Programmable Block** to your grid.  
   - Open its terminal → Edit → Paste the script into the editor.  
   - Click **“Check Code”**, then **“OK”**.

2. **Select a display surface** (LCD, console, or cockpit).  
   - Rename it to match the `DISPLAY_BLOCK_NAME` in the script (default: `"Control Seat (Nomad)"`).  
   - Adjust `SURFACE_INDEX` if the cockpit has multiple displays.

3. **Tag vents for monitoring.**  
   - Open the air vent’s **Custom Data**.  
   - Add the following line:
     ```
     monitored=true
     ```

4. **Adjust the threshold (optional).**  
   - Change the constant `PRESSURE_THRESHOLD` at the top of the script (default: `90f`).

5. **Run the script.**  
   - Set the Programmable Block to **Run**.  
   - To force a rescan of vents, run it with the argument:
     ```
     rescan
     ```

---

## ⚙️ Configuration

| Variable | Description | Default |
|-----------|-------------|----------|
| `MONITOR_TAG` | Text tag searched in air vent Custom Data | `"monitored=true"` |
| `DISPLAY_BLOCK_NAME` | Name of the block that contains the LCD | `"Control Seat (Nomad)"` |
| `SURFACE_INDEX` | Which built-in LCD screen to use | `4` |
| `PRESSURE_THRESHOLD` | Minimum acceptable pressure (percent) | `90f` |
| `RESCAN_INTERVAL_RUNS` | How often to rescan vents (~10s intervals) | `6` |

---

## 🧮 How It Works

- Finds all **air vents** on the same grid.  
- Filters to those tagged with `monitored=true`.  
- Checks their **room oxygen level** via `GetOxygenLevel()` (0–1 → 0–100%).  
- Displays only those below the defined threshold.  
- Updates automatically every few seconds.

---

## 🪛 Tips & Notes

- Works on any **block with LCD surfaces**, including cockpits, control seats, programmable blocks, and standard LCD panels.  
- Uses **monospace font** for clean alignment.  
- Auto-adjusts font size based on display dimensions.  
- To display on multiple screens, copy the script and adjust `DISPLAY_BLOCK_NAME` / `SURFACE_INDEX` for each.

---

## 📜 License & Attribution

This script is free to use and modify in any way, shape or form. Use at your own risk.
