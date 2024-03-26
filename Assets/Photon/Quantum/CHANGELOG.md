# 3.0.0

**Breaking Changes**

- The Quantum SDK is now a unitypackage.
- Renamed PhotonDeterministic.dll to `Quantum.Deterministic.dll` and QuantumCore.dll to `Quantum.Engine.dll`.
- Upgraded network libraries to Photon Realtime 5 (see Realtime changelog separately).
- All Unity-only scripts have been put in `Quantum` namespace and have been given `Quantum` prefix (e.g. `MapData` -> `QuantumMapData`,  `EntityPrototype` -> `QuantumEntityPrototype`).
- The `PhysicsCollider` 2D and 3D components now have a field called `Layer Source` which defines where the layer info should come from. This might require colliders layers to be set again;
- `AssetBase` has been obsoleted as is no longer functional.
- `AssetObject` now derive from `UnityEngine.ScriptableObject`. This means that there is no longer the need for `AssetBase` wrapper and all the partial `AssetBase` extensions need to be moved to `AssetObject` definitions. We provide tools that migrate asset data without data loss.
- `AssetObjects` need to be created with `AssetObject.Create<T>` or `T.Create`.
- To add a new list of assets (e.g. with Addressables), mark a method with `[QuantumGlobalScriptableObjectLoaderMethod]` attribute.
- `AssetObjects` referencing other `AssetObjects` directly (not by `AssetRef<T>`) are supported, but will no longer be fully serializable by the default `IAssetSerializer`. This should be a concern for when any non-Unity runner is used or such assets are used in the `DynamicDB`.
- Removed "standalone assets" / `QPrefabs`. They have been fully replaced with standalone prototypes. All `_data` files should be removed.
- Removed built-in support for AssetBundles.
- `RuntimeConfig` and `RuntimePlayer` are serialized with Json when send to the server.
- `MapDataBakeCallbacks` implemented in a custom assembly it has to be made known by adding this `[assembly: QuantumMapBakeAssembly]` to the script.
- Changed the timing around the `GameStarted` callback. It is now only called once per started Quantum session and, when waiting for a snapshot, it will be called after the snapshot has arrived with `isResync` = true.
- `Frame.Heap` is no longer a `Heap*` but instead a reference to a managed `FrameHeap` instance, which shares the same API and adds allocation tracking capabilities. Direct usages of `frame.Heap->` can be replaced by `frame.Heap.` instead.
- Added  the `isResync` parameter to the `GameStarted` callback (which is true, when the callback is invoked after the game has been re-synced for example after a late-join).
- Changed `QuantumNavMesh.ImportSettings.LinkErrorCorrection` type from `bool` to `float` (representing a distance).
- `NavMeshRegionMask.HasValidRegions()` now returns `true` for the "MainArea", use `HasValidNoneMainRegion` instead to only query for non-MainArea regions
- AppSettings.RealtimeAppId was replaced by AppSettings.QuantumAppId, the Unity inspector will automatically swap them when the PhotonServerSettings asset is inspected.
- The Quantum3 server will automatically block all non-protocol messages and all Photon Realtime player properties. Unblock them using the Photon dashboard and set `BlockNonProtocolMessages` and `BlockPlayerProperties` to `false`.
- Removed `DeterministicGameMode.Spectating` (use `Multiplay`) because all online Quantum simulation are started in spectating mode by default until a player is added.
- Also check out other references 
  - Migration Guide: `https://doc.photonengine.com/quantum/v3/getting-started/migration-guide`
  - What's New: `https://doc.photonengine.com/quantum/v3/getting-started/whats-new`
  - Photon Realtime 5 changelogs (`Assets\Photon\PhotonLibs\changes-library.txt`, `Assets\Photon\PhotonRealtime\Code\changes-realtime.txt`)

**What's New**

