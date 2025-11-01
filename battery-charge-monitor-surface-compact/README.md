# 🪫 Battery Charge Monitor (Text-Only LCD Version)

A lightweight, text-based battery monitor script for **Space Engineers**.  
Designed to run on small LCDs, cockpit screens, and console panels — perfect for compact ship control seats.

---

## 📖 Overview

This script displays the **aggregate power status** of all batteries on your grid that are tagged for monitoring.  
It shows three key stats:

```
=== Battery Status ===
In: 34.5%
Out: 5.2%
Charge: 82.3%
```


It automatically rescans periodically to detect newly tagged batteries.

---

## ⚙️ Setup Instructions

1. **Add a Programmable Block** to your grid.  
   - Open its terminal → Edit → Paste the script into the editor.  
   - Click **“Check Code”**, then **“OK”**.

2. **Add the display block** (e.g., cockpit, control seat, console, or LCD).  
   - Rename it to match the `DISPLAY_BLOCK_NAME` in the script (default:  
     `"Control Seat (Nomad)"`).

3. **Find the correct screen index** (if your block has multiple screens):  
   - Change the `SURFACE_INDEX` constant in the script.  
   - Common examples:
     | Block Type | Main Screen Index |
     |-------------|-------------------|
     | Cockpit / Control Seat | 0–4 |
     | Industrial Console | 0 |
     | LCD Panel | 0 |

4. **Tag the batteries** you want monitored:  
   - Open a battery’s **Custom Data**.  
   - Add this line:
     ```
     monitored=true
     ```
   - Only tagged batteries will be included in the display.

5. **Run the script**:  
   - Set the Programmable Block to **Run** (no arguments).  
   - To force a rescan of batteries, run it with the argument:
     ```
     rescan
     ```

---

## ⚙️ Configuration

| Variable | Description | Default |
|-----------|-------------|----------|
| `MONITOR_TAG` | Text tag searched in battery Custom Data | `"monitored=true"` |
| `DISPLAY_BLOCK_NAME` | Name of the LCD/cockpit/console to display on | `"Control Seat (Nomad)"` |
| `SURFACE_INDEX` | Which built-in LCD screen to use | `4` |
| `RESCAN_INTERVAL_RUNS` | How often to rescan batteries (every ~10 seconds per cycle) | `6` |

---

## 🧮 How It Works

- On startup, the script finds all **batteries** containing the tag `MONITOR_TAG` in their Custom Data.  
- It aggregates:
  - **Total stored power**
  - **Current input/output rates**
  - **Maximum capacity**
- Every few seconds, it updates a text-only LCD display showing:
  - Current **input %**
  - Current **output %**
  - Overall **charge %**

---

## 🪛 Tips & Notes

- Works on **any block with LCD surfaces**: cockpits, control seats, consoles, programmable blocks, and standard LCD panels.
- Use **monospace font** for best alignment.
- Automatically adjusts text size for small displays.
- You can increase or decrease `Runtime.UpdateFrequency` in `Program()` for faster or slower updates.

---

## 📜 License & Attribution

This script is free to use and modify in any way shape or form whatsoever. Use at your own risk.
