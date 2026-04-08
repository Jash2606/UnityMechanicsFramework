using UnityEngine;
using GameplayMechanicsUMFOSS.Systems;

namespace GameplayMechanicsUMFOSS.Samples.SaveSystem
{
    /// <summary>
    /// Demo health system implementing ISaveable_UMFOSS.
    /// Demonstrates saving and restoring simple float values.
    /// </summary>
    public class HealthSystem_UMFOSS : MonoBehaviour, ISaveable_UMFOSS
    {
        // ─────────────────────────────────────────────
        // Serialized Fields
        // ─────────────────────────────────────────────

        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;

        [Header("Save Settings")]
        [Tooltip("Unique identifier for this saveable. Must be unique across all saveables.")]
        [SerializeField] private string uniqueID = "HealthSystem_Player";

        // ─────────────────────────────────────────────
        // Private Fields
        // ─────────────────────────────────────────────

        private float currentHealth;

        // ─────────────────────────────────────────────
        // Public Properties
        // ─────────────────────────────────────────────

        /// <summary>Current health value.</summary>
        public float CurrentHealth { get { return currentHealth; } }

        /// <summary>Maximum health value.</summary>
        public float MaxHealth { get { return maxHealth; } }

        /// <summary>Whether the entity is alive.</summary>
        public bool IsAlive { get { return currentHealth > 0f; } }

        // ─────────────────────────────────────────────
        // Unity Lifecycle
        // ─────────────────────────────────────────────

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        private void OnEnable()
        {
            // Register with SaveSystem when enabled.
            // If Instance is null (due to execution order on scene load), try finding it directly.
            SaveSystem_UMFOSS sys = SaveSystem_UMFOSS.Instance;
            if (sys == null) sys = FindObjectOfType<SaveSystem_UMFOSS>();

            if (sys != null)
            {
                sys.Register(this);
            }
            else
            {
                Debug.LogWarning($"[HealthSystem] SaveSystem not found. Save/load will not work for '{uniqueID}'.");
            }
        }

        private void OnDisable()
        {
            // Deregister when disabled
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
        /// Reduces health by the specified amount. Clamps at 0.
        /// </summary>
        public void TakeDamage(float amount)
        {
            currentHealth = Mathf.Max(0f, currentHealth - amount);
            Debug.Log($"[HealthSystem] Took {amount} damage. Health: {currentHealth}/{maxHealth}");
        }

        /// <summary>
        /// Restores health by the specified amount. Clamps at maxHealth.
        /// </summary>
        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            Debug.Log($"[HealthSystem] Healed {amount}. Health: {currentHealth}/{maxHealth}");
        }

        /// <summary>
        /// Resets health to maximum.
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
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
            return new HealthSaveData
            {
                currentHealth = this.currentHealth,
                maxHealth = this.maxHealth
            };
        }

        public void RestoreState(object state)
        {
            // State arrives as a JSON string from SaveSystem
            string json = state as string;
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning($"[HealthSystem] RestoreState received null/empty data for '{uniqueID}'.");
                return;
            }

            Debug.Log($"[HealthSystem] Attempting to restore from JSON: {json}");

            HealthSaveData data = new HealthSaveData();
            try
            {
                JsonUtility.FromJsonOverwrite(json, data);
                
                // Since the user accidentally duplicated the component in the scene,
                // we apply the loaded save to ALL copies so the UI never desyncs.
                foreach (var duplicate in FindObjectsOfType<HealthSystem_UMFOSS>())
                {
                    duplicate.currentHealth = data.currentHealth;
                    duplicate.maxHealth = data.maxHealth;
                }
                
                Debug.Log($"[HealthSystem] State restored SUCCESSFULLY. New Health: {currentHealth}/{maxHealth}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[HealthSystem] CRITICAL ERROR parsing Health JSON: {e}");
            }
        }
    }

    /// <summary>
    /// Serializable class for health data. Un-nested to guarantee JsonUtility support.
    /// </summary>
    [System.Serializable]
    public class HealthSaveData
    {
        public float currentHealth;
        public float maxHealth;
    }
}