- Added input delta compression.
- Increasing minimum Unity version to 2021 LTS.
- New start online protocol to support adding and removing players at runtime.
- Added Quantum webhooks for the Photon public cloud.
- Increased maximum player count to 128.
- Predicted commands.
- A new graphical demo menu (requires the import TextMeshPro local assets upon opening the scene for the first time).
- Added async support for connection handling.
- Native physics libraries (Windows only in Preview).
- Support for assets that are neither a Resouce nor Addressable. Direct references to such assets are stored in `QuantumUnityDB`.
- `QuantumUnityDB.Get[Global]AssetGuids` - ability to iterate over GUIDs, based on asset types.
- Support for `netstandard2.0` and `netstandard2.1`.
- Full support for Odin inspector.
- Quantum Unity inspector polish and adding code documentation foldouts.
- Newtonsoft-powered Json deserializer can now read `[SerializeReferences]`.
- QuantumHud window displays onboarding information and installs Quantum user scripts and assets.
- Creating Quantum system instances can now be done in a data-driven way by adding a `SystemsConfig.asset` to the `RuntimeConfig`.
- The multiclient script now can add local players without creating a new client connection.
- Added `AllowedLobbyProperties` dashboard variable to restrict lobby property usage for Photon matchmaking.
- Added `Quantum.Log.dll` dependency, which introduces the `Quantum.LogType` and can clash with `UnityEngine.LogType` when migrating.
- Quantum debug dlls can be toggled in the BuildFeatures section on the `QuantumEditorSetting` inspector and in the Quantum menu.

**Changes**

- Unified Quantum CodeGens into one tool.
- Unified QuantumRunner and SessionContainer into the `SessionRunner` class and moved it to the `QuantumGame` project.
- Component prototype suffix has been changed from `_Prototype` to `Prototype`.
- Component prototype wrapper prefix changed from `EntityComponent` to `QPrototype` (e.g. `EntityComponentTransform2D` -> `QPrototypeTransform2D`).
- `Frame.Assets` is now obsolete. Use `Frame.FindAsset<T>` instead.
- `AssetObjectConfigAttribute` is obsoleted and no longer functional.
- `AssetObject` is no longer restricted to being in the same assembly as Quantum simulation code. If the simulation code needs to access such assets, it can either use a base class or an interface. This allows `AssetObjects` to be extended with any Unity-specific data.
- `AssetRefT` generated types are now obsolete, use `AssetRef<T>` instead.
- `asset T;` and `import asset T;` are no longer needed in .qtn files. Any type derived from `AssetObject` is already a fully functional Quantum asset now and can be used with `AssetRef<T>` fields.
- `AssetObject.Guid` is now deterministic by default, based on Unity's GUID and `fileId`. When needed can be overridden (e.g. for assets imported from Quantum 2.1) - such overrides are stored in `QuantumEditorSettings`. Turning `AssetGuids` deterministic speeds up `QuantumUnityDB` by an order of magnitude.
- `UnityDB` has been obsoleted and `AssetResourceContainer` has been removed. `QuantumUnityDB` replaced both and uses more consistent method naming.
- Prototype assets for new prefabs are created as standalone assets, with `EntityPrototype` suffix and `qprototype` extension. This fully decouples loading prototypes during simulation from their source prefab, improving asset load times and avoiding deadlock issues with Unity's job system.
- All Quantum assets located in asset search paths are now marked with `QuantumAsset` label.
- Moved NavMesh baking to the `Quantum Simulation` project to bake parts deterministically (not the Unity navmesh export).
- Removed the `MovementType.DynamicBody` and `MovementType.CharacterController2D` from the NavmeshAgentConfig. Select `MovementType.Callback` instead and perform movement during the `ISignalOnNavMeshMoveAgent` callback. The removed options were only good for prototyping and led to a lot of questions.
- Changed `QuantumNavMesh.BakeData` and related data structures to only use fixed point vectors.
- Replaced non-deterministic code (Triangle Normal Computation) in `BakeNavMesh()` with fixed point math.
- Moved navmesh link detection and error correction from `BakeNavMesh()` to `ImportFromUnity()`, `StartTriangle` and `EndTriangle` now are set on `BakeData.Links`.
- Increased max navmesh region count to 128
- The MainArea of the navmesh now has its own valid navmesh region (always index 0) and can be toggled on/off and can be properly used for navmesh queries like `LineOfSight()` or `FindClosestTriangle()`
- Changed `BakeNavMesh` signature to only use relevant data types.
- Replaced the signal `OnPlayerDataSet(PlayerRef player)` with `OnPlayerAdded(PlayerRef player, bool firstTime)` where `firstTime` signals this is the first time that this player was assigned..
- Added `OnPlayerRemoved(PlayerRef player)` signal.
- Added a parameter to the `GameStarted` callback (`isResync`: true, when the callback is invoked after the game has been re-synced for example after a late-join).
- Removed the `ReleaseProfiler` configuration from the code solutions and replacing it with a flag in `StartParameters.GameFlags` called `QuantumGameFlags.EnableTaskProfiler`.
- Removed API that was deprecated in Quantum 2.1.
- All Quantum Unity MonoBehaviours now derive from `QuantumMonoBehaviour`.
- All Quantum Unity ScriptableObjects now derive from `QuantumScriptableObject`.
- The state inspector now shows all relevant configs when selecting the Runner.
- Restructured the Quantum Unity menus.
- `Navigation.Raycast2D()` provides information about the border index that generated the closest hit.

