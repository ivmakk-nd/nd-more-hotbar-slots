namespace MoreHotbarSlots
{
    // Game-free slot rules: no game, Unity, or BepInEx type, so the tests link this file alone.
    public static class SlotLogic
    {
        // The most hotbar slots a player can have: base slots plus bag slots never go above it.
        public const int Cap = 10;

        public static int ClampBase(int baseSlots)
        {
            if (baseSlots < 1)
            {
                return 1;
            }
            return baseSlots > Cap - 1 ? Cap - 1 : baseSlots;
        }

        // The game gives the small backpack 1 bag slot and the big backpack 2, so the vanilla value tells
        // the two apart. Any other backpack keeps its value. Each result is at least 1 and fits in the cap with the base slots.
        public static int BagSlots(int vanilla, int baseSlots, int small, int big)
        {
            int slots = vanilla == 1 ? small : vanilla == 2 ? big : vanilla;
            int most = Cap - ClampBase(baseSlots);
            if (slots > most)
            {
                return most;
            }
            return slots < 1 ? 1 : slots;
        }

        // The game has keys for hotbar slots 1 to 8. Returns the index of the mod's key entry for slot 9 or 10, else -1.
        public static int KeySlot(int slotNumber)
        {
            return slotNumber == 9 || slotNumber == 10 ? slotNumber - 9 : -1;
        }
    }
}
