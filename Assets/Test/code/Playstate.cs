using UnityEngine;

public static class Playstate
{
    public static GameObject gunslot1 { get; private set; }
    public static GameObject gunslot2 { get; private set; }
    public static GameObject gunslot3 { get; private set; }
    public static GameObject gunslot4 { get; private set; }
    public static GameObject robotType { get; set; }

    public static void SaveToPlaystate(GameObject weaponPrefab, int gunSlot)
    {
        switch (gunSlot)
        {
            case 1: gunslot1 = weaponPrefab; break;
            case 2: gunslot2 = weaponPrefab; break;
            case 3: gunslot3 = weaponPrefab; break;
            case 4: gunslot4 = weaponPrefab; break;
            default: Debug.LogError("Slot number must be 1-4"); break;
        }
    }
}
