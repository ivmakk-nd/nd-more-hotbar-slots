# Configuration

Every setting lives in `BepInEx\config\com.ivmakk.nomaddrive.morehotbarslots.cfg`, written the first time you run the game with the mod installed. Edit it with any text editor while the game is closed. Changes apply at the next game start. A value out of its range applies as the nearest limit.

Base slots and backpack slots together are at most 10. A backpack adds fewer slots when the total would be higher, so a higher `BaseSlots` can make your backpacks smaller. Empty your backpacks before you change any slot setting: a backpack that gets smaller loses the items above its new size at the next load.

## Slots

| Setting | Default | Values | What it does |
|---|---|---|---|
| `BaseSlots` | `2` | `1` to `9` | Hotbar slots you have without a backpack. The game has 2. |
| `SmallBackpackSlots` | `4` | `1` to `9` | Hotbar slots the Small Backpack adds, and the number of items it stores. The game has 1. |
| `BigBackpackSlots` | `8` | `1` to `9` | Hotbar slots the Large Backpack adds, and the number of items it stores. The game has 2. |

## Keys

| Setting | Default | Values | What it does |
|---|---|---|---|
| `Slot9Key` | `Alpha9` | A [Unity key name](https://docs.unity3d.com/ScriptReference/KeyCode.html), such as `F9` or `Keypad9`, or `None` | The key that selects hotbar slot 9. `None` turns it off. A wrong name uses the default key. |
| `Slot10Key` | `Alpha0` | A [Unity key name](https://docs.unity3d.com/ScriptReference/KeyCode.html), such as `F10` or `Keypad0`, or `None` | The key that selects hotbar slot 10. `None` turns it off. A wrong name uses the default key. |

## General

| Setting | Default | Values | What it does |
|---|---|---|---|
| `Verbose` | `false` | `true`, `false` | Writes extra lines to `BepInEx\LogOutput.log` for troubleshooting. Keep it off in normal play. |
