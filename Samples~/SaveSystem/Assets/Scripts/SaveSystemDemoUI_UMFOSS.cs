using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Systems;

namespace GameplayMechanicsUMFOSS.Samples.SaveSystem
{
    /// <summary>
    /// UI Controller for the Save System Demo.
    /// Programmatically generates a simple interface to demonstrate:
    /// - Registering / Deregistering ISaveable
    /// - Triggering Save / Load / New Game
    /// - Observing state changes (Health / Inventory) across loads
    /// </summary>
    public class SaveSystemDemoUI_UMFOSS : MonoBehaviour
    {
        [Header("Target Systems")]
        [Tooltip("If left empty, will auto-find in scene")]
        [SerializeField] private HealthSystem_UMFOSS healthSystem;

        [Tooltip("If left empty, will auto-find in scene")]
        [SerializeField] private InventorySystem_UMFOSS inventorySystem;

        [Header("Mock Data")]
        [SerializeField] private ItemData_UMFOSS swordItem;
        [SerializeField] private ItemData_UMFOSS shieldItem;

        // UI references (created at runtime)
        private Text healthText;
        private Text inventoryText;
        private Text timestampText;
        private Text savePathText;
        private Text statusText;

        private Canvas canvas;

        // ─────────────────────────────────────────────
        // Unity Lifecycle
        // ─────────────────────────────────────────────

        private void Start()
        {
            // Auto-find references if not assigned
            if (healthSystem == null) healthSystem = FindObjectOfType<HealthSystem_UMFOSS>();
            if (inventorySystem == null) inventorySystem = FindObjectOfType<InventorySystem_UMFOSS>();

            // Load demo items from Resources
            if (swordItem == null) swordItem = Resources.Load<ItemData_UMFOSS>("Items/Sword");
            if (shieldItem == null) shieldItem = Resources.Load<ItemData_UMFOSS>("Items/Shield");

            // Build the UI
            CreateUI();

            // Subscribe to events
            if (SaveSystem_UMFOSS.Instance != null)
            {
                SaveSystem_UMFOSS.Instance.OnGameSaved += OnSaved;
                SaveSystem_UMFOSS.Instance.OnGameLoaded += OnLoaded;
                SaveSystem_UMFOSS.Instance.OnLoadFailed += OnLoadFailed;

                // Load initial metadata if save exists
                var meta = SaveSystem_UMFOSS.Instance.GetSaveMetadata("Slot1");
                if (meta != null)
                {
                    timestampText.text = $"Last Saved: {System.DateTime.Parse(meta.lastSavedTimestamp).ToString("g")}";
                }
            }

            UpdateDisplay();
        }

        private void OnDestroy()
        {
            if (SaveSystem_UMFOSS.Instance != null)
            {
                SaveSystem_UMFOSS.Instance.OnGameSaved -= OnSaved;
                SaveSystem_UMFOSS.Instance.OnGameLoaded -= OnLoaded;
                SaveSystem_UMFOSS.Instance.OnLoadFailed -= OnLoadFailed;
            }
        }

        // ─────────────────────────────────────────────
        // UI Creation (Programmatic)
        // ─────────────────────────────────────────────

        private void CreateUI()
        {
            // Ensure EventSystem exists
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<EventSystem>();
                System.Type newInputModule = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (newInputModule != null) eventSystemObj.AddComponent(newInputModule);
                else eventSystemObj.AddComponent<StandaloneInputModule>();
            }