## Preview

### Build 1393 (Mar 21, 2024)

**Changes**

- Logging a more precise error on session start time outs

**Bug Fixes**

- Fixed: Desync caused by serialization of the PhysicsQueryRef

### Build 1390 (Mar 19, 2024)

**What's New**

- Quantum runtime logging can now be toggled by a `LogLevel` inside the `EditorSettings`, the default is `WARN`
- A tool to quickly convert Unity colliders to Quantum colliders `GameObject/Quantum/Convert Colliders`
- Create `SystemSignalsOnly` template

**Bug Fixes**

- Fixed: Warnings in `QuantumAssetObjectSourceStatic.AssetType` and `QuantumAssetObjectSourceStaticLazy.AssetType`

### Build 1388 (Mar 16, 2024)

**Changes**

- Removing code from the navmesh baker that was needed when the MainArea was not a region
- `Map.SerializeTrianglesMetadata` is now obsolete and triangle metadata is never serialized

**Bug Fixes**

- Fixed: Draw capsule function to use the correct scale
- Fixed: Static capsule 3D gizmo drawning to use the correct scale
- Fixed: Correctly enabling `QUANTUM_ENABLE_TEXTMESHPRO` for com.unity.ugui version 2.0.0 and up
- Fixed: An issue on the server that cause input to be not accepted for late-joines for a while when lots of commands are issued by other clients

### Build 1383 (Mar 14, 2024)

**What's New**

- `FrameThreadSafe.GetGlobal()` extension method
- Obsolete methods imitating legacy serialization API: `Serialize/DeserializeAssets/Replay/Checksum`
- `Compress` and `Decompress` editor buttons for `BinaryData`
- `FP.AsDoubleRounded` and `FP.FromRoundedDouble_UNSAFE` - these variants do not convert to exact values, but rather ones with the least significant digits, within FP's precision range
- `FP.FromDouble_UNSAFE`

**Changes**

- `QuantumAssetSourceStatic.Prefab` -> `QuantumAssetSourceStatic.Object`

**Removed**

- `QuantumEditorSettings.FPDisplayPrecision` - no longer needed, FPs behave in same way as doubles

**Bug Fixes**

