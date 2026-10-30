# Copilot code-review instructions

This repo is a BepInEx 5 (Mono) Harmony mod for *Nomad Drive*, a co-op Unity game on Mirror. The plugin derives from `BaseUnityPlugin` and patches the game's `Assembly-CSharp.dll` at runtime. Review with these traps in mind; a general C# review misses most of them.

## Unity and Harmony traps

- **Unity null.** `UnityEngine.Object` overloads `==`, so a destroyed object compares equal to `null` only through `==` or its bool conversion. Flag `?.`, `??`, `is null`, and `is not null` on a Unity object (a `Component`, a `GameObject`, a `ScriptableObject`).
- **Guard game lookups.** Services, components, and singletons are often null during a scene load or in the main menu. Flag an unchecked dereference inside a patch.
- **A patch must not break the game.** Each patch body sits in a `try`/`catch` that logs the error once. Flag a patch body without one, and a log call inside a per-frame path with no once-only guard.
- **Reflection lookups can fail after a game update.** `AccessTools.Method`, `AccessTools.Field`, and `FieldRefAccess` by name return null or throw when the game renames a member. Flag a lookup whose result is used with no null check, and a lookup repeated on each call instead of cached in a static field.
- **Prefer a Postfix.** Flag a Prefix that returns `false` (skips the original) with no comment that says why.
- **State machines.** A patch of an `async` method or a coroutine runs only at its start; the work is in the `MoveNext` of the state machine. Flag a patch that expects to see the result of such a method.
- **Burst.** Flag a patch of an `Execute` method of a job with `[BurstCompile]`: it has no effect.

## Multiplayer (Mirror)

- **Woven methods.** The Mirror weaver moves the body of each `[Command]`, `[ClientRpc]`, and `[TargetRpc]` method into `UserCode_<Method>__<ParamTypes>`. Flag a patch on the original `Cmd`/`Rpc` method that expects to change what the receiver does.
- **SyncVars.** A `[SyncVar]` field syncs only when written through its `Network<field>` property, and only on the host. Flag a direct write of a SyncVar field, and a write on a client.
- **Side.** Flag server logic that runs with no `NetworkServer.active` or `isServer` check, and a change of shared state from a client.

## Performance

- Flag `FindObjectsByType`, `FindFirstObjectByType`, `GetComponentsInChildren`, `Camera.main`, LINQ, string building, or `Traverse` in an `Update`, `FixedUpdate`, or a patch of a per-frame or per-physics-step method.

## Structure and tests

- **Pure logic is separated and tested.** Logic that does not need the running game lives in its own file with no BepInEx, Unity, or game reference, unit-tested under `tests/`. Flag new pure logic in a patch class, and new pure logic with no test.
- **One patch class for each target method,** named `<Feature>On<Target>`.

## Release and config hygiene

- **Verbose ships off.** The `Verbose` config binds with default `false`. Diagnostic tracing goes on `LogDebug` behind it; `LogInfo` stays quiet apart from the load line.
- **The plugin GUID never changes.** It is the BepInEx identity and the config file name. Flag any edit to it.
- **The version is in two places that must agree:** `<Version>` in the csproj and the `BepInPlugin` attribute.
- **No committed build output.** Flag `bin/`, `obj/`, `dist/`, or a game DLL in the diff. Game `<Reference>` entries keep `<Private>false</Private>`.
- **No game source.** Flag a method body copied from the decompiled game code.
- **Changelog matches the change.** A player-visible change adds an `[Unreleased]` entry to `CHANGELOG.md` in player-facing wording. An internal-only refactor gets none.

## This mod

More Hotbar Slots raises the hotbar slots of the player and the bag slots of each backpack, raises the hotbar cap from 8 to 10, and adds keys for hotbar slots 9 and 10.

- **Patch targets.** A postfix on `HotbarManager.Awake` writes `defaultSlotCount` (the base slots) and `maxSlotCount` (10). A postfix on `Backpack.Awake` writes `BackpackDefinition.extraSlots` once for each definition (a `HashSet` of written definitions). A postfix on `OnFootInputs.IsHotbarSlotButtonDown` answers for slots 9 and 10 from the configured keys. Each target is found with `AccessTools.DeclaredMethod`, because `AccessTools.Method` would fall back to a base-class `Awake` after a game update. Flag a switch to `AccessTools.Method` for these targets.
- **Write before use.** Both `Awake` postfixes run before Mirror's `OnStartServer`, the save restore, and the first wear, so each later read sees the new values. Flag a move of the writes to a later hook.
- **Two patch groups.** The two slot patches go on or off together (a failure calls `UnpatchSelf`). The keys patch is separate, so a broken keys target keeps the slots. Flag a change that puts them back in one group.
- **Bag slots are also the backpack's capacity.** A lower value makes the next load drop stored items above it. The backpack kind comes from its vanilla value (1 is the Small Backpack, 2 is the Large Backpack); `SlotLogic.BagSlots` maps it and caps base slots plus bag slots at 10. Flag any change that can lower a backpack's capacity without a note in the config descriptions and the README.
- **Slot values are read once.** `BaseSlots`, `SmallBackpackSlots`, and `BigBackpackSlots` are read at `Awake`, so both postfixes use the same values for a session. Flag a read of the config entries inside a postfix.
- **Keys.** The keys are string entries with a Unity `KeyCode` name, parsed by `KeySetting` and cached until the string changes. Flag a `ConfigEntry<KeyCode>`: BepInEx would write every `KeyCode` name into the config file. The key read is gated by window focus, a locked cursor, photo mode, and suspended gameplay input, like the game's own Rewired keys.
- **Every player.** The host decides the slot count, and each client reads the base slots and the bag slots too. The mod sends no network message and writes no SyncVar itself. Flag any `Cmd`/`Rpc` call or SyncVar write.
