using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.SaveSystem
{
    /// <summary>
    /// ScriptableObject definition for an item in the demo.
    /// Create instances via Assets > Create > UMFOSS > SaveSystem Demo > Item Data.
    /// Place items in a Resources/Items/ folder so they can be loaded by name on restore.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "UMFOSS/SaveSystem Demo/Item Data")]
    public class ItemData_UMFOSS : ScriptableObject
    {
        [Header("Item Properties")]
        [Tooltip("Display name of the item.")]
        public string itemName = "Unnamed Item";

        [Tooltip("Short description of the item.")]
        [TextArea(2, 4)]
        public string description = "";

        [Tooltip("Item icon (optional — for UI display).")]
        public Sprite icon;

        [Tooltip("Maximum stack size for this item.")]
        public int maxStackSize = 99;
    }
}