- Fixed: `BinaryDataAttribute` being ignored when using Odin
- Fixed: An issue with the physics layer matrix editor tool in the `SimulationConfig.Physics` inspector
- Fixed: Compile errors for 2022.2 related to `FindObjectSortMode`
- Fixed: `SimulationConfig's `Import Layers` buttons not marking asset as dirty

### Build 1378 (Mar 13, 2024)

**What's New**

- FPVector3.XYO, .XOZ and .OYZ swizzle properties

### Build 1372 (Mar 12, 2024)

**Changes**

- `FP.ToString` improvements: each FP value emits a unique string and is rounded towards shortest value within the rounding error
- Introducing `QUANTUM_ENABLE_AI_NAVIGATION` to toggle Unity navigation addon related code

**Bug Fixes**

- Fixed: An issues that caused the debug draw shapes from the simulation not being displayed when using the URP
- Fixed: Exception when trying to obtain `GetDrawerTypeForType` MethodInfo
- Fixed: Using NamedBuildTarget accross the board
- Fixed: Occasional exception in loading of LUT in the editor

### Build 1369 (Mar 08, 2024)

**Bug Fixes**

- Fixed: QuantumUnityDB not available resulting in a flood of error messages when inspecting `AssetObject`

### Build 1368 (Mar 07, 2024)

**What's New**

- Photon room information on the `QuantumRunner` inspector

**Bug Fixes**

- Fixed: Potential deadlocks when the simulation is using all Job threads and an asset which is sync-loaded triggers an async mesh/texture load
- Fixed: An issue in the Quantum menu that could result in not being able to start a game when the loading a fallback map failed with an exception

### Build 1366 (Mar 06, 2024)

**What's New**

- The context `LayerInfo` is now exposed in `FrameThreadSafe.Layers` property, similarly to `Frame.Layers`

**Changes**

- Setting the default `DeltaTimeType` used in the menu to `EngineDeltaTime`

### Build 1365 (Mar 05, 2024)

**What's New**

- The QuantumStart room property can be controlled by the server using dashboard variables or webhook configuration, check out the CreateGame dashboard configuration or webhook documents for more information

**Changes**

- Wrapping event dispatching callbacks into HostProfiler markers

**Removed**

- `SessionConfig` and `SessionConfigPlayerCountIsVariable` dashboard configs, please use webhooks instead

### Build 1364 (Mar 04, 2024)

**Breaking Changes**

- `IAssetSerializer` interface changed, it also deals with `RuntimePlayer` and `RuntimeConfig` serialization
- `QuantumUnityJsonSerializer` output format changed; asset data is no longer stored as a JSON-escaped string

**What's New**

- PlayerRef now implements IEquatable<PlayerRef>
- `QuantumUnityJsonSerializer.NullifyUnityObjectReferences` - enabling will serialize all Unity references as JSON-null. (true by default)
- `QuantumUnityJsonSerializer.EntityViewPrefabResolver` - a custom handler for resolving entity view prefabs
- Added a few new markers for the Unity profiler
- Added another simulation debug draw shape `Draw.Capsule`

**Changes**

- Solid colored simulation debug shapes now use the depth buffer when rendering, replace the default materials setting `DebugMesh.DebugMaterial` and `DebugMesh.DebugSolidMaterial`

**Bug Fixes**

- Fixed: Overflow in internal physics capacity fields when going above 64K entries in triangles, manifolds and other types
- Fixed: Compile error (collection literals not supported in out TC setup)
- Fixed: Global objects no loading in a build due to assembly attributes being stripped
- Fixed: Compile error in `QPrototypePhysicsCollider2D` when both 2D & 3D physics are disabled
- Fixed: An issue that inverted the meaning of `QuantumEntityViewFlags.DisableSearchChildrenForEntityViewComponents`
- Fixed: Making simulation debug draw consistently drawing in debug builds

### Build 1356 (Feb 27, 2024)

**Changes**

- The `RuntimePlayer` list on the QuantumMenu connection args is now an array and it initializes itself with one entry by default, this way freshly installed menu scenes have one `RuntimePlayer` that is added automatically and the list now can be empty if players get added differently to the running game

### Build 1355 (Feb 24, 2024)

**What's New**

- Running the CodeGen now throws an error when a UnityEditor restart is needed after a package upgrade
- `QuantumSceneViewComponent` can be used to create standalone (without an Entity) view component scripts that get updated by the EntityViewUpdater, to create simple view scripts that have access to the simulation

**Changes**

- Navmesh serialization logs the name and guid on errors

### Build 1354 (Feb 23, 2024)

**Bug Fixes**

- Fixed: An issue that could cause false navmeshborders being generated by introducing `QuantumNavMesh.ImportSettings.FixTrianglesOnEdgesHeightEpsilon`

### Build 1353 (Feb 22, 2024)

**Changes**

- CodeGen: All extension methods for flags are now grouped under `Quantum.FlagsExtensions` class
- CodeGen: `ComponentTypeIdGen`, `StaticDelegates` and `TypeRegistry` merged into a single `Statics` class

**Bug Fixes**

- Fixed: CodeGen: using `AllocateOnComponentAdded` attribute resulting in a compile warning
- Fixed: An issue that caused the simualtion to end with an exception when the delta input cache is full (e.g. breakpoint for 30+ sec), now a PluginDisconnect callback is triggered with `ERROR #41` and the simualtion is destroyed
- Fixed: An issue that could cause navmesh border normals to be flipped

### Build 1352 (Feb 21, 2024)

**Changes**

- Frame.AllocateCollection (List, HashSet and Dictionary) overloads that receive a copy of the collection Ptr are now obsolete. Use either the overload that does not receive a Ptr or the one that does so as an 'out' parameter

**Bug Fixes**

- Fixed: Missing script on an asset preventing `QuantumUnityDB` from being generated
- Fixed: The sleep detection when the flag IsAwakenedByForces is enabled in 3D simulation for the Legacy Gauss colision solver
- Fixed: The sleep detection when the flag IsAwakenedByForces is enabled in 2D simulation for the collision solver Hybrid Jacobi and Legacy Gauss
- Fixed: Migration: `QUANTUM_ENABLE_MIGRATION` being defined ignoring `QUANTUM_ENABLE_MIGRATION_Q3PREVIEW`

### Build 1350 (Feb 17, 2024)

**What's New**

- `QuantumUnityDB.Global` can be set manually now. This enables async/Addressables workflows

**Changes**

- Global ScriptableObjects are by default instantiated from a loaded Resource, if requested during Play mode in Editor
- `QuantumGlobalScriptableObjectLoaderMethodAttribute` is now obsolete. Use `QuantumGlobalScriptableObjectAddressAttribute` or `QuantumGlobalScriptableObjectResourceAttribute` to point to custom a runtime location or use the global object's setter instead

