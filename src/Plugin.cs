using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using NomadDrive.Features.Backpacks;
using NomadDrive.Features.Hotbar;
using NomadDrive.Features.Inputs;
using UnityEngine;

namespace MoreHotbarSlots
{
    [BepInPlugin(PluginGuid, "More Hotbar Slots", "1.0.0")]
    [BepInProcess("Nomad Drive.exe")]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.ivmakk.nomaddrive.morehotbarslots";

        internal static ManualLogSource Log;
        internal static Harmony Harmony;
        internal static KeySetting Slot9Key;
        internal static KeySetting Slot10Key;
        internal static ConfigEntry<bool> Verbose;

        // Read once at the start, so the base slots and the bag slots limit of one session always agree.
        internal static int BaseSlots;
        internal static int SmallBackpackSlots;
        internal static int BigBackpackSlots;

        private static readonly HashSet<string> WarnedParts = new HashSet<string>();

        private void Awake()
        {
            Log = Logger;
            var baseSlots = Config.Bind("Slots", "BaseSlots", 2, new ConfigDescription(
                "Hotbar slots you have without a backpack. The game has 2. " +
                "The total of base slots and backpack slots is at most 10, so a higher value can make your backpacks smaller. " +
                "Empty your backpacks before you change this value. Applies at the next game start.",
                new AcceptableValueRange<int>(1, 9)));
            var smallSlots = Config.Bind("Slots", "SmallBackpackSlots", 4, new ConfigDescription(
                "Hotbar slots the Small Backpack adds, and the number of items it stores. The game has 1. " +
                "The total of base slots and backpack slots is at most 10. Empty your backpacks before you change this value. " +
                "Applies at the next game start.",
                new AcceptableValueRange<int>(1, 9)));
            var bigSlots = Config.Bind("Slots", "BigBackpackSlots", 8, new ConfigDescription(
                "Hotbar slots the Large Backpack adds, and the number of items it stores. The game has 2. " +
                "The total of base slots and backpack slots is at most 10. Empty your backpacks before you change this value. " +
                "Applies at the next game start.",
                new AcceptableValueRange<int>(1, 9)));
            Slot9Key = new KeySetting(Config.Bind("Keys", "Slot9Key", nameof(KeyCode.Alpha9),
                "Key that selects hotbar slot 9, a Unity key name (for example Alpha9, F9, Keypad9). None turns it off. " +
                "All key names: https://docs.unity3d.com/ScriptReference/KeyCode.html"), KeyCode.Alpha9);
            Slot10Key = new KeySetting(Config.Bind("Keys", "Slot10Key", nameof(KeyCode.Alpha0),
                "Key that selects hotbar slot 10, a Unity key name (for example Alpha0, F10, Keypad0). None turns it off. " +
                "All key names: https://docs.unity3d.com/ScriptReference/KeyCode.html"), KeyCode.Alpha0);
            Verbose = Config.Bind("General", "Verbose", false, "Log extra detail at Debug level. Keep off in normal play.");

            BaseSlots = SlotLogic.ClampBase(baseSlots.Value);
            SmallBackpackSlots = smallSlots.Value;
            BigBackpackSlots = bigSlots.Value;

            Harmony = new Harmony(PluginGuid);
            // The slot patches go on or off together: one without the other gives a hotbar that does not match the backpacks.
            try
            {
                GameMembers.CheckSlots();
                Harmony.CreateClassProcessor(typeof(SlotsOnHotbarManagerAwake)).Patch();
                Harmony.CreateClassProcessor(typeof(SlotsOnBackpackAwake)).Patch();
            }
            catch (Exception e)
            {
                Harmony.UnpatchSelf();
                Log.LogWarning($"More Hotbar Slots is off, the game works as without it. A game member it needs is missing: {e.Message}");
                return;
            }
            // The keys are separate, so a game update that breaks them keeps the slots, and the items in them, as they are.
            try
            {
                GameMembers.CheckKeys();
                Harmony.CreateClassProcessor(typeof(KeysOnIsHotbarSlotButtonDown)).Patch();
            }
            catch (Exception e)
            {
                Log.LogWarning($"The keys for hotbar slots 9 and 10 are off, use the mouse wheel. A game member they need is missing: {e.Message}");
            }
            Log.LogInfo("More Hotbar Slots loaded.");
        }