            // Create Canvas
            GameObject canvasObj = new GameObject("SaveSystemDemoCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            // Fullscreen Dark Background
            GameObject bgPanel = CreatePanel(canvasObj.transform, "Background", new Vector2(0,0), new Vector2(1,1), Vector2.zero, Vector2.zero, new Color(0.08f, 0.08f, 0.11f, 1f));

            // ── Left Panel: Status Display ──
            GameObject leftPanel = CreatePanel(canvasObj.transform, "StatusPanel",
                new Vector2(0f, 0f), new Vector2(0f, 1f),
                new Vector2(30f, 30f), new Vector2(450f, -30f),
                new Color(0.12f, 0.13f, 0.17f, 0.95f));

            CreateHeader(leftPanel.transform, "StatusHeader", "System Status", 0);
            healthText = CreateLabel(leftPanel.transform, "HealthText", "Health: ---", 1);
            inventoryText = CreateLabel(leftPanel.transform, "InventoryText", "Inventory: ---", 2);
            timestampText = CreateLabel(leftPanel.transform, "TimestampText", "Last Saved: Never", 3);
            savePathText = CreateLabel(leftPanel.transform, "SavePathText", "Save Path: ---", 4);
            statusText = CreateLabel(leftPanel.transform, "StatusText", "Status: Ready", 6);

            // ── Right Panel: Actions ──
            GameObject rightPanel = CreatePanel(canvasObj.transform, "ActionsPanel",
                new Vector2(1f, 0f), new Vector2(1f, 1f),
                new Vector2(-350f, 30f), new Vector2(-30f, -30f),
                new Color(0.12f, 0.13f, 0.17f, 0.95f));

            CreateHeader(rightPanel.transform, "GameActionsHeader", "Game Actions", 0);
            CreateButton(rightPanel.transform, "TakeDamageBtn", "Take Damage (-20)", -80f, OnTakeDamage, false);
            CreateButton(rightPanel.transform, "HealBtn", "Heal (+15)", -130f, OnHeal, false);
            CreateButton(rightPanel.transform, "AddSwordBtn", "Add Sword", -180f, OnAddSword, false);
            CreateButton(rightPanel.transform, "AddShieldBtn", "Add Shield", -230f, OnAddShield, false);
            CreateButton(rightPanel.transform, "ClearInventoryBtn", "Clear Inventory", -280f, OnClearInventory, false);

            CreateHeader(rightPanel.transform, "PersistenceHeader", "Persistence", 7);
            CreateButton(rightPanel.transform, "SaveBtn", "Save (Slot1)", -395f, OnSave, true);
            CreateButton(rightPanel.transform, "LoadBtn", "Load (Slot1)", -445f, OnLoad, true);
            CreateButton(rightPanel.transform, "DeleteBtn", "Delete Save", -495f, OnDeleteSave, false);
            CreateButton(rightPanel.transform, "NewGameBtn", "New Game", -545f, OnNewGameClicked, false);
        }