**Bug Fixes**

- Fixed: Adding/removing sources from `QuantumUnityDB.Global` in Playmode no longer modifies the asset itself by default
- Fixed: `QuantumUnityDB` is loaded from Resources (rather than AssetDatabase) in Playmode

### Build 1349 (Feb 16, 2024)

**Bug Fixes**

- Fixed: Serialization issue in FrameMetaData that could cause a soft desync when expanding internal hash collections

### Build 1348 (Feb 15, 2024)

**What's New**

- `QuantumUnityDB.RemoveSource`

### Build 1347 (Feb 14, 2024)

**Changes**

- `QuantumUnityDB.DisposeAsset` only returns false if an asset does not exist

**Bug Fixes**

- Fixed: Making the menus current scene saved in playerprefs more robust against changes in the menu config

### Build 1346 (Feb 13, 2024)

**Bug Fixes**

- Fixed: Assets left in `Disposing` state when unloading with the Quantum Unity DB Inspector

### Build 1344 (Feb 12, 2024)

**Breaking Changes**

- Delete the folder `Assets/Photon/Quantum/Assemblies/Dotnet` before or after upgrading. We had to change the way how to handle non-Unity Quantum dependencies inside the unitypackage and placed them into a zip folder at `Assets/Photon/Quantum/Editor/Dotnet`. Although properly excluded the unarchived dlls were causing random issues with builds

**Bug Fixes**

- Fixed: Retaining `Quantum-Menu.unitypackage` meta file between upgrades

### Build 1342 (Feb 09, 2024)

**Changes**

- Replaced `QuantumEntityView.GameObjectNameIsEntityRef` with the view flag `DisableEntityRefNaming`
- Loading entity view components now also searches game object children, disable with the view flag `DisableSearchChildrenForEntityViewComponents`
- ViewContexts are also searched in children of the entity view updater game object

**Bug Fixes**

- Fixed: `QuantumEntityView.SetViewFlag()` was missing a way to unset flags
- Fixed: An issue on the server that caused the simulation to start prematurely on the server when using the CreateGame webhook

### Build 1340 (Feb 08, 2024)

**What's New**

- CodeGen: `import component` and `import singleton component` support. This directive registers an existing component without emitting the type itself, so components can now be defined outside of a QTN file. When defining a component type, these are the requirements:  
- implements `IComponent` interface  
- has `const int SIZE`  
- has `public static void Serialize(void* ptr, FrameSerializer serializer)`  
- has `ComponentChangedDelegate OnAdded` static property (may return null) or static `OnAdded` method matching the delegate signature.  
- has `ComponentChangedDelegate OnRemoved` static property (may return null) or static `OnRemoved` method matching the delegate signature
- AssetScript template under the create asset menu
- Qtn file icon

**Changes**

- Generated `AllocatePointers` and `ClearPointers` first parameter changed from `Frame` to `FrameBase`

### Build 1339 (Feb 07, 2024)

**Changes**

- `QuantumEntityView` now uses `InlineHelp` instead of duplicating the comments for tooltips

### Build 1338 (Feb 06, 2024)

**What's New**

- `Assets/Create/Quantum/Asset...` menu item

**Changes**

- Improving the Quantum asset menu sorting

**Bug Fixes**

- Fixed: An issue on predicted commands that could cause a desync by executing the same command on different verified frames
- Fixed: An issue that caused changes to the navmesh settings on the Quantum map (e.g. GridSize) not be baked on to the Quantum navmesh

### Build 1335 (Feb 03, 2024)

**Changes**

- Physics Materials' Friction coefficients are no longer clamped between 0-1, but only > 0. Restitution is now clamped between 0-1

**Bug Fixes**

- Fixed: Unable to find style 'FloatFieldLinkButton' warning when installing the QuantumMenu

### Build 1334 (Feb 02, 2024)

**Breaking Changes**