        // Logs the first error of each part once, so a fault in a per-frame part does not flood the log.
        internal static void WarnOnce(string part, Exception e)
        {
            if (WarnedParts.Add(part))
            {
                Log.LogWarning($"{part} failed, later errors of it are not logged: {e}");
            }
        }
    }

    // A key stored as its name, not as a KeyCode entry, so BepInEx does not write the list of every
    // KeyCode into the config file. The name is parsed again only when it changes; a wrong name uses the default key.
    internal sealed class KeySetting
    {
        private readonly ConfigEntry<string> _entry;
        private readonly KeyCode _default;
        private string _parsed;
        private KeyCode _key;

        internal KeySetting(ConfigEntry<string> entry, KeyCode defaultKey)
        {
            _entry = entry;
            _default = defaultKey;
        }

        internal KeyCode Key
        {
            get
            {
                string value = _entry.Value;
                if (!ReferenceEquals(value, _parsed))
                {
                    _parsed = value;
                    if (!Enum.TryParse(value?.Trim(), true, out _key) || !Enum.IsDefined(typeof(KeyCode), _key))
                    {
                        _key = _default;
                        Plugin.Log.LogWarning($"{_entry.Definition.Key} '{value}' is not a Unity key name, using {_default}.");
                    }
                }
                return _key;
            }
        }
    }

    // Base slots and the cap: written before Mirror's OnStartServer and any save restore read them.
    [HarmonyPatch]
    internal static class SlotsOnHotbarManagerAwake
    {
        private static readonly AccessTools.FieldRef<HotbarManager, int> DefaultSlotCount =
            AccessTools.FieldRefAccess<HotbarManager, int>("defaultSlotCount");
        private static readonly AccessTools.FieldRef<HotbarManager, int> MaxSlotCount =
            AccessTools.FieldRefAccess<HotbarManager, int>("maxSlotCount");

        private static MethodBase TargetMethod() => GameMembers.HotbarManagerAwake;

        private static void Postfix(HotbarManager __instance)
        {
            try
            {
                if (Plugin.Verbose.Value)
                {
                    Plugin.Log.LogDebug($"MoreHotbarSlots: hotbar defaultSlotCount {DefaultSlotCount(__instance)} -> {Plugin.BaseSlots}, maxSlotCount {MaxSlotCount(__instance)} -> {SlotLogic.Cap}");
                }
                DefaultSlotCount(__instance) = Plugin.BaseSlots;
                MaxSlotCount(__instance) = SlotLogic.Cap;
            }
            catch (Exception e)
            {
                Plugin.WarnOnce(nameof(SlotsOnHotbarManagerAwake), e);
            }
        }
    }

    // Bag slots: each BackpackDefinition asset is written once, at the first backpack that uses it.
    [HarmonyPatch]
    internal static class SlotsOnBackpackAwake
    {
        private static readonly AccessTools.FieldRef<BackpackDefinition, int> ExtraSlots =
            AccessTools.FieldRefAccess<BackpackDefinition, int>("extraSlots");

        // Each definition the mod wrote, so no value is mapped twice.
        private static readonly HashSet<BackpackDefinition> Written = new HashSet<BackpackDefinition>();

        private static MethodBase TargetMethod() => GameMembers.BackpackAwake;

        private static void Postfix(Backpack __instance)
        {
            try
            {
                BackpackDefinition definition = __instance.Definition;
                if (definition == null || !Written.Add(definition))
                {
                    return;
                }
                int vanilla = ExtraSlots(definition);
                int slots = SlotLogic.BagSlots(vanilla, Plugin.BaseSlots, Plugin.SmallBackpackSlots, Plugin.BigBackpackSlots);
                ExtraSlots(definition) = slots;
                if (Plugin.Verbose.Value)
                {
                    Plugin.Log.LogDebug($"MoreHotbarSlots: backpack {definition.name} tier {definition.Tier} bag slots {vanilla} -> {slots}");
                }
            }
            catch (Exception e)
            {
                Plugin.WarnOnce(nameof(SlotsOnBackpackAwake), e);
            }
        }
    }

    // Keys for hotbar slots 9 and 10. The game asks for each slot up to the slot count, after its own hotbar checks.
    [HarmonyPatch]
    internal static class KeysOnIsHotbarSlotButtonDown
    {
        private static MethodBase TargetMethod() => GameMembers.IsHotbarSlotButtonDown;

        private static void Postfix(int slotNumber, ref bool __result)
        {
            int keySlot = SlotLogic.KeySlot(slotNumber);
            if (__result || keySlot < 0)
            {
                return;
            }
            try
            {
                // Keys 1 to 8 go through Rewired, which a menu, photo mode, and the console turn off. These checks do the same.
                if (!Application.isFocused || Cursor.lockState != CursorLockMode.Locked
                    || ActionMapsManager.IsPhotoModeActive || ActionMapsManager.IsGameplayMapsSuspended)
                {
                    return;
                }
                KeyCode key = keySlot == 0 ? Plugin.Slot9Key.Key : Plugin.Slot10Key.Key;
                if (key == KeyCode.None || !UnityInput.Current.GetKeyDown(key))
                {
                    return;
                }
                __result = true;
                if (Plugin.Verbose.Value)
                {
                    Plugin.Log.LogDebug($"MoreHotbarSlots: key {key} -> slot {slotNumber}");
                }
            }
            catch (Exception e)
            {
                Plugin.WarnOnce(nameof(KeysOnIsHotbarSlotButtonDown), e);
            }
        }
    }

    // The game members the mod changes. A game update can rename one, so Awake checks them before it patches.
    internal static class GameMembers
    {
        internal static MethodInfo HotbarManagerAwake => AccessTools.DeclaredMethod(typeof(HotbarManager), "Awake");
        internal static MethodInfo BackpackAwake => AccessTools.DeclaredMethod(typeof(Backpack), "Awake");
        internal static MethodInfo IsHotbarSlotButtonDown =>
            AccessTools.DeclaredMethod(typeof(OnFootInputs), nameof(OnFootInputs.IsHotbarSlotButtonDown), new[] { typeof(int) });

        internal static void CheckSlots()
        {
            Require(AccessTools.Field(typeof(HotbarManager), "defaultSlotCount"), "HotbarManager.defaultSlotCount");
            Require(AccessTools.Field(typeof(HotbarManager), "maxSlotCount"), "HotbarManager.maxSlotCount");
            Require(AccessTools.Field(typeof(BackpackDefinition), "extraSlots"), "BackpackDefinition.extraSlots");
            Require(HotbarManagerAwake, "HotbarManager.Awake");
            Require(BackpackAwake, "Backpack.Awake");
        }

        internal static void CheckKeys()
        {
            Require(IsHotbarSlotButtonDown, "OnFootInputs.IsHotbarSlotButtonDown");
        }

        private static void Require(MemberInfo member, string name)
        {
            if (member == null)
            {
                throw new MissingMemberException(name);
            }
        }
    }
}
