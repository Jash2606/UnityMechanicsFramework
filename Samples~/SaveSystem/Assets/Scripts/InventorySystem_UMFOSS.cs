using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameplayMechanicsUMFOSS.Systems;

namespace GameplayMechanicsUMFOSS.Samples.SaveSystem
{
    /// <summary>
    /// Demo inventory system implementing ISaveable_UMFOSS.
    /// Demonstrates saving and restoring complex structured data
    /// (a list of items with quantities) using the ISaveable pattern.
    /// </summary>
    public class InventorySystem_UMFOSS : MonoBehaviour, ISaveable_UMFOSS
    {
        // ─────────────────────────────────────────────
        // Serialized Fields
        // ─────────────────────────────────────────────

        [Header("Inventory Settings")]
        [Tooltip("Maximum number of unique item slots.")]
        [SerializeField] private int maxSlots = 10;

        [Header("Save Settings")]
        [Tooltip("Unique identifier for this saveable. Must be unique across all saveables.")]
        [SerializeField] private string uniqueID = "Inventory_Player";

        // ─────────────────────────────────────────────
        // Private Fields
        // ─────────────────────────────────────────────

        /// <summary>
        /// Runtime inventory data. Each entry is an item + quantity.
        /// </summary>
        private List<InventorySlot> slots = new List<InventorySlot>();

        // ─────────────────────────────────────────────
        // Public Properties
        // ─────────────────────────────────────────────

        /// <summary>Read-only access to inventory slots.</summary>
        public IReadOnlyList<InventorySlot> Slots { get { return slots.AsReadOnly(); } }

        /// <summary>Number of occupied slots.</summary>
        public int OccupiedSlots { get { return slots.Count; } }

        /// <summary>Whether the inventory is full.</summary>
        public bool IsFull { get { return slots.Count >= maxSlots; } }

        // ─────────────────────────────────────────────
        // Inner Types
        // ─────────────────────────────────────────────

        /// <summary>
        /// A single inventory slot: an item reference + quantity.
        /// </summary>
        public class InventorySlot
        {
            public ItemData_UMFOSS itemData;
            public int quantity;

            public InventorySlot(ItemData_UMFOSS data, int qty)
            {
                itemData = data;
                quantity = qty;
            }

            public bool IsEmpty { get { return itemData == null || quantity <= 0; } }
        }

        // ─────────────────────────────────────────────
        // Unity Lifecycle
        // ─────────────────────────────────────────────

        private void OnEnable()
        {
            SaveSystem_UMFOSS sys = SaveSystem_UMFOSS.Instance;
            if (sys == null) sys = FindObjectOfType<SaveSystem_UMFOSS>();

            if (sys != null)
            {
                sys.Register(this);
            }
            else
            {
                Debug.LogWarning($"[InventorySystem] SaveSystem not found. Save/load will not work for '{uniqueID}'.");
            }
        }

        private void OnDisable()
        {
            SaveSystem_UMFOSS sys = SaveSystem_UMFOSS.Instance;
            if (sys == null) sys = FindObjectOfType<SaveSystem_UMFOSS>();

            if (sys != null)
            {
                sys.Deregister(this);
            }
        }

        // ─────────────────────────────────────────────
        // Public Methods
        // ─────────────────────────────────────────────

        /// <summary>
        /// Adds an item to the inventory. Stacks if already present.
        /// </summary>
        /// <param name="itemData">The item to add.</param>
        /// <param name="quantity">How many to add.</param>
        /// <returns>True if successfully added.</returns>
        public bool AddItem(ItemData_UMFOSS itemData, int quantity = 1)
        {
            if (itemData == null)
            {
                Debug.LogWarning("[InventorySystem] Cannot add null item.");
                return false;
            }

            // Check if item already exists — stack it
            InventorySlot existingSlot = slots.FirstOrDefault(s => s.itemData == itemData);
            if (existingSlot != null)
            {
                existingSlot.quantity += quantity;
                Debug.Log($"[InventorySystem] Stacked {quantity}x {itemData.itemName}. Total: {existingSlot.quantity}");
                return true;
            }

            // New slot
            if (IsFull)
            {
                Debug.LogWarning($"[InventorySystem] Inventory full! Cannot add {itemData.itemName}.");
                return false;
            }

            slots.Add(new InventorySlot(itemData, quantity));
            Debug.Log($"[InventorySystem] Added {quantity}x {itemData.itemName}. Slots used: {slots.Count}/{maxSlots}");
            return true;
        }