- When upgrading a Quantum 3 preview project use the online migration notes `https://doc.photonengine.com/quantum/v3/getting-started/migration-guide-preview`
- Built-in component prototypes are a part of the SDK now and Unity script guids of `QPrototypeTransform2D` etc. have changed, use the scripting define `QUANTUM_ENABLE_MIGRATION_Q3PREVIEW` as mentioned on the migration notes when upgrading
- Moving all QuantumMenu Unity scripts and assets to a separate unitypackage `Assets/Photon/QuantumMenu/Quantum-Menu.unitypackage` and added assembly definitions, this way the menu can be easily removed from the SDK if desired, to migrate import the unitypackage manually
- Renamed `Game` and `Runtime` folders inside `Assets/QuantumUser/` to `Simulation` and `View` to better represent which library its content belongs (Quantum.Simulation.dll and Quantum.Unity.dll respectively), to migrate change the code-generation destination folders in `QuantumCodeGenSettings.User.cs` as described in the migration notes
- Renamed the final game assembly from `Quantum.Game.dll` to `Quantum.Simulation.dll`, to migrate open all `SystemsConfig` assets with a text editor and replace `AssemblyQualifiedName:.. Quantum.Game` with `AssemblyQualifiedName:.. Quantum.Simulation` and rebake all maps
- Collections used in events are now passed by their appropriate pointer type (e.g. `list<T>` -> `QListPtr<T>`). Was: `Ptr`
- The default backing type for `flags` in DSL is now Int32 (was: Int64)

**What's New**

- Adding entity view pooling, add `QuantumEntityViewPool` to the entity view updater game object
- Introducing `EntityViewComponent`s, a quick way to add view logic to the Quantum entity views
- Use the `IQuantumEntityViewContext` to quickly inject data into the view components
- Standalone Quantum dll build tools using the `QuantumStandaloneBuildSettings` and `QuantumStandaloneProjectSettings` scriptable objects
- CodeGen: Support for casting in the attribute list. E.g.: `[DrawIf("SomeField", (long)SomeEnum.SomeValue)] Int32 IntField;`
- CodeGen: Support for `AssetRef` and `asset_ref` types without angle brackets
- `FramePrinter.IPrintable` - if a struct/component implements this interface, `FramePrinter` will use it to do the printing instead of the default reflection-based printing
- Support for QUANTUM_ENABLE_MIGRATION_Q3PREVIEW define
- `QuantumUnityJsonSerializer.IntegerEnquotingEnabled` and `QuantumUnityJsonSerializer.IntegerEnquotingMinDigits` - helps with the cases where resulting JSON is processed by a serializer that treats all numbers as floating points, resulting in loss of precision for long-backed Guids etc
- `QuantumUnityJsonSerializer.EntityViewPrefabResolvingEnabled`

**Changes**

- Upgrading Photon Realtime to version 5.0.2
- The default location for Quantum code generated script files is not `QuantumUser/View/Generated` (was `Runtime/Generated`)
- Collections pointer are readonly structs now
- The menu can now be started and used without scenes added to the MenuConfig, it will use the first Quantum scene that can be found
- Renamed `JsonAssetSerializerBase.IsPrettyPrintEnabled` to `PrettyPrintEnabled`
- Added `[InitializeOnLoadMethod]` to `QuantumUnityLogger.Initialize`
- Assertion in QuantumUnityDB.DisposeEntry logs more info now
- Moving the `Quantum` menu to `Tools/Quantum` to comply with AssetStore guidelines

**Removed**

- The game sample scene from the SDK, an initial test scene is generated by the Hub during installation and new Quantum game scenes can be generated by the Create Asset menu
- Simple connection scene, which now can be generated by the Hub
- `PathUtils.MakeRelativePath()`, use `PathUtils.TryMakeRelativeToFolder()` or even better `Path.GetRelativePath()`
- `ReflectionUtils.GetShortAssemblyQualifiedName()`, use `SerializableType.GetShortAssemblyQualifiedName()` instead
- `QuantumUnityJsonSerializer.SerializeEntityViewPrefabInstanceId`

**Bug Fixes**

- Fixed: `LayerMatrixGUI` error on latest Unity 2023.2
- Fixed: The gizmos in 2d simulation when a capsule shape is added to the scene
- Fixed: CodeGen: possible incorrect recognition of `QUANTUM_ENABLE_MIGRATION` define
- Fixed: Globals auto init/clear pointers not being called automatically. `AllocatePointers` is executed before `InitUser` and `ClearPointers` is executed before `FreeUser`
- Fixed: The static collision on the broadphase in predict mode by using the static collider state to check if the meshes are enable in the frame
- Fixed: Issue in 3D Ray and Linecast Broad-Phase queries when using FirstHitOnly=false causing some hits to be missed
- Fixed: `AssertException` when saving a dirty scene, after exiting from play mode
- Fixed: `[ExcludeFromPrototype]` applied on `entity_ref` fields not restoring prototype's partial status

### Build 1307 (Jan 18, 2024)

**Bug Fixes**

- Fixed: The computation of the support offset of capsule 3D to solve issues with the CCD collision with capsules

