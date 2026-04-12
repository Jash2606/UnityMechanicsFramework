# Save and Load System - Video and Integration Guide

## Mechanic Metadata

| Field | Value |
|---|---|
| Mechanic | Save and Load System |
| Author | [Jash Savaliya](https://github.com/Jash2606), [Rushabh Mistry](https://github.com/Rushhaabhhh), [Sarthak Pandey](https://github.com/SarthakPaandey) |
| Namespace | GameplayMechanicsUMFOSS.Systems |
| Runtime Entry | Runtime/Systems/SaveSystem/SaveSystem_UMFOSS.cs |
| Demo Scene | Samples~/SaveSystem/Assets/Scenes/DemoScene.unity |
| Video | https://drive.google.com/drive/folders/1d3vY3Rbn1R7yiDcg34YOhl4gvYksV0Lm?usp=sharing |

## What This Mechanic Does

This mechanic provides a generic, extensible persistence system for Unity gameplay data.

Any system can participate in saving by implementing ISaveable_UMFOSS, then registering with SaveSystem_UMFOSS. The save core does not need to know mechanic-specific internals, which keeps the architecture modular and scalable.

Key capabilities:

1. Multiple save slots such as Slot1, Slot2, and AutoSave
2. Save versioning with migration hooks
3. Optional XOR-based anti-tamper encryption
4. Async scene restoration before state restore
5. Event callbacks for save success and failure paths
6. JSON save files written under Application.persistentDataPath/Saves

## Why This Exists

PlayerPrefs is fine for tiny scalar settings, but it does not scale well for structured progression data like inventory, quest state, world flags, and cross-scene progress. This mechanic provides a maintainable file-based approach with better lifecycle handling, inspectable data, and version-aware evolution.

## Architecture

The system is organized into four parts:

1. ISaveable_UMFOSS
	Any mechanic implements GetSaveID, CaptureState, and RestoreState.

2. SaveData_UMFOSS
	The outer save payload containing metadata and serialized per-saveable state entries.

3. SaveFileHandler_UMFOSS
	Handles disk IO, JSON read/write, optional encryption/decryption, slot checks, and metadata read path.

4. SaveSystem_UMFOSS
	The orchestrator that registers saveables, performs save/load flow, runs version migration, and emits events.

## Save Flow

1. Build SaveData_UMFOSS with current version, slot, timestamp, and active scene.
2. Iterate all registered ISaveable_UMFOSS instances.
3. Call CaptureState per saveable, serialize to JSON, store by unique SaveID.
4. Persist to disk through SaveFileHandler_UMFOSS.
5. Emit OnGameSaved or OnSaveFailed.

## Load Flow

1. Load file by slot via SaveFileHandler_UMFOSS.
2. Validate version direction and run migration if needed.
3. If scene differs, load saved scene asynchronously.
4. Restore state per registered saveable by SaveID lookup.
5. Skip missing IDs safely and log warnings.
6. Emit OnGameLoaded or OnLoadFailed.

## Public API Summary

SaveSystem_UMFOSS exposes:

1. Register and Deregister for ISaveable lifecycle
2. Save
3. Load
4. Delete
5. NewGame
6. SaveExists
7. GetAllSaveSlots
8. GetSaveMetadata

## Demo Scene Behavior

Open Samples~/SaveSystem/Assets/Scenes/DemoScene.unity and press Play.

Demo UI actions:

1. Take Damage
2. Heal
3. Add Sword
4. Add Shield
5. Clear Inventory
6. Save Slot1
7. Load Slot1
8. Delete Save
9. New Game

On-screen information:

1. Current Health
2. Current Inventory
3. Last Saved timestamp
4. Save file path
5. Status messages

## What The Video Walkthrough Covers

1. Mechanic overview and problem statement
2. Script architecture and responsibilities
3. SaveSystem inspector configuration
4. ISaveable implementation pattern using Health and Inventory examples
5. Full save-load-delete-new game cycle in Play Mode
6. Save file path and save content inspection
7. Version migration concept and expected behavior
8. How to integrate ISaveable into new mechanics without changing core save code

## Acceptance Coverage Checklist

This guide and demo are intended to validate:

1. Health and Inventory state capture and restore
2. Slot isolation between save files
3. Graceful handling for missing or corrupt save data
4. Version check and migration path behavior
5. Metadata retrieval for UI use cases
6. Optional encryption behavior when enabled
7. Registration and deregistration lifecycle alignment with OnEnable and OnDisable

## Important Notes

1. Use stable unique IDs for saveables.
2. Save files are stored in Application.persistentDataPath/Saves.
3. Unknown or missing SaveID entries should never crash restore.
4. Auto-save interval of 0 means disabled.
5. JsonUtility has limitations for advanced polymorphic payloads.

## Video Link

Watch the full walkthrough here:

https://drive.google.com/drive/folders/1d3vY3Rbn1R7yiDcg34YOhl4gvYksV0Lm?usp=sharing
