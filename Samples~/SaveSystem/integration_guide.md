# Save & Load System — Unity Hub Setup & Integration Guide

> Complete step-by-step guide to set up, run, and integrate the Save & Load System mechanic from the UnityMechanicsFramework into your Unity project.

---

## Table of Contents

1. [Prerequisites](#1-prerequisites)
2. [Install Unity via Unity Hub](#2-install-unity-via-unity-hub)
3. [Get the Repository](#3-get-the-repository)
4. [Add the Project to Unity Hub](#4-add-the-project-to-unity-hub)
5. [Import the Save System Sample](#5-import-the-save-system-sample)
6. [Create the Required Resources Folder](#6-create-the-required-resources-folder)
7. [Open and Verify the Demo Scene](#7-open-and-verify-the-demo-scene)
8. [Run the Demo](#8-run-the-demo)
9. [Verify the Save File on Disk](#9-verify-the-save-file-on-disk)
10. [Integrate ISaveable Into Your Own Mechanic](#10-integrate-isaveable-into-your-own-mechanic)
11. [Advanced Configuration](#11-advanced-configuration)
12. [Troubleshooting](#12-troubleshooting)

---

## 1. Prerequisites

| Requirement | Details |
|---|---|
| **Unity Hub** | [Download here](https://unity.com/download) — latest version |
| **Unity Editor** | 2021.3 LTS (minimum), **2022.3 LTS recommended**, Unity 6 supported |
| **Git** | Required for cloning the repository ([download](https://git-scm.com/downloads)) |
| **OS** | Windows 10/11, macOS 10.15+, or Ubuntu 20.04+ |
| **Disk Space** | ~2 GB for Unity Editor + project |

---

## 2. Install Unity via Unity Hub

If you already have Unity 2021.3+ installed, skip to [Step 3](#3-get-the-repository).

1. **Open Unity Hub**
2. Go to the **Installs** tab (left sidebar)
3. Click **"Install Editor"**
4. Select **Unity 2022.3 LTS** (recommended)
5. In the modules selection:
   - ✅ Check **"Microsoft Visual Studio Community"** (or your preferred IDE)
   - ✅ Check your target platform (Windows Build Support is usually pre-selected)
6. Click **"Install"** and wait for completion

> [!TIP]
> Unity 2022.3 LTS is recommended because it has the best stability and the widest package compatibility. The Save System works on 2021.3+ and Unity 6 as well.

---

## 3. Get the Repository

### Option A — Clone via Git (Recommended)

Open a terminal/PowerShell and run:

```bash
git clone https://github.com/Jash2606/UnityMechanicsFramework.git
```

This creates a `UnityMechanicsFramework/` folder in your current directory.

### Option B — Download ZIP

1. Go to [https://github.com/Jash2606/UnityMechanicsFramework](https://github.com/Jash2606/UnityMechanicsFramework)
2. Click the green **"Code"** button → **"Download ZIP"**
3. Extract the ZIP to a location of your choice (e.g., `C:\Unity\Projects\`)

### Option C — Import as a Unity Package (for existing projects)

If you want to add the Save System to an **existing Unity project** without cloning the full repo:

1. Open your existing Unity project
2. Go to **Window → Package Manager**
3. Click the **"+"** button in the top-left
4. Select **"Add package from git URL..."**
5. Enter: `https://github.com/Jash2606/UnityMechanicsFramework.git`
6. Click **Add**
7. Unity will download and import the package automatically

Then skip to [Step 5](#5-import-the-save-system-sample).

---

## 4. Add the Project to Unity Hub

1. Open **Unity Hub**
2. Go to the **Projects** tab (left sidebar)
3. Click **"Open" → "Add project from disk"**
4. Browse to the `UnityMechanicsFramework/` folder you cloned/extracted
5. Select the folder and click **"Open"**

![Unity Hub Add Project](/path/to/screenshot) <!-- placeholder for future screenshot -->

6. If Unity Hub asks to choose an Editor version:
   - Select **2022.3 LTS** (or any 2021.3+ version you have installed)
   - Click **"Open with..."**

7. **Wait for import** — The first import takes 2-5 minutes as Unity compiles all scripts and generates the Library folder

> [!NOTE]
> If you see a **"Safe Mode"** dialog about compilation errors, it usually means TextMeshPro isn't installed. Click **"Ignore"** to enter the editor, then go to `Window → Package Manager` and install **TextMeshPro**.

---

## 5. Import the Save System Sample

The demo files live in `Samples~/` which is hidden from Unity's Asset browser by default. You need to import them.

### Option A — Via Unity Package Manager (Recommended)

1. Go to **Window → Package Manager**
2. In the **top-left dropdown**, change to **"Packages: In Project"**
3. Find **"Unity Mechanics Framework"** in the package list
4. Click on it to expand its details
5. Scroll down to the **"Samples"** section
6. Click the **"Import"** button next to **"Save System Demo"**

The sample files will be imported to:
```
Assets/Samples/Unity Mechanics Framework/1.0.0/Save System Demo/
```

### Option B — Manual Copy

If the package manager import doesn't work:

1. Open your OS file explorer
2. Navigate to the project root: `UnityMechanicsFramework/`
3. Go into `Samples~/SaveSystem/`
4. Copy the entire `SaveSystem/` folder contents
5. Paste into your project's `Assets/` folder (e.g., create `Assets/SaveSystemDemo/`)
6. Return to Unity and wait for it to import the new files

> [!IMPORTANT]
> When copying manually, you must also copy the scripts from `Samples~/SaveSystem/Assets/Scripts/` — these contain the demo HealthSystem, InventorySystem, ItemData, and DemoUI scripts.

---

## 6. Create the Required Resources Folder

The InventorySystem demo uses `Resources.Load()` to restore items by name. You need to create the item assets in the correct Resources path.

### Step 6.1 — Create the folder structure

1. In the Unity **Project** window, right-click in `Assets/`
2. Click **Create → Folder** and name it `Resources`
3. Right-click on the new `Resources/` folder
4. Click **Create → Folder** and name it `Items`

Your folder structure should now look like:
```
Assets/
  Resources/
    Items/        ← item assets go here
```

### Step 6.2 — Create the Sword item

1. Right-click inside `Assets/Resources/Items/`
2. Select **Create → UMFOSS → SaveSystem Demo → Item Data**
3. A new ScriptableObject asset appears — rename it to exactly: **`Sword`**
4. Select the `Sword` asset and in the **Inspector**:
   - Set **Item Name** to `Sword`
   - Set **Description** to `A sharp blade` (optional)
   - Leave **Max Stack Size** at `99`

### Step 6.3 — Create the Shield item

1. Right-click inside `Assets/Resources/Items/` again
2. Select **Create → UMFOSS → SaveSystem Demo → Item Data**
3. Rename it to exactly: **`Shield`**
4. In the Inspector:
   - Set **Item Name** to `Shield`
   - Set **Description** to `A sturdy shield` (optional)

> [!WARNING]
> The **file names** (`Sword`, `Shield`) must match exactly — they are used by `Resources.Load<ItemData_UMFOSS>("Items/Sword")` to restore inventory items on load. A mismatch means items won't restore.

---

## 7. Open and Verify the Demo Scene

### Step 7.1 — Open the scene

In the **Project** window, navigate to:

- **If imported via UPM**: `Assets/Samples/Unity Mechanics Framework/1.0.0/Save System Demo/Assets/Scenes/`
- **If manually copied**: `Assets/SaveSystemDemo/Assets/Scenes/`

Double-click **`DemoScene.unity`** to open it.

### Step 7.2 — Verify the Hierarchy

In the **Hierarchy** window, you should see exactly these GameObjects:

| GameObject | Components | Purpose |
|---|---|---|
| **Main Camera** | Camera, AudioListener | Scene camera |
| **Directional Light** | Light | Scene lighting |
| **Managers** | `SaveSystem_UMFOSS` | The singleton save system coordinator |
| **Player** | `HealthSystem_UMFOSS`, `InventorySystem_UMFOSS` | Two ISaveable implementations |
| **DemoManager** | `SaveSystemDemoUI_UMFOSS` | Creates the demo UI at runtime |

### Step 7.3 — Verify SaveSystem settings

1. Click on **Managers** in the Hierarchy
2. In the Inspector, you should see `SaveSystem_UMFOSS` with:

| Setting | Value | Meaning |
|---|---|---|
| Current Save Version | `1` | Save format version |
| Auto Save On Scene Unload | `true` or `false` | Auto-save when leaving a scene |
| Auto Save Interval | `0` | 0 = disabled; set to e.g. `60` for every 60 seconds |
| Default Slot Name | `Slot1` | The default save slot |
| Encrypt Save File | `false` | Enable for XOR encryption |
| Encryption Key | (empty or key) | Required if encryption is enabled |

### Step 7.4 — Wire up DemoManager references (if needed)

1. Click on **DemoManager** in the Hierarchy
2. In the Inspector, check `SaveSystemDemoUI_UMFOSS`:
   - **Health System**: Drag the **Player** GameObject here (or leave empty for auto-find)
   - **Inventory System**: Drag the **Player** GameObject here (or leave empty for auto-find)
   - **Sword Item**: Drag the `Assets/Resources/Items/Sword` asset here (or leave empty for auto-load)
   - **Shield Item**: Drag the `Assets/Resources/Items/Shield` asset here (or leave empty for auto-load)

> [!TIP]
> All references auto-resolve at runtime via `FindObjectOfType()` and `Resources.Load()`. Pre-assigning them is optional but eliminates startup warnings.

---

## 8. Run the Demo

### Step 8.1 — Press Play

Click the **▶ Play** button in the Unity Editor toolbar.

### Step 8.2 — The UI

The demo UI appears with two panels:

**Left Panel — System Status:**
| Display | Shows |
|---|---|
| Health | Current / Max health and alive status |
| Inventory | List of items and quantities |
| Last Saved | Timestamp of the most recent save |
| Save Path | Full file path to the save file on disk |
| Status | Result of the last operation |

**Right Panel — Actions:**

| Button | What It Does |
|---|---|
| **Take Damage (-20)** | Reduces health by 20 |
| **Heal (+15)** | Restores 15 health |
| **Add Sword** | Adds 1 Sword to inventory |
| **Add Shield** | Adds 1 Shield to inventory |
| **Clear Inventory** | Removes all items |
| **Save (Slot1)** | Saves current state to Slot1 |
| **Load (Slot1)** | Loads saved state from Slot1 |
| **Delete Save** | Deletes the Slot1 save file |
| **New Game** | Deletes save and resets all state |

### Step 8.3 — Test the save/load cycle

Follow this exact sequence to verify everything works:

```
1. Click "Take Damage" 3 times          → Health: 40/100
2. Click "Add Sword" twice              → Inventory: Sword x2
3. Click "Add Shield" once              → Inventory: Sword x2, Shield x1
4. Click "Save (Slot1)"                 → Status: "Saved successfully to Slot1"
5. Note the health and inventory values
6. Click "Take Damage" 2 more times     → Health: 0/100 (DEAD)
7. Click "Clear Inventory"              → Inventory: Empty
8. Click "Load (Slot1)"                 → Health: 40/100, Inventory: Sword x2, Shield x1
9. ✅ State is restored to what you saved in step 4!
```

---

## 9. Verify the Save File on Disk

### Step 9.1 — Find the save file

The **Save Path** displayed in the demo UI shows the full file path. It will be something like:

| Platform | Path |
|---|---|
| **Windows** | `C:\Users\<you>\AppData\LocalLow\<company>\<product>\Saves\Slot1.sav` |
| **macOS** | `~/Library/Application Support/<company>/<product>/Saves/Slot1.sav` |
| **Linux** | `~/.config/unity3d/<company>/<product>/Saves/Slot1.sav` |

### Step 9.2 — Open the file

Open `Slot1.sav` in any text editor (VS Code, Notepad++, Notepad). You should see valid JSON:

```json
{
    "saveVersion": 1,
    "saveSlotName": "Slot1",
    "lastSavedTimestamp": "2026-04-12T16:50:00.1234567+05:30",
    "sceneNameOnSave": "DemoScene",
    "keys": [
        "HealthSystem_Player",
        "Inventory_Player"
    ],
    "values": [
        "{\"currentHealth\":40.0,\"maxHealth\":100.0}",
        "{\"slots\":[{\"itemDataName\":\"Sword\",\"quantity\":2},{\"itemDataName\":\"Shield\",\"quantity\":1}]}"
    ]
}
```

> [!NOTE]
> The `keys` and `values` arrays are the serialized form of the `SerializableDictionary` — Unity's `JsonUtility` can't serialize `Dictionary<K,V>` directly, so we convert to parallel arrays.

---

## 10. Integrate ISaveable Into Your Own Mechanic

This is the core integration pattern. Follow these steps for **any** script you want to make saveable.

### Step 10.1 — Add the interface

```csharp
using UnityEngine;
using GameplayMechanicsUMFOSS.Systems;

public class ScoreManager : MonoBehaviour, ISaveable_UMFOSS
{
```

### Step 10.2 — Add a unique ID field

```csharp
    [Header("Save Settings")]
    [SerializeField] private string uniqueID = "ScoreManager_Main";
```

> [!IMPORTANT]
> The `uniqueID` must be unique across ALL saveables in the scene. Two saveables with the same ID = the last one overwrites the other on save (data loss).

### Step 10.3 — Define your save data structure

```csharp
    [System.Serializable]
    private class ScoreSaveData
    {
        public int totalScore;
        public int highScore;
        public int levelsCompleted;
    }
```

The struct/class must be `[System.Serializable]` and contain only types that `JsonUtility` can serialize (primitives, strings, arrays, lists, nested serializable types).

### Step 10.4 — Implement GetSaveID()

```csharp
    public string GetSaveID()
    {
        return uniqueID;
    }
```

### Step 10.5 — Implement CaptureState()

Pack your current state into the save data object:

```csharp
    public object CaptureState()
    {
        return new ScoreSaveData
        {
            totalScore = this.totalScore,
            highScore = this.highScore,
            levelsCompleted = this.levelsCompleted
        };
    }
```

### Step 10.6 — Implement RestoreState()

Receive the JSON string, deserialize it, and apply:

```csharp
    public void RestoreState(object state)
    {
        // State arrives as a JSON string from SaveSystem
        string json = state as string;
        if (string.IsNullOrEmpty(json)) return;

        ScoreSaveData data = JsonUtility.FromJson<ScoreSaveData>(json);
        this.totalScore = data.totalScore;
        this.highScore = data.highScore;
        this.levelsCompleted = data.levelsCompleted;
    }
```

### Step 10.7 — Register on OnEnable / Deregister on OnDisable

```csharp
    private void OnEnable()
    {
        SaveSystem_UMFOSS.Instance?.Register(this);
    }

    private void OnDisable()
    {
        SaveSystem_UMFOSS.Instance?.Deregister(this);
    }
}
```

### Complete Example

```csharp
using UnityEngine;
using GameplayMechanicsUMFOSS.Systems;

public class ScoreManager : MonoBehaviour, ISaveable_UMFOSS
{
    [Header("Score Settings")]
    [SerializeField] private int totalScore;
    [SerializeField] private int highScore;
    [SerializeField] private int levelsCompleted;

    [Header("Save Settings")]
    [SerializeField] private string uniqueID = "ScoreManager_Main";

    [System.Serializable]
    private class ScoreSaveData
    {
        public int totalScore;
        public int highScore;
        public int levelsCompleted;
    }

    // ISaveable_UMFOSS implementation
    public string GetSaveID() => uniqueID;

    public object CaptureState() => new ScoreSaveData
    {
        totalScore = this.totalScore,
        highScore = this.highScore,
        levelsCompleted = this.levelsCompleted
    };

    public void RestoreState(object state)
    {
        string json = state as string;
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<ScoreSaveData>(json);
        totalScore = data.totalScore;
        highScore = data.highScore;
        levelsCompleted = data.levelsCompleted;
    }

    private void OnEnable() => SaveSystem_UMFOSS.Instance?.Register(this);
    private void OnDisable() => SaveSystem_UMFOSS.Instance?.Deregister(this);

    // Your game logic...
    public void AddScore(int points)
    {
        totalScore += points;
        if (totalScore > highScore) highScore = totalScore;
    }
}
```

**That's it.** Zero changes to SaveSystem. Your scores are now automatically saved and loaded.

### Step 10.8 — Save and Load from anywhere

```csharp
// Save all registered saveables to Slot1
SaveSystem_UMFOSS.Instance.Save("Slot1");

// Load from Slot1 (restores all saveables)
SaveSystem_UMFOSS.Instance.Load("Slot1");

// Other useful API
SaveSystem_UMFOSS.Instance.Delete("Slot1");
SaveSystem_UMFOSS.Instance.NewGame("Slot1");
bool exists = SaveSystem_UMFOSS.Instance.SaveExists("Slot1");
List<string> allSlots = SaveSystem_UMFOSS.Instance.GetAllSaveSlots();
```

---

## 11. Advanced Configuration

### Multiple Save Slots

```csharp
// Save to different slots
SaveSystem_UMFOSS.Instance.Save("Slot1");
SaveSystem_UMFOSS.Instance.Save("Slot2");
SaveSystem_UMFOSS.Instance.Save("AutoSave");

// List all existing save slots
List<string> slots = SaveSystem_UMFOSS.Instance.GetAllSaveSlots();
// Returns: ["Slot1", "Slot2", "AutoSave"]

// Get metadata for UI display (timestamp, scene, version)
SaveData_UMFOSS meta = SaveSystem_UMFOSS.Instance.GetSaveMetadata("Slot1");
Debug.Log($"Last saved: {meta.lastSavedTimestamp}");
Debug.Log($"Scene: {meta.sceneNameOnSave}");
```

### Enable Auto-Save

In the Inspector on your `SaveSystem_UMFOSS` component:
- Set **Auto Save Interval** to e.g. `120` (auto-saves every 2 minutes)
- Enable **Auto Save On Scene Unload** to save when changing scenes

Auto-saves go to the `"AutoSave"` slot.

### Enable Encryption

1. Check **Encrypt Save File** in the Inspector
2. Set **Encryption Key** to a secret string (e.g., `"MyGameKey2024"`)

> [!WARNING]
> This is XOR encryption — anti-tamper for casual games, not cryptographic security. It prevents players from casually editing the save file in a text editor.

### Subscribe to Events

```csharp
void OnEnable()
{
    SaveSystem_UMFOSS.Instance.OnGameSaved += OnSaved;
    SaveSystem_UMFOSS.Instance.OnGameLoaded += OnLoaded;
    SaveSystem_UMFOSS.Instance.OnSaveFailed += OnError;
    SaveSystem_UMFOSS.Instance.OnLoadFailed += OnError;
}

void OnDisable()
{
    if (SaveSystem_UMFOSS.Instance != null)
    {
        SaveSystem_UMFOSS.Instance.OnGameSaved -= OnSaved;
        SaveSystem_UMFOSS.Instance.OnGameLoaded -= OnLoaded;
        SaveSystem_UMFOSS.Instance.OnSaveFailed -= OnError;
        SaveSystem_UMFOSS.Instance.OnLoadFailed -= OnError;
    }
}

void OnSaved(string slot) => Debug.Log($"Game saved to {slot}!");
void OnLoaded(string slot) => Debug.Log($"Game loaded from {slot}!");
void OnError(string slot, string reason) => Debug.LogError($"Error on {slot}: {reason}");
```

### Version Migration

When you add new fields to your save data after players already have save files:

1. Increment **Current Save Version** on the `SaveSystem_UMFOSS` Inspector (e.g., 1 → 2)
2. Open `SaveSystem_UMFOSS.cs` and add a migration case in `MigrateSave()`:

```csharp
private void MigrateSave(SaveData_UMFOSS saveData)
{
    int fromVersion = saveData.saveVersion;
    switch (fromVersion)
    {
        case 0:
            Debug.Log("[SaveSystem] Migrated save from v0 → v1");
            goto case 1;
        case 1:
            // v1 → v2: Added "levelsCompleted" to ScoreManager
            // No action needed — new fields default to 0/null on deserialization
            Debug.Log("[SaveSystem] Migrated save from v1 → v2");
            break;
        default:
            Debug.LogWarning($"[SaveSystem] No migration path from version {fromVersion}.");
            break;
    }
}
```

Old saves load safely — `JsonUtility.FromJson<T>()` ignores unknown fields and defaults missing fields to their type defaults (`0`, `null`, `false`, etc.).

---

## 12. Troubleshooting

| Problem | Cause | Solution |
|---|---|---|
| **"SaveSystem not found" warning** | No `SaveSystem_UMFOSS` component in the scene | Add a GameObject with `SaveSystem_UMFOSS` to your scene |
| **Inventory items don't restore** | ItemData assets not in `Resources/Items/` or named wrong | Ensure assets are at `Assets/Resources/Items/Sword.asset` (file name = resource name) |
| **Save file not created** | Write permissions or path issue | Check Console for the save path; verify the folder exists and is writable |
| **Scene won't compile** | Missing TextMeshPro package | `Window → Package Manager → Unity Registry → TextMeshPro → Install` |
| **NullReferenceException on Play** | Missing item references on DemoManager | Assign Sword/Shield items, or create them in `Resources/Items/` |
| **"Duplicate singleton" warning** | Two GameObjects with `SaveSystem_UMFOSS` | Delete one — only one SaveSystem should exist per scene |
| **Load does nothing / no state change** | Save file doesn't exist yet | Save first before loading; check if `SaveExists()` returns true |
| **Encrypted save won't load** | Key mismatch or file was saved without encryption | Ensure the same encryption key is used for both save and load |
| **"Save version newer" error on load** | Player has a save from a newer build | This is by design — prevents data corruption from version mismatch |
| **Package not showing in Package Manager** | Git URL not resolving | Check your internet connection; ensure the URL is correct and the repo is public |
| **Sample not appearing in Package Manager** | package.json might not be loaded | Close and reopen the Package Manager; or use Manual Copy (Option B in Step 5) |

### Debug Commands (Console)

Paste these in a test script to diagnose issues:

```csharp
Debug.Log("Save path: " + Application.persistentDataPath);
Debug.Log("SaveSystem exists: " + (SaveSystem_UMFOSS.Instance != null));
Debug.Log("Save slots on disk: " + string.Join(", ", SaveFileHandler_UMFOSS.GetAllSaveSlots()));
Debug.Log("Slot1 exists: " + SaveFileHandler_UMFOSS.SaveExists("Slot1"));
```

---


After setup, integrating ISaveable into any new mechanic — just implement 3 methods and 2 lifecycle hooks.