### Build 1306 (Jan 17, 2024)

**Changes**

- Updating Quantum System templates by adding `using Photon.Deterministic;` for convenience

### Build 1303 (Jan 13, 2024)

**Bug Fixes**

- Fixed: The computation of the support offset of capsule 3D to solve issues with the CCD collision with capsules

### Build 1302 (Jan 12, 2024)

**Breaking Changes**

- Rebake all Quantum navmeshes
- `NavMeshRegionMask.HasValidRegions()` now returns `true` for the "MainArea", use `HasValidNoneMainRegion` instead to only query for non-MainArea regions

**Changes**

- The MainArea of the navmesh now has its own valid navmesh region (always index 0) and can be toggled on/off and can be properly used for navmesh queries like `LineOfSight()` or `FindClosestTriangle()`

**Bug Fixes**

- Fixed: Migration: compile errors for `UnityDB.ResourceManager`
- Fixed: Odin: warnings for `QuantumStaticEdgeCollider2DEditor` and `QuantumStaticPolygonCollider2DEditor`
- Fixed: An issue with the remote profiler that caused the Unity Editor to freeze after quitting the app

### Build 1301 (Jan 11, 2024)

**What's New**

- Quantum system script templates under `Create/Quantum/System`

**Bug Fixes**

- Fixed: Warning about `StaticEditorFlags.NavigationStatic` for Unity Editor 2022 and newer

### Build 1299 (Jan 10, 2024)

**Breaking Changes**

- Rebake all navmeshes and run the code generation

**Changes**

- Increased max navmesh region count to 128

**Bug Fixes**

- Fixed: `ComponentSet.IsSubsetOf` not computing the returned value correctly

### Build 1298 (Jan 06, 2024)

**Bug Fixes**

- Fixed: Unity crashing when using an undefined type in QTN

### Build 1297 (Jan 05, 2024)

**What's New**

- `IResourceManager` query capabilities
- `ResourceManagerStatic.AddAssets`

**Changes**

- `QuantumUnityDB` is not a `IResourceManager` itself, cutting down some asset management complexities
- `ResoureManagerStatic` treats assets as not loaded by default

**Bug Fixes**

- Fixed: CompareExportedAndUnityDB losing track of the scene it was supposed to delete

### Build 1296 (Jan 04, 2024)

**Bug Fixes**

- Fixed: Typo in GUID Override Warning dialog

### Build 1294 (Dec 22, 2023)

**Bug Fixes**

- Fixed: Trigger and query collisions between two 2D or 3D capsules  
not being correctly detected
- Fixed: Solve the CCD issues on capsule colliders 3D and 2D
- Fixed: Adding a camera to the demo menu scene to prevent the warning overlay for missing camera

### Build 1293 (Dec 21, 2023)

**What's New**

- `QuantumUnityJsonSerializer.SerializeEntityViewPrefabInstanceId`
- `QList.Sort()` and `QList.Reverse()` methods

**Bug Fixes**

- Fixed: `EntityView` serializing `Prefab` reference as `instanceId` when exporting the DB using `QuantumUnityJsonSerializer`
- Fixed: `EntityView.Prefab` being null after a deserialization using `QuantumUnityJsonSerializer`, when the DB was created with another process. When `EntityView` is deserialized, its `Prefab` field is resolved with `QuantumUnityDB.Global`

### Build 1292 (Dec 20, 2023)

**Bug Fixes**

- Fixed: An issue with delta compression in local mode `SessionConfig.OffsetMin` (input deply) was greater then `0`

### Build 1288 (Dec 16, 2023)

**What's New**

- Odin: dedicated drawer for `DictionaryEntry`
- `UnionPrototype` Odin-dedicated drawer

**Bug Fixes**

- Fixed: Odin: errors when inspecting fields with `OptionalAttribute` or `DictionaryAttribute`
- Fixed: CodeGen: Type not found exception for QString when used as dictionary key but not defined anywhere else on the DSL
- Fixed: An issue that caused the command prediction to fail under higher pings

### Build 1287 (Dec 15, 2023)

- Upgraded to Photon Realtime 5.0.1

**Breaking Changes**

- Delete the folder `Assets/Photon/PhotonRealtime` before importing the new Unity package. Unfortunately we had to do last minute changes to a few meta files of the new Photon Realtime scripts: `change-realtime.txt`, `ConnectionHandler.cs` and `CustomTypesUnity.cs`

**What's New**

- Support to 2D Capsule shape, including queries and on dynamic and static colliders

### Build 1285 (Dec 13, 2023)

**What's New**

