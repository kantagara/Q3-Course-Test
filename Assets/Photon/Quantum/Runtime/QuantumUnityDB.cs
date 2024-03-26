namespace Quantum {
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Threading;
  using JetBrains.Annotations;
  using Photon.Deterministic;
  using Profiling;
  using Task;
  using UnityEngine;
  using UnityEngine.Serialization;
#if ODIN_INSPECTOR && !QUANTUM_ODIN_DISABLED
  using Sirenix.OdinInspector;
#endif

  [QuantumGlobalScriptableObject(DefaultPath)]
  public partial class QuantumUnityDB : QuantumGlobalScriptableObject<QuantumUnityDB>, IResourceManager {
    /// <summary>
    /// The default location of the DB asset.
    /// </summary>
    public const string DefaultPath = "Assets/QuantumUser/Resources/QuantumUnityDB.qunitydb";
    public const char NestedPathSeparator = '|';

    /// <summary>
    /// All the assets that are managed by this DB.
    /// </summary>
#if ODIN_INSPECTOR && !QUANTUM_ODIN_DISABLED
    [ListDrawerSettings(ListElementLabelName = "Path")]
#endif
    [FormerlySerializedAs("Sources")]
    [SerializeField]
    private List<Entry> _entries = new();
    
    /// <summary>
    /// AssetGuid to index in <see cref="_entries"/> mapping.
    /// </summary>
    private readonly Dictionary<AssetGuid, int> _guidToIndex = new();

    /// <summary>
    /// Path to index in <see cref="_entries"/> mapping.
    /// </summary>
    private readonly Dictionary<string, int> _pathToIndex = new();
    
    /// <summary>
    /// Allocator used for assets initialization and disposal.
    /// </summary>
    private readonly Native.Allocator _allocator = new QuantumUnityNativeAllocator();

    /// <summary>
    /// Assets are disposed on the main thread, but the disposal is scheduled from the worker threads.
    /// </summary>
    private readonly ConcurrentQueue<Entry> _disposeQueue = new();

    /// <summary>
    /// When loading assets on the main thread, the loading is scheduled from the worker threads.
    /// </summary>
    private readonly ConcurrentQueue<(AssetGuid, bool)> _workedThreadLoadQueue = new();
    
    /// <summary>
    /// Actual loading is done on the main thread, but the loading is scheduled from the worker threads.
    /// </summary>
    private int _mainThreadId;
    
    /// <summary>
    /// Exposes the list of entries in the DB. Can be used to iterate asset sources at both runtime and edit time.
    /// </summary>
    public IReadOnlyList<Entry> Entries => _entries;
    
    /// <summary>
    /// Raised when an asset is unloaded.
    /// </summary>
    public event AssetObjectDisposingDelegate AssetObjectDisposing;

    #region Unity Messages
    
    private void OnEnable() {
      FPMathUtils.LoadLookupTables();
      Native.Utils ??= new QuantumUnityNativeUtility();

      _mainThreadId = Thread.CurrentThread.ManagedThreadId;
      _guidToIndex.Clear();
      _pathToIndex.Clear();
      for (var i = 0; i < _entries.Count; ++i) {
        if (_entries[i] == null) {
          // removed, slot not used
          continue;
        } 
        AddSourceMapping(i, _entries[i].Guid, _entries[i].Path);
      }
    }

    private void OnDisable() {
      ((IDisposable)this).Dispose();
    }
    
    #endregion
    
    /// <summary>
    /// Updates <see cref="Global"/> DB, if loaded. Must be called from the main thread. Call periodically, if assets are
    /// loaded/unloaded without the simulation running.
    /// </summary>
    public static void UpdateGlobal() {
      if (!IsGlobalLoadedInternal) {
        return;
      }

      var global = Global;
      global.ProcessLoadQueue();
      global.ProcessDespawnQueue();
    }

    /// <summary>
    /// Unloads <see cref="Global" />, if already loaded and unloads any asset that has been loaded.
    /// Next call to <see cref="Global"/> will load the DB again.
    /// </summary>
    public static bool UnloadGlobal() {
      return UnloadGlobalInternal();
    }

    /// <summary>
    /// Unloads all the assets that have been loaded by the <see cref="Global"/> DB.
    /// </summary>
    /// <param name="destroyed"></param>
    protected override void OnUnloadedAsGlobal(bool destroyed) {
      if (!destroyed) {
        // TODO: is this check necessary?
        DisposeAllAssetsImmediate();
      }
    }
    
    /// <summary>
    /// Registers a source for the asset with the given <paramref name="guid"/> and an optional <paramref name="path"/>.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="guid"></param>
    /// <param name="path"></param>
    public void AddSource(IQuantumAssetObjectSource source, AssetGuid guid, string path = null) {
      Assert.Check(source);
      Assert.Check(guid.IsValid);

      var entry = new Entry { Guid = guid, Source = source, Path = path };
      
      // are there any free slots?
      if (_entries.Count == _guidToIndex.Count) {
        // nope
        AddSourceMapping(_entries.Count, guid, path);  
        _entries.Add(entry);
      } else {
        // yes, find first free slot
        var index = _entries.FindIndex(e => e == null);
        Assert.Check(index >= 0);
        AddSourceMapping(index, guid, path);
        _entries[index] = entry;
      }
    }

    /// <summary>
    /// Removes the source for the asset with the given <paramref name="guid"/>.
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool RemoveSource(AssetGuid guid, out (IQuantumAssetObjectSource source, bool isAcquired) result) {
      if (!_guidToIndex.TryGetValue(guid, out var index)) {
        result = default;
        return false;
      }
      
      var entry = _entries[index];
      Assert.Check(entry != null);
      _entries[index] = null;
      
      result = (entry.Source, entry.State.Value >= EntryState.LoadingAsync);
      
      _guidToIndex.Remove(guid);
      if (!string.IsNullOrEmpty(entry.Path)) {
        _pathToIndex.Remove(entry.Path);
      }

      return true;
    }

    public bool RemoveSource(AssetGuid guid) {
      return RemoveSource(guid, out _);
    }
    
    private void AddSourceMapping(int index, AssetGuid guid, string path) {
      if (_guidToIndex.TryGetValue(guid, out var existingIndex)) {
        throw new ArgumentException($"Entry with {guid} already exists: {_entries[existingIndex]}", nameof(guid));
      }

      if (!string.IsNullOrEmpty(path)) {
        if (_pathToIndex.TryGetValue(path, out existingIndex)) {
          throw new ArgumentException($"Entry with {path} already exists: {_entries[existingIndex]}", nameof(path));
        }
      }

      _guidToIndex.Add(guid, index);

      if (!string.IsNullOrEmpty(path)) {
        _pathToIndex.Add(path, index);
      }
    }
    
    #region Global API

    /// <summary>
    /// Returns the global DB. If the DB is not loaded, it will be loaded.
    /// </summary>
    public new static QuantumUnityDB Global {
      get => QuantumGlobalScriptableObject<QuantumUnityDB>.Global;
      set => QuantumGlobalScriptableObject<QuantumUnityDB>.Global = value;
    }


    public static bool DisposeGlobalAsset(AssetGuid assetGuid, bool immediate = false) {
      if (!IsGlobalLoadedInternal) {
        return false;
      }

      return Global.DisposeAsset(assetGuid, immediate);
    }
    
    public static AssetGuid FindGlobalAssetGuid(AssetObjectQuery query) {
      return Global.FindAssetGuid(query);
    }
    
    public static void FindGlobalAssetGuids(AssetObjectQuery query, List<AssetGuid> result) {
      Global.FindAssetGuids(query, result);
    }
    

    public static List<AssetGuid> FindGlobalAssetGuids(AssetObjectQuery query) {
      return Global.FindAssetGuids(query);
    }
    

    public static IQuantumAssetObjectSource GetGlobalAssetSource(AssetGuid assetGuid) {
      return Global.GetAssetSource(assetGuid);
    }
    
  
    public static IQuantumAssetObjectSource GetGlobalAssetSource(string assetPath) {
      return Global.GetAssetSource(assetPath);
    }
    

    public static string GetGlobalAssetPath(AssetGuid assetGuid) {
      return Global.GetAssetPath(assetGuid);
    }
    

    public static AssetGuid GetGlobalAssetGuid(string path) {
      return Global.GetAssetGuid(path);
    }
    

    public static AssetObjectState GetGlobalAssetState(AssetGuid guid) {
      return Global.GetAssetState(guid);
    }
    

    public static Type GetGlobalAssetType(AssetGuid guid) {
      return Global.GetAssetType(guid);
    }


    public static AssetObject GetGlobalAsset(AssetRef assetRef) {
      return Global.GetAsset(assetRef.Id);
    }

    
    public static T GetGlobalAsset<T>(AssetRef<T> assetRef) where T : AssetObject {
      return Global.GetAsset(assetRef.Id) as T;
    }

    public static AssetObject GetGlobalAsset(string assetPath) {
      return Global.GetAsset(assetPath);
    }

    public static bool TryGetGlobalAsset<T>(AssetGuid assetGuid, out T result)
      where T : AssetObject {
      return Global.TryGetAsset(new AssetRef(assetGuid), out result);
    }

    public static bool TryGetGlobalAsset<T>(AssetRef assetRef, out T result)
      where T : AssetObject {
      return Global.TryGetAsset(assetRef, out result);
    }

    public static bool TryGetGlobalAsset<T>(AssetRef<T> assetRef, out T result)
      where T : AssetObject {
      return Global.TryGetAsset(assetRef, out result);
    }

    public static bool TryGetGlobalAsset<T>(string assetPath, out T result)
      where T : AssetObject {
      return Global.TryGetAsset(assetPath, out result);
    }
    
    #endregion

    #region Asset API

    private static AssetObjectState ToAssetObjectState(EntryState state) {
      return state switch {
        EntryState.NotLoaded => AssetObjectState.NotLoaded,
        EntryState.Error => AssetObjectState.Error,
        EntryState.LoadingAsyncEnqueued => AssetObjectState.Loading,
        EntryState.LoadingSyncEnqueued => AssetObjectState.Loading,
        EntryState.LoadingAsync => AssetObjectState.Loading,
        EntryState.LoadingSync => AssetObjectState.Loading,
        EntryState.LoadedInvokingCallbacks => AssetObjectState.Loading,
        EntryState.Loaded => AssetObjectState.Loaded,
        EntryState.UnloadingEnqueued => AssetObjectState.Disposing,
        EntryState.UnloadingInvokingCallbacks => AssetObjectState.Disposing,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
      };
    }
    
    public IQuantumAssetObjectSource GetAssetSource(AssetGuid assetGuid) {
      if (_guidToIndex.TryGetValue(assetGuid, out var index)) {
        return _entries[index].Source;
      }

      return default;
    }
    
    public IQuantumAssetObjectSource GetAssetSource(string assetPath) {
      if (_pathToIndex.TryGetValue(assetPath, out var index)) {
        return _entries[index].Source;
      }

      return default;
    }
    
    /// <summary>
    /// Converts a Quantum asset path to a Quantum asset guid.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public AssetGuid GetAssetGuid(string path) {
      if (_pathToIndex.TryGetValue(path, out var index)) {
        return _entries[index].Guid;
      }

      return default;
    }
    
    public string GetAssetPath(AssetGuid assetGuid) {
      if (_guidToIndex.TryGetValue(assetGuid, out var index)) {
        return _entries[index].Path;
      }

      return string.Empty;
    }
    
    public bool DisposeAsset(AssetGuid guid, bool immediate = false) {
      if (!_guidToIndex.TryGetValue(guid, out var index)) {
        return false;
      }

      if (immediate) {
        if (_mainThreadId != Thread.CurrentThread.ManagedThreadId) {
          throw new InvalidOperationException($"Immediate disposal can only be requested from the main thread. Guid: {guid}");
        }
      }
      
      var entry = _entries[index];

      if (!entry.State.TryCompareExchange(immediate ? EntryState.UnloadingInvokingCallbacks : EntryState.UnloadingEnqueued, EntryState.Loaded)) {
        return true;
      }

      Log.TraceAssets($"Enqueuing asset {guid} for disposal.");
      AssetObjectDisposing?.Invoke(guid);

      if (immediate) {
        DisposeEntry(entry);
      } else {
        _disposeQueue.Enqueue(entry);
      }

      return true;
    }

    public void DisposeAllAssetsImmediate() {
      if (_mainThreadId != Thread.CurrentThread.ManagedThreadId) {
        throw new InvalidOperationException($"Immediate disposal can only be requested from the main thread.");
      }
      
      foreach (var entry in _entries) {
        if (entry == null) {
          // removed, slot not used
          continue;
        }
        var prevState = entry.State.Value;
        if (prevState == EntryState.NotLoaded) {
          // not loaded at all
          continue;
        }

        if (prevState >= EntryState.LoadingAsync) {
          // already acquired
          entry.State.Exchange(EntryState.UnloadingInvokingCallbacks);
          DisposeEntry(entry);
        } else {
          // no need to dispose anything
          entry.State.Exchange(EntryState.NotLoaded);
        }
      }

      _disposeQueue.Clear();
      _workedThreadLoadQueue.Clear();
    }
    
    public AssetObject GetAsset(AssetGuid guid) {
      if (!_guidToIndex.TryGetValue(guid, out var index)) {
        return null;
      }
      return GetAssetInternal(_entries[index], synchronous: true);
    }

    public bool LoadAssetAsync(AssetGuid guid) {
      if (!_guidToIndex.TryGetValue(guid, out var index)) {
        return false;
      }

      GetAssetInternal(_entries[index], synchronous: false);
      return true;
    }
    
    public bool FindNextAssetGuid(ref AssetObjectQuery query, out AssetGuid guid) {
      ref int i = ref query.ResourceManagerStateValue;
      for ( ; i < _entries.Count; ++i) {
        var entry = _entries[i];
        if (entry == null) {
          // removed, slot not used
          continue;
        }
        
        if (!query.IsMatch(ToAssetObjectState(entry.State.Value))) {
          continue;
        }
        if (!query.IsMatch(entry.Source.AssetType)) {
          continue;
        }

        ++i;
        guid = entry.Guid;
        return true;
      }
      
      guid = default;
      return false;
    }

    public AssetObjectState GetAssetState(AssetGuid guid) {
      if (!_guidToIndex.TryGetValue(guid, out var index)) {
        return AssetObjectState.NotFound;
      }

      return ToAssetObjectState(_entries[index].State.Value);
    }
    
    public Type GetAssetType(AssetGuid guid) {
      if (!_guidToIndex.TryGetValue(guid, out var index)) {
        return default;
      }

      return _entries[index].Source.AssetType;
    }

    private AssetObject GetAssetInternal(Entry entry, bool synchronous) {
      
      var guid = entry.Guid;
      
      for (;;) {
        var state = entry.State.Value;

        if (state is EntryState.Loaded or EntryState.UnloadingInvokingCallbacks) {
          // already loaded, just return
          return ExpectValidAsset(entry);
        }

        if (state is EntryState.UnloadingEnqueued) {
          // enqueued for disposal; flipping the state will effectively prevent it from being disposed and that
          // will be the end of it
          if (entry.State.TryCompareExchange(EntryState.Loaded, EntryState.UnloadingEnqueued)) {
            continue;
          }

          Log.TraceAssets($"Asset {entry.Guid} was being disposed, but is requested again. Reverting.");
          return ExpectValidAsset(entry);
        }

        Debug.Assert(state < EntryState.Loaded);
        
        if (_mainThreadId == Thread.CurrentThread.ManagedThreadId) {
          
          var targetState = synchronous ? EntryState.LoadingSync : EntryState.LoadingAsync;
          if (state >= targetState) {
            // reentry; this can happen with consecutive async then sync load or when
            // sync load happens from Loaded callback
            return entry.LoadedAsset;
          }
          
          if (!entry.State.TryCompareExchange(targetState, state)) {
            continue;
          }

          Log.TraceAssets($"Asset {guid} is being loaded: {targetState}, prev state: {state}.");

          // actually do the load
          try {
            if (state < EntryState.LoadingAsync) {
              // hasn't been acquired yet
              entry.Source.Acquire(synchronous);
              Log.TraceAssets($"Asset source for {guid} acquired.");
            }

            if (synchronous) {
              try {
                var asset = entry.Source.WaitForResult();
                Debug.Assert(asset != null);

                entry.LoadedAsset = asset;
                entry.State.Exchange(EntryState.LoadedInvokingCallbacks);

                Log.TraceAssets($"Invoking Loaded callback for {guid}.");
                asset.Loaded(this, _allocator);
                entry.State.Exchange(EntryState.Loaded);
              } catch (Exception) {
                entry.LoadedAsset = null;
                entry.Source.Release();
                throw;
              }
            }
          } catch (Exception ex) {
            Log.Exception($"Failed loading {guid}.", ex);
            entry.State.Exchange(EntryState.Error);
            throw;
          }

          if (synchronous) {
            Log.TraceAssets($"Finished loading {guid}.");
            return ExpectValidAsset(entry);
          } else {
            return entry.LoadedAsset;
          }
        } else {
          // progress the state
          var targetState = synchronous ? EntryState.LoadingSyncEnqueued : EntryState.LoadingAsyncEnqueued;
          if (state < targetState) {
            if (!entry.State.TryCompareExchange(targetState, state)) {
              continue;
            }
            
            Log.TraceAssets($"Enqueuing asset {guid} for loading on the main thread.");
            _workedThreadLoadQueue.Enqueue((guid, synchronous));
          }

          if (synchronous) {
            // wait for the asset to be loaded
            UnityEngine.Profiling.Profiler.BeginSample("Waiting On Main Thread Asset Load");
            for (;;) {
              Thread.Yield();
              state = entry.State.Value;
              if (state is < EntryState.LoadingAsyncEnqueued or >= EntryState.Loaded) {
                // either an error happened or the asset is loaded
                break;
              }
              // TODO: what about scheduled disposal, hm?
            }

            UnityEngine.Profiling.Profiler.EndSample();

            Log.TraceAssets($"Finished waitig for {guid}.");
            return ExpectValidAsset(entry);
          } else {
            return entry.LoadedAsset;
          }
        }
      }


      AssetObject ExpectValidAsset(Entry entry) {
        var asset = entry.LoadedAsset;
        if (asset == null) {
          throw new InvalidOperationException($"Expected asset to be loaded: {entry.Guid}");
        }

        var state = entry.State.Value;
        if (state < EntryState.LoadedInvokingCallbacks) {
          throw new InvalidOperationException($"Expected asset to be loaded: {entry.Guid}, but it's in state {state}");
        }

        return asset;
      }
    }
    
    private void ProcessDespawnQueue() {
      while (_disposeQueue.TryDequeue(out var entry)) {
        Debug.Assert(entry != null);

        if (entry.State.TryCompareExchange(EntryState.UnloadingInvokingCallbacks, EntryState.UnloadingEnqueued)) {
          DisposeEntry(entry);
        }
      }
    }

    private void DisposeEntry(Entry entry) {
      Assert.Check(entry.State.Value == EntryState.UnloadingInvokingCallbacks, "Expected asset {0} ({1}) to be in UnloadingInvokingCallbacks state: {2}", entry.Guid, entry.Path, entry.State.Value);
      try {
        var loadedAsset = entry.LoadedAsset;
        if (loadedAsset != null) {
          loadedAsset.Disposed(this, _allocator);
        }
      } catch (Exception ex) {
        Log.Exception($"Error while disposing {entry.Guid}", ex);
      } finally {
        entry.State.Exchange(EntryState.NotLoaded);
        entry.LoadedAsset = null;
        entry.Source.Release();
      }
    }

    private void ProcessLoadQueue() {
      while (_workedThreadLoadQueue.TryDequeue(out var tuple)) {
        var (guid, synchronous) = tuple;

        try {
          if (synchronous) {
            GetAsset(guid);
          } else {
            LoadAssetAsync(guid);
          }
        } catch (Exception ex) {
          Log.Exception(ex);
        }
      }
    }
    
    #endregion
        
    #region IResourceManager
    
    void IDisposable.Dispose() {
      DisposeAllAssetsImmediate();
    }
    
    AssetObjectState IResourceManager.GetAssetState(AssetGuid guid) {
      return GetAssetState(guid);
    }
    
    bool IResourceManager.DisposeAsset(AssetGuid guid) {
      return DisposeAsset(guid, immediate: false);
    }
    
    AssetObject IResourceManager.GetAsset(AssetGuid guid) {
      return GetAsset(guid);
    }
    
    bool IResourceManager.LoadAssetAsync(AssetGuid guid) {
      return LoadAssetAsync(guid);
    }
    
    Type IResourceManager.GetAssetType(AssetGuid guid) {
      return GetAssetType(guid);
    }
    
    AssetGuid IResourceManager.GetAssetGuid(string path) {
      return GetAssetGuid(path);
    }
    
    bool IResourceManager.FindNextAssetGuid(ref AssetObjectQuery query, out AssetGuid guid) {
      return FindNextAssetGuid(ref query, out guid);
    }
    
    void IResourceManager.Update(bool inSimulation, in Profiler profiler) {
      if (!_workedThreadLoadQueue.IsEmpty) {
        using (profiler.Scope("Asset Loading #ff3300")) {
          ProcessLoadQueue();
        }
      }

      if (!inSimulation) {
        ProcessDespawnQueue();
      }
    }
    
    event AssetObjectDisposingDelegate IResourceManager.AssetObjectDisposing {
      add => AssetObjectDisposing += value;
      remove => AssetObjectDisposing -= value;
    }
    
    #endregion
    
    internal enum EntryState {
      NotLoaded,
      Error,
      LoadingAsyncEnqueued,
      LoadingSyncEnqueued,
      LoadingAsync,
      LoadingSync,
      LoadedInvokingCallbacks,
      Loaded,
      UnloadingEnqueued,
      UnloadingInvokingCallbacks,
    }
  
    [Serializable]
    public sealed class Entry {
      public string    Path;
      public AssetGuid Guid;

      [NonSerialized]
      public volatile AssetObject LoadedAsset;

      [SerializeReference]
      public IQuantumAssetObjectSource Source;

      [NonSerialized]
      internal AtomicEnum<EntryState> State;
    }
  }
}