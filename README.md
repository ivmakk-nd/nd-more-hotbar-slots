# More Hotbar Slots

Carry up to 10 items in your hotbar.

The game gives you 2 hotbar slots, and a backpack adds only 1 or 2 more, so you can carry 4 items at most. This mod makes each backpack hold more, so the Large Backpack takes your hotbar to 10 slots.

## Features

- **Bigger backpacks**: the Small Backpack adds 4 hotbar slots instead of 1 (6 in total), and the Large Backpack adds 8 instead of 2 (10 in total). Without a backpack you keep the game's 2 slots.
- **Backpacks store more**: when you take a backpack off, it keeps the items of its slots, as in the game: 4 items in the Small Backpack, 8 in the Large Backpack. Wear it again and the items come back into your hotbar.
- **Keys for slots 9 and 10**: press 9 and 0. The mouse wheel and the controller bumpers cycle through all slots, as in the game. The keys do nothing while a menu is open, in photo mode, and wherever the game blocks the keys 1 to 8.
- **Settings**: change the base slots, the slots of each backpack, and the keys of slots 9 and 10. The hotbar never goes above 10 slots.

## Multiplayer

In co-op, the host and every player need the mod, in the same version and with the same settings. Not tested in co-op.

## Save safety

The mod stores nothing in your save: the game already saves your hotbar and your backpacks. Removing the mod needs care, see Uninstall.

## Requirements

- Nomad Drive (Early Access), the update of 28 October 2026 or later.
- [Nomad Drive BepInEx Pack](https://www.nexusmods.com/nomaddrive/mods/7).

## Installation

1. Install the [BepInEx pack](https://www.nexusmods.com/nomaddrive/mods/7).
2. Extract this mod's zip into the game folder (the folder with the game .exe). The DLL lands in BepInEx\plugins. Full path example:
   `C:\Program Files (x86)\Steam\steamapps\common\Nomad Drive\BepInEx\plugins\MoreHotbarSlots.dll`

## Uninstall

1. In the game, empty hotbar slots 9 and 10 and every backpack, then save. Without the mod, items in slots 9 and 10 leave your hotbar at the next load, and a backpack keeps only the game's number of items (1 or 2).
2. Quit the game.
3. Delete `MoreHotbarSlots.dll` from the `BepInEx\plugins` folder.

## Configuration

To change mod settings, edit `BepInEx\config\com.ivmakk.nomaddrive.morehotbarslots.cfg` while the game is closed (see [CONFIG.md](CONFIG.md) for every setting). Changes apply at the next game start.

Base slots and backpack slots together are at most 10, so a higher `BaseSlots` can make your backpacks smaller. Empty your backpacks before you change any slot setting. A backpack that gets smaller loses the items above its new size at the next load.

## Build

The build needs the game and the BepInEx pack installed, and the .NET SDK 10.

```
dotnet build src/MoreHotbarSlots.csproj -c Release
dotnet test tests/MoreHotbarSlots.Tests
```

The build looks for the game in `C:\Program Files (x86)\Steam\steamapps\common\Nomad Drive`. For another folder, pass `-p:GameDir=<game folder>` or set a `GameDir` environment variable. Add `-p:Package=true` to make the install zip in `dist/`.

## License

Licensed under the GNU General Public License v3.0. Copyright (C) 2026 ivmakk.

Do not reupload this mod or a changed copy without credit and a link to this repository.