        private GameObject CreatePanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color bgColor)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            Image img = panel.AddComponent<Image>();
            img.color = bgColor;
            return panel;
        }

        private Text CreateHeader(Transform parent, string name, string text, int lineIndex)
        {
            GameObject labelObj = new GameObject(name);
            labelObj.transform.SetParent(parent, false);

            RectTransform rt = labelObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(25, -20 - (lineIndex * 45));
            rt.sizeDelta = new Vector2(-50, 40);

            Text label = labelObj.AddComponent<Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 26;
            label.fontStyle = FontStyle.Bold;
            label.color = new Color(0.3f, 0.75f, 1f, 1f);
            label.alignment = TextAnchor.MiddleLeft;

            return label;
        }

        private Text CreateLabel(Transform parent, string name, string text, int lineIndex)
        {
            GameObject labelObj = new GameObject(name);
            labelObj.transform.SetParent(parent, false);

            RectTransform rt = labelObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(25, -20 - (lineIndex * 45));
            rt.sizeDelta = new Vector2(-50, 40);

            Text label = labelObj.AddComponent<Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 18;
            label.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;

            return label;
        }

        private void CreateButton(Transform parent, string name, string text,
            float yPos, UnityEngine.Events.UnityAction onClick, bool isPrimary)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, yPos);
            rt.sizeDelta = new Vector2(270, 42);

            Image img = btnObj.AddComponent<Image>();
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;

            Color baseColor = isPrimary ? new Color(0.15f, 0.5f, 0.85f, 1f) : new Color(0.2f, 0.22f, 0.28f, 1f);
            Color highlightColor = isPrimary ? new Color(0.25f, 0.6f, 0.95f, 1f) : new Color(0.3f, 0.35f, 0.42f, 1f);
            Color pressedColor = isPrimary ? new Color(0.1f, 0.4f, 0.7f, 1f) : new Color(0.15f, 0.17f, 0.23f, 1f);

            img.color = baseColor;
            ColorBlock colors = btn.colors;
            colors.normalColor = baseColor;
            colors.highlightedColor = highlightColor;
            colors.pressedColor = pressedColor;
            colors.selectedColor = baseColor;
            btn.colors = colors;

            btn.onClick.AddListener(onClick);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            Text btnText = textObj.AddComponent<Text>();
            btnText.text = text;
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            btnText.fontSize = isPrimary ? 20 : 18;
            btnText.fontStyle = isPrimary ? FontStyle.Bold : FontStyle.Normal;
            btnText.color = Color.white;
            btnText.alignment = TextAnchor.MiddleCenter;
        }

        // ─────────────────────────────────────────────
        // Button Handlers
        // ─────────────────────────────────────────────

        private void OnTakeDamage()
        {
            foreach (var hs in FindObjectsOfType<HealthSystem_UMFOSS>())
            {
                hs.TakeDamage(20f);
            }
            UpdateDisplay();
        }

        private void OnHeal()
        {
            foreach (var hs in FindObjectsOfType<HealthSystem_UMFOSS>())
            {
                hs.Heal(15f);
            }
            UpdateDisplay();
        }

        private void OnAddSword()
        {
            foreach (var inv in FindObjectsOfType<InventorySystem_UMFOSS>())
            {
                if (swordItem != null) inv.AddItem(swordItem, 1);
            }
            UpdateDisplay();
        }

        private void OnAddShield()
        {
            foreach (var inv in FindObjectsOfType<InventorySystem_UMFOSS>())
            {
                if (shieldItem != null) inv.AddItem(shieldItem, 1);
            }
            UpdateDisplay();
        }

        private void OnClearInventory()
        {
            foreach (var inv in FindObjectsOfType<InventorySystem_UMFOSS>())
            {
                inv.ClearInventory();
            }
            UpdateDisplay();
        }

        private void OnSave()
        {
            if (SaveSystem_UMFOSS.Instance != null)
            {
                SaveSystem_UMFOSS.Instance.Save("Slot1");
            }
        }

        private void OnLoad()
        {
            if (SaveSystem_UMFOSS.Instance != null)
            {
                SaveSystem_UMFOSS.Instance.Load("Slot1");
                UpdateDisplay();
            }
        }

        private void OnDeleteSave()
        {
            if (SaveSystem_UMFOSS.Instance != null)
            {
                SaveSystem_UMFOSS.Instance.Delete("Slot1");
                timestampText.text = "Last Saved: ---";
                statusText.text = "Status: Save file deleted.";
            }
        }

        private void OnNewGameClicked()
        {
            if (SaveSystem_UMFOSS.Instance != null)
            {
                SaveSystem_UMFOSS.Instance.NewGame("Slot1");

                // Reset game state
                foreach (var hs in FindObjectsOfType<HealthSystem_UMFOSS>()) hs.ResetHealth();
                foreach (var inv in FindObjectsOfType<InventorySystem_UMFOSS>()) inv.ClearInventory();

                UpdateDisplay();
            }
        }

        // ─────────────────────────────────────────────
        // Event Callbacks
        // ─────────────────────────────────────────────

        private void OnSaved(string slot)
        {
            if (slot == "AutoSave")
            {
                if (timestampText != null) timestampText.text = $"Last Auto-Saved: {System.DateTime.Now.ToString("g")}";
                if (statusText != null) statusText.text = $"Status: Auto-Save completed ({slot})";
            }
            else
            {
                if (timestampText != null) timestampText.text = $"Last Saved: {System.DateTime.Now.ToString("g")}";
                if (statusText != null) statusText.text = $"Status: Saved successfully to {slot}";

                if (savePathText != null)
                {
                    savePathText.text = $"Save Path: {Application.persistentDataPath}/Saves/{slot}.sav";
                }
            }
        }

        private void OnLoaded(string slot)
        {
            if (statusText != null) statusText.text = $"Status: Loaded successfully from {slot}";
            UpdateDisplay();
        }

        private void OnLoadFailed(string slot, string error)
        {
            if (statusText != null) statusText.text = $"Status: <color=red>Load Failed ({error})</color>";
        }

        // ─────────────────────────────────────────────
        // View Updates
        // ─────────────────────────────────────────────

        private void UpdateDisplay()
        {
            // Health - auto fetch to gracefully handle duplicates if user messed up inspector
            HealthSystem_UMFOSS activeHealth = FindObjectOfType<HealthSystem_UMFOSS>();
            if (healthText != null && activeHealth != null)
            {
                healthText.text = $"Health: {activeHealth.CurrentHealth:F0} / {activeHealth.MaxHealth:F0}  " +
                    (activeHealth.IsAlive ? "(Alive)" : "(DEAD)");
            }

            // Inventory
            InventorySystem_UMFOSS activeInventory = FindObjectOfType<InventorySystem_UMFOSS>();
            if (inventoryText != null && activeInventory != null)
            {
                var slots = activeInventory.Slots;
                string list = "";
                foreach (var s in slots)
                {
                    if (s.itemData != null) list += $"{s.quantity}x {s.itemData.itemName}, ";
                }
                if (list.EndsWith(", ")) list = list.Substring(0, list.Length - 2);
                if (list == "") list = "Empty";

                inventoryText.text = $"Inventory: {list}";
            }
        }
    }
}