        /// <summary>
        /// Removes an item from the inventory.
        /// </summary>
        /// <param name="itemData">The item to remove.</param>
        /// <param name="quantity">How many to remove.</param>
        /// <returns>True if successfully removed.</returns>
        public bool RemoveItem(ItemData_UMFOSS itemData, int quantity = 1)
        {
            if (itemData == null) return false;

            InventorySlot slot = slots.FirstOrDefault(s => s.itemData == itemData);
            if (slot == null)
            {
                Debug.LogWarning($"[InventorySystem] Item {itemData.itemName} not found in inventory.");
                return false;
            }

            slot.quantity -= quantity;
            if (slot.quantity <= 0)
            {
                slots.Remove(slot);
                Debug.Log($"[InventorySystem] Removed all {itemData.itemName} from inventory.");
            }
            else
            {
                Debug.Log($"[InventorySystem] Removed {quantity}x {itemData.itemName}. Remaining: {slot.quantity}");
            }

            return true;
        }

        /// <summary>
        /// Clears all items from the inventory.
        /// </summary>
        public void ClearInventory()
        {
            slots.Clear();
            Debug.Log("[InventorySystem] Inventory cleared.");
        }

        /// <summary>
        /// Gets a display string of all inventory contents.
        /// </summary>
        public string GetInventoryDisplay()
        {
            if (slots.Count == 0) return "Empty";

            return string.Join(", ", slots.Select(s =>
                s.itemData != null ? $"{s.itemData.itemName} x{s.quantity}" : "? x" + s.quantity
            ));
        }

        // ─────────────────────────────────────────────
        // ISaveable_UMFOSS Implementation
        // ─────────────────────────────────────────────

        public string GetSaveID()
        {
            return uniqueID;
        }

        public object CaptureState()
        {
            var slotData = slots
                .Where(s => !s.IsEmpty)
                .Select(s => new SlotSaveData
                {
                    itemDataName = s.itemData.name, // SO asset name
                    quantity = s.quantity
                }).ToList();

            return new InventorySaveData { slots = slotData };
        }

        public void RestoreState(object state)
        {
            string json = state as string;
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning($"[InventorySystem] RestoreState received null/empty data for '{uniqueID}'.");
                return;
            }

            InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(json);
            ClearInventory();

            if (data.slots == null) return;

            foreach (var slotData in data.slots)
            {
                ItemData_UMFOSS itemData = Resources.Load<ItemData_UMFOSS>("Items/" + slotData.itemDataName);
                if (itemData != null)
                {
                    // Apply to ALL duplicated inventory systems in the scene to cure inspector desyncs
                    foreach (var duplicate in FindObjectsOfType<InventorySystem_UMFOSS>())
                    {
                        duplicate.AddItem(itemData, slotData.quantity);
                    }
                }
                else
                {
                    Debug.LogWarning($"[InventorySystem] Could not load item '{slotData.itemDataName}' from Resources/Items/. Item skipped.");
                }
            }

            Debug.Log($"[InventorySystem] State restored: {slots.Count} item types loaded.");
        }
    }

    /// <summary>
    /// Serializable data for the entire inventory. Un-nested.
    /// </summary>
    [System.Serializable]
    public class InventorySaveData
    {
        public List<SlotSaveData> slots;
    }

    [System.Serializable]
    public class SlotSaveData
    {
        public string itemDataName;
        public int quantity;
    }
}