- CodeGen: support for AssetRef<T> (as an alternative to asset_ref<T>) in QTN

**Bug Fixes**

- Fixed: `UnionPrototype` drawers throwing an exception
- Fixed: `StructPrototype` and `UnionPrototype` types not having `[Quantum.Prototypes.Prototype]` attribute
- Fixed: An issues that caused the menu to not use the selected preferred region

### Build 1284 (Dec 12, 2023)

**What's New**

- AssetRef.IsValid

**Bug Fixes**

- Fixed: `AssetRef<>.Equals` always returning false

### Build 1283 (Dec 11, 2023)

**What's New**

- Adding meaningful assertion methods when using misconfigured navmesh agents
- When exporting a SessionConfig to Json the max possible player count is set instead of 0

**Changes**

- Renaming `Room Wait Time` setting on the `SessionConfig` inspector to resemble more the property name `Session Start Wait Time` and correct the code doc summary

**Bug Fixes**

- Fixed: Toggling prototype's AssetGuid override removing all prototypes from the DB, until the next reimport
- Fixed: `Assert` when installing Quantum for the first time
- Fixed: `QuantumEditorSettings` clearing guid overrides for prototypes on full reimport
- Fixed: Migration: keep GenericAssets.cs when running CodeGen

### Build 1275 (Nov 29, 2023)

**What's New**

- `QuantumUnityDBUtilities.SetQuantumUnityDBRefreshMode`
- QuantumUnityDBUtilities.RefreshGlobalDB has `force` parameter

**Changes**

- `Frame.Create(AssetRef<EntityPrototype> prototype)` throws `ArgumentOutOfRangeException` in case it fails to find the prototype
- If the asset postprocessor detects changes in `AssetObjects`, `QuantumUnityDB` gets refreshed in `EditorApplication.delayCall`
- Tests: Added AssetPostprocessor performance tests

**Bug Fixes**

- Fixed: Compiler warnings when no events or buttons are defined in QTN
- Fixed: Restored original GUIDs of some test assets (broken after 596d7256d85ca456b9465f7d110a2a45363ebf07)
- Fixed: An issue that cause the AppVersion to be saved on the `PhotonServerSettings` when using the sample connection scene
- Fixed: An issue that caused the command prediction to not work properly
- Fixed: Occasional assert in `QuantumAssetSourceAddressable.UnloadInternal`
- Fixed: An issue that caused input being polled incorrectly
- Fixed: An issue that failed to add multiple runtime players added to the menu game object
- Fixed: An issue on the plugin that failed to add players when adding multiple local players at the same time
- Fixed: `SessionRunner.Arguments.StartGameTimeoutInSeconds` to be a nullable and default to `SessionRunner.Arguments.DefaultStartGameTimeoutInSeconds` which is set to 10 seconds
- Fixed: QuantumUnityDB not refreshing on incoming new assets

### Build 1268 (Nov 27, 2023)

**Breaking Changes**

- The files inside Assets/Photon/Quantum/Game/Core have been moved, delete this folder by hand before upgrading the Quantum 3 SDK

**What's New**

- CodeGen: Prototype adapters now have `partial void ConvertUser` method if user needs to include their own convert logic

**Changes**

- `QuantumUnityDBImporter` does not stop processing assets after an asset causes an error
- Package: Quantum/Game/Core folder is gone, contents moved to Quantum/Game
- Package: Quantum/Game/Core/Core.cs moved to Quantum/Game/QuantumGameCore.cs
- Migration: TransferAssetBaseGuidsToAssetObjects won't perform any destructive operation unless each and every `AssetBase` and `AssetObject` scripts are ready for the operation
- Migration: CodeGen will run on .qtn changes during migration, with the same options as initial CodeGen
- Package: Hierarchy of `Quantum/Editor` folder simplified

**Bug Fixes**

- Fixed: RealtimeAppIds are still shown in the PhotonServerSettings inspector when not null
- Fixed: An issue that caused the simulation to assert with a unrelated error after causing an error when creating systems with the SystemsConfig asset
- Fixed: Legacy `AssetRefT` not being registered in MemoryLayoutVerifier
- Fixed: `[Flags]` attribute usage in qtn without being fully qualified
- Fixed: Migration: Addressable sections not checking for a legacy define `QUANTUM_ADDRESSABLES`
- Fixed: Compilation errors with disabled Unity ai module `com.unity.modules.ai`
- Fixed: Addressables did not build with migration enabled due to missing `#if UNITY_EDITOR`

### Build 1260 (Nov 09, 2023)

- Initial preview release

