#if !QUANTUM_DEV

#region Assets/Photon/Quantum/Runtime/GraphProfilers/QuantumGraphPlayerLoopUtility.cs

namespace Quantum.Profiling
{
  using System;
  using System.Collections.Generic;
  using UnityEngine;
  using UnityEngine.LowLevel;

  public static partial class QuantumGraphPlayerLoopUtility
  {
    public static void SetDefaultPlayerLoopSystem()
    {
      PlayerLoop.SetPlayerLoop(PlayerLoop.GetDefaultPlayerLoop());
    }

    public static bool HasPlayerLoopSystem(Type playerLoopSystemType)
    {
      if (playerLoopSystemType == null)
        return false;

      PlayerLoopSystem loopSystem = PlayerLoop.GetCurrentPlayerLoop();
      for (int i = 0, subSystemCount = loopSystem.subSystemList.Length; i < subSystemCount; ++i) {

        var subSubSystems = loopSystem.subSystemList[i].subSystemList;
        if (subSubSystems == null) {
          continue;
        }

        for (int j = 0; j < subSubSystems.Length; ++j) {
          if (subSubSystems[j].type == playerLoopSystemType)
            return true;
        }
      }

      return false;
    }

    public static bool AddPlayerLoopSystem(Type playerLoopSystemType, Type targetLoopSystemType, PlayerLoopSystem.UpdateFunction updateFunction, int position = -1)
    {
      if (playerLoopSystemType == null || targetLoopSystemType == null || updateFunction == null)
        return false;

      PlayerLoopSystem loopSystem = PlayerLoop.GetCurrentPlayerLoop();
      for (int i = 0, subSystemCount = loopSystem.subSystemList.Length; i < subSystemCount; ++i)
      {
        PlayerLoopSystem subSystem = loopSystem.subSystemList[i];
        if (subSystem.type == targetLoopSystemType)
        {
          PlayerLoopSystem targetSystem = new PlayerLoopSystem();
          targetSystem.type = playerLoopSystemType;
          targetSystem.updateDelegate = updateFunction;

          List<PlayerLoopSystem> subSubSystems = new List<PlayerLoopSystem>(subSystem.subSystemList);
          if (position >= 0)
          {
            if (position > subSubSystems.Count)
              throw new ArgumentOutOfRangeException(nameof(position));

            subSubSystems.Insert(position, targetSystem);
//						Debug.LogWarningFormat("Added Player Loop System: {0} to: {1} position: {2}/{3}", playerLoopSystemType.FullName, subSystem.type.FullName, position, subSubSystems.Count - 1);
          }
          else
          {
            subSubSystems.Add(targetSystem);
//						Debug.LogWarningFormat("Added Player Loop System: {0} to: {1} position: {2}/{2}", playerLoopSystemType.FullName, subSystem.type.FullName, subSubSystems.Count - 1);
          }

          subSystem.subSystemList = subSubSystems.ToArray();
          loopSystem.subSystemList[i] = subSystem;

          PlayerLoop.SetPlayerLoop(loopSystem);

          return true;
        }
      }

      Debug.LogErrorFormat("Failed to add Player Loop System: {0} to: {1}", playerLoopSystemType.FullName, targetLoopSystemType.FullName);

      return false;
    }

    public static bool AddPlayerLoopSystem(Type playerLoopSystemType, Type targetSubSystemType, PlayerLoopSystem.UpdateFunction updateFunctionBefore, PlayerLoopSystem.UpdateFunction updateFunctionAfter)
    {
      if (playerLoopSystemType == null || targetSubSystemType == null || (updateFunctionBefore == null && updateFunctionAfter == null))
        return false;

      PlayerLoopSystem loopSystem = PlayerLoop.GetCurrentPlayerLoop();
      for (int i = 0, subSystemCount = loopSystem.subSystemList.Length; i < subSystemCount; ++i)
      {
        PlayerLoopSystem subSystem = loopSystem.subSystemList[i];
        for (int j = 0, subSubSystemCount = subSystem.subSystemList.Length; j < subSubSystemCount; ++j)
        {
          PlayerLoopSystem subSubSystem = subSystem.subSystemList[j];
          if (subSubSystem.type == targetSubSystemType)
          {
            List<PlayerLoopSystem> subSubSystems = new List<PlayerLoopSystem>(subSystem.subSystemList);
            int currentPosition = j;

            if (updateFunctionBefore != null)
            {
              PlayerLoopSystem playerLoopSystem = new PlayerLoopSystem();
              playerLoopSystem.type = playerLoopSystemType;
              playerLoopSystem.updateDelegate = updateFunctionBefore;

              subSubSystems.Insert(currentPosition, playerLoopSystem);

//							Debug.LogWarningFormat("Added Player Loop System: {0} to: {1} before: {2}", playerLoopSystemType.FullName, subSystem.type.FullName, subSubSystem.type.FullName);

              ++currentPosition;
            }

            if (updateFunctionAfter != null)
            {
              ++currentPosition;

              PlayerLoopSystem playerLoopSystem = new PlayerLoopSystem();
              playerLoopSystem.type = playerLoopSystemType;
              playerLoopSystem.updateDelegate = updateFunctionAfter;

              subSubSystems.Insert(currentPosition, playerLoopSystem);

//							Debug.LogWarningFormat("Added Player Loop System: {0} to: {1} after: {2}", playerLoopSystemType.FullName, subSystem.type.FullName, subSubSystem.type.FullName);
            }

            subSystem.subSystemList = subSubSystems.ToArray();
            loopSystem.subSystemList[i] = subSystem;

            PlayerLoop.SetPlayerLoop(loopSystem);

            return true;
          }
        }
      }

      Debug.LogErrorFormat("Failed to add Player Loop System: {0}", playerLoopSystemType.FullName);

      return false;
    }

    public static bool RemovePlayerLoopSystems(Type playerLoopSystemType)
    {
      if (playerLoopSystemType == null)
        return false;

      bool setPlayerLoop = false;

      PlayerLoopSystem loopSystem = PlayerLoop.GetCurrentPlayerLoop();
      for (int i = 0, subSystemCount = loopSystem.subSystemList.Length; i < subSystemCount; ++i)
      {
        PlayerLoopSystem subSystem = loopSystem.subSystemList[i];
        if (subSystem.subSystemList == null)
          continue;

        bool removedFromSubSystem = false;

        List<PlayerLoopSystem> subSubSystems = new List<PlayerLoopSystem>(subSystem.subSystemList);
        for (int j = subSubSystems.Count - 1; j >= 0; --j)
        {
          if (subSubSystems[j].type == playerLoopSystemType)
          {
            subSubSystems.RemoveAt(j);
            removedFromSubSystem = true;
//						Debug.LogWarningFormat("Removed Loop System: {0} from: {1}", playerLoopSystemType.FullName, subSystem.type.FullName);
          }
        }

        if (removedFromSubSystem == true)
        {
          setPlayerLoop = true;

          subSystem.subSystemList = subSubSystems.ToArray();
          loopSystem.subSystemList[i] = subSystem;
        }
      }

      if (setPlayerLoop == true)
      {
        PlayerLoop.SetPlayerLoop(loopSystem);
      }

      return setPlayerLoop;
    }

    public static void DumpPlayerLoopSystems()
    {
      Debug.LogError("====================================================================================================");

      PlayerLoopSystem loopSystem = PlayerLoop.GetCurrentPlayerLoop();
      for (int i = 0, subSystemCount = loopSystem.subSystemList.Length; i < subSystemCount; ++i)
      {
        PlayerLoopSystem subSystem = loopSystem.subSystemList[i];

        Debug.LogWarning(subSystem.type.FullName);

        List<PlayerLoopSystem> subSubSystems = new List<PlayerLoopSystem>(subSystem.subSystemList);
        for (int j = 0; j < subSubSystems.Count; ++j)
        {
          Debug.Log("    " + subSubSystems[j].type.FullName);
        }
      }

      Debug.LogError("====================================================================================================");
    }
  }
}


#endregion


#region Assets/Photon/Quantum/Runtime/GraphProfilers/QuantumGraphPool.cs

namespace Quantum.Profiling
{
  using System.Collections.Generic;
  using System.Runtime.CompilerServices;

  public static class QuantumGraphPool<T> where T : new()
  {
    private const int POOL_CAPACITY = 4;

    private static List<T> _pool = new List<T>(POOL_CAPACITY);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Get()
    {
      bool found = false;
      T    item  = default;

      lock (_pool)
      {
        int index = _pool.Count - 1;
        if (index >= 0)
        {
          found = true;
          item  = _pool[index];

          _pool[index] = _pool[_pool.Count - 1];
          _pool.RemoveAt(_pool.Count - 1);
        }
      }

      if (found == false)
      {
        item = new T();
      }

      return item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Return(T item)
    {
      if (item == null)
        return;

      lock (_pool)
      {
        _pool.Add(item);
      }
    }
  }
}



#endregion


#region Assets/Photon/Quantum/Runtime/GraphProfilers/QuantumGraphProfiler.cs

namespace Quantum.Profiling
{
  using UnityEngine;

  public abstract class QuantumGraphProfiler<TGraph> : MonoBehaviour where TGraph : QuantumGraphSeries
  {
    protected TGraph  Graph    { get; private set; }
    protected float[] Values   { get; private set; }
    protected bool    IsActive { get; private set; }

    [SerializeField]
    private bool       _enableOnAwake;
    [SerializeField]
    private GameObject _renderObject;

    private int        _targetFPS;

    public void ToggleVisibility()
    {
      SetState(!IsActive);
    }

    protected virtual void OnInitialize()             {}
    protected virtual void OnDeinitialize()           {}
    protected virtual void OnActivated()              {}
    protected virtual void OnDeactivated()            {}
    protected virtual void OnUpdate()                 {}
    protected virtual void OnRestore()                {}
    protected virtual void OnTargetFPSChaged(int fps) {}

    private void Awake()
    {
      Graph  = GetComponentInChildren<TGraph>(true);
      Values = new float[Graph.Samples];

      Graph.Initialize();

      OnInitialize();

      SetState(_enableOnAwake);
    }

    private void Update()
    {
      if (_targetFPS != Application.targetFrameRate)
      {
        _targetFPS = Application.targetFrameRate;

        OnTargetFPSChaged(_targetFPS > 0 ? _targetFPS : 60);
      }

      OnUpdate();
    }

    private void OnDestroy()
    {
      OnDeinitialize();
    }

    private void OnApplicationFocus(bool focus)
    {
      Graph.Restore();
    }

    private void SetState(bool isActive)
    {
      IsActive = isActive;

      _renderObject.SetActive(isActive);

      if (isActive == true)
      {
        Restore();
        OnActivated();
      }
      else
      {
        OnDeactivated();
        Restore();
      }
    }

    private void Restore()
    {
      if (Values != null)
      {
        System.Array.Clear(Values, 0, Values.Length);
      }

      if (Graph != null)
      {
        Graph.Restore();
      }

      OnRestore();
    }
  }
}


#endregion


#region Assets/Photon/Quantum/Runtime/GraphProfilers/QuantumGraphProfilerMarkerSeries.cs

namespace Quantum.Profiling
{
  public abstract class QuantumGraphProfilerMarkerSeries : QuantumGraphProfiler<QuantumGraphSeriesMarker>
  {
    private int _offset;
    private int _samples;

    protected override void OnActivated()
    {
      _offset  = 0;
      _samples = 0;
    }

    protected void SetMarkers(params bool[] markers)
    {
      if (IsActive == false)
        return;

      int value = 0;

      for (int i = 0; i < markers.Length; ++i)
      {
        if (markers[i] == true)
        {
          value |= 1 << i;
        }
      }

      float[] values = Values;
      values[_offset] = value;

      _offset = (_offset + 1) % values.Length;
      ++_samples;

      Graph.SetValues(values, _offset , _samples);
    }
  }
}


#endregion


#region Assets/Photon/Quantum/Runtime/GraphProfilers/QuantumGraphProfilers.cs

namespace Quantum.Profiling
{
  using UnityEngine;
  using UnityEngine.PlayerLoop;

  public static class QuantumGraphProfilers
  {
    public static readonly QuantumGraphTimer FrameTimer   = new QuantumGraphTimer("Frame");
    public static readonly QuantumGraphTimer ScriptsTimer = new QuantumGraphTimer("Scripts");
    public static readonly QuantumGraphTimer RenderTimer  = new QuantumGraphTimer("Render");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitializeSubSystem()
    {
#if UNITY_EDITOR
      if (Application.isPlaying == false)
        return;
#endif
      if (QuantumGraphPlayerLoopUtility.HasPlayerLoopSystem(typeof(QuantumGraphProfilers)) == false)
      {
        QuantumGraphPlayerLoopUtility.AddPlayerLoopSystem(typeof(QuantumGraphProfilers), typeof(EarlyUpdate), EarlyUpdate, 0);
        QuantumGraphPlayerLoopUtility.AddPlayerLoopSystem(typeof(QuantumGraphProfilers), typeof(FixedUpdate.ScriptRunBehaviourFixedUpdate),  BeforeFixedUpdate, AfterFixedUpdate);
        QuantumGraphPlayerLoopUtility.AddPlayerLoopSystem(typeof(QuantumGraphProfilers), typeof(Update.ScriptRunBehaviourUpdate),            BeforeUpdate,      AfterUpdate);
        QuantumGraphPlayerLoopUtility.AddPlayerLoopSystem(typeof(QuantumGraphProfilers), typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate), BeforeLateUpdate,  AfterLateUpdate);
        QuantumGraphPlayerLoopUtility.AddPlayerLoopSystem(typeof(QuantumGraphProfilers), typeof(PostLateUpdate), PostLateUpdateFirst, 0);
        QuantumGraphPlayerLoopUtility.AddPlayerLoopSystem(typeof(QuantumGraphProfilers), typeof(PostLateUpdate), PostLateUpdateLast);
      }

      Application.quitting -= OnApplicationQuit;
      Application.quitting += OnApplicationQuit;
    }

    private static void EarlyUpdate()
    {
#if UNITY_EDITOR
      if (Application.isPlaying == false)
      {
        QuantumGraphPlayerLoopUtility.RemovePlayerLoopSystems(typeof(QuantumGraphProfilers));
        return;
      }
#endif

      FrameTimer.Reset();
      ScriptsTimer.Reset();
      RenderTimer.Reset();

      FrameTimer.Start();
    }

    private static void BeforeFixedUpdate() { ScriptsTimer.Start(); }
    private static void AfterFixedUpdate()  { ScriptsTimer.Pause(); }
    private static void BeforeUpdate()      { ScriptsTimer.Start(); }
    private static void AfterUpdate()       { ScriptsTimer.Pause(); }
    private static void BeforeLateUpdate()  { ScriptsTimer.Start(); }
    private static void AfterLateUpdate()   { ScriptsTimer.Stop();  }

    private static void PostLateUpdateFirst()
    {
      RenderTimer.Start();
    }

    private static void PostLateUpdateLast()
    {
      RenderTimer.Stop();
      FrameTimer.Stop();
    }

    private static void OnApplicationQuit()
    {
      QuantumGraphPlayerLoopUtility.RemovePlayerLoopSystems(typeof(QuantumGraphProfilers));
    }
  }
}


#endregion


#region Assets/Photon/Quantum/Runtime/GraphProfilers/QuantumGraphProfilersUtility.cs

namespace Quantum.Profiling
{
  using Photon.Client;

  public static class QuantumGraphProfilersUtility
  {
    public static PhotonPeer GetNetworkPeer()
    {
      QuantumRunner quantumRunner = QuantumRunner.Default;
      if (quantumRunner != null && quantumRunner.NetworkClient != null)
      {
        return quantumRunner.NetworkClient.RealtimePeer;
      }

      return null;
    }
  }
}


#endregion


#region Assets/Photon/Quantum/Runtime/GraphProfilers/QuantumGraphProfilerValueSeries.cs

namespace Quantum.Profiling
{
  public abstract class QuantumGraphProfilerValueSeries : QuantumGraphProfiler<QuantumGraphSeriesValue>
  {
    private int _offset;
    private int _samples;

    protected override void OnActivated()
    {
      _offset  = 0;
      _samples = 0;
    }

    protected void AddValue(float value)
    {
      if (IsActive == false)
        return;

      float[] values = Values;
      values[_offset] = value;

      _offset = (_offset + 1) % values.Length;
      ++_samples;

      Graph.SetValues(values, _offset , _samples);
    }
  }
}


#endregion


#region Assets/Photon/Quantum/Runtime/GraphProfilers/QuantumGraphSeries.cs

namespace Quantum.Profiling
{
  using UnityEngine;
  using UnityEngine.UI;

  public abstract class QuantumGraphSeries : MonoBehaviour
  {
    private const string SHADER_PROPERTY_VALUES  = "_Values";
    private const string SHADER_PROPERTY_SAMPLES = "_Samples";

    public int Samples { get { return _samples; } }

    [SerializeField]
    protected Image _targetImage;
    [SerializeField][Range(60, 540)]
    protected int _samples = 300;

    protected float[] _values;
    protected Material _material;
    protected int _valuesShaderPropertyID;

    protected virtual void OnInitialize() {}
    protected virtual void OnRestore()    {}

    public void Initialize()
    {
      _valuesShaderPropertyID = Shader.PropertyToID(SHADER_PROPERTY_VALUES);

      _values = new float[_samples];

      _material = new Material(_targetImage.material);
      _targetImage.material = _material;

      Restore();

      OnInitialize();
    }

    public virtual void SetValues(float[] values, int offset, int samples)
    {
      if (_values == null || values == null || _values.Length != values.Length)
        return;

      for (int i = 0; i < _samples; ++i, ++offset)
      {
        offset %= _samples;

        _values[i] = values[offset];
      }

      _material.SetFloatArray(_valuesShaderPropertyID, _values);
    }

    public void Restore()
    {
      if (_material != null)
      {
        _material.SetInt(SHADER_PROPERTY_SAMPLES, _samples);
        _material.SetFloatArray(_valuesShaderPropertyID, _values);
      }

      OnRestore();
    }
  }
}


#endregion


#region Assets/Photon/Quantum/Runtime/GraphProfilers/QuantumGraphTimer.cs

namespace Quantum.Profiling
{
  using System;
  using System.Diagnostics;
  using System.Runtime.CompilerServices;

  public sealed partial class QuantumGraphTimer
  {
    public enum EState
    {
      Stopped = 0,
      Running = 1,
      Paused  = 2,
    }

    // PUBLIC MEMBERS

    public readonly int    ID;
    public readonly string Name;

    public EState   State      { get { return _state;   } }
    public int      Counter    { get { return _counter; } }
    public TimeSpan TotalTime  { get { if (_state == EState.Running) { Update(); } return new TimeSpan(_totalTicks);  } }
    public TimeSpan RecentTime { get { if (_state == EState.Running) { Update(); } return new TimeSpan(_recentTicks); } }
    public TimeSpan PeakTime   { get { if (_state == EState.Running) { Update(); } return new TimeSpan(_peakTicks);   } }
    public TimeSpan LastTime   { get { if (_state == EState.Running) { Update(); } return new TimeSpan(_lastTicks);   } }

    // PRIVATE MEMBERS

    private EState _state;
    private int    _counter;
    private long   _baseTicks;
    private long   _totalTicks;
    private long   _recentTicks;
    private long   _peakTicks;
    private long   _lastTicks;

    // CONSTRUCTORS

    public QuantumGraphTimer() : this(null)
    {
    }

    public QuantumGraphTimer(string name) : this(-1, name)
    {
    }

    public QuantumGraphTimer(int id, string name)
    {
      ID   = id;
      Name = name;
    }

    // PUBLIC METHODS

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Start()
    {
      if (_state == EState.Running)
        return;

      if (_state != EState.Paused)
      {
        if (_recentTicks != 0)
        {
          _lastTicks   = _recentTicks;
          _recentTicks = 0;
        }

        ++_counter;
      }

      _baseTicks = Stopwatch.GetTimestamp();
      _state     = EState.Running;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Pause()
    {
      if (_state != EState.Running)
        return;

      Update();

      _state = EState.Paused;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Stop()
    {
      if (_state == EState.Running)
      {
        Update();
      }

      _state = EState.Stopped;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Restart()
    {
      if (_recentTicks != 0)
      {
        _lastTicks = _recentTicks;
      }

      _state       = EState.Running;
      _counter     = 1;
      _baseTicks   = Stopwatch.GetTimestamp();
      _recentTicks = 0;
      _totalTicks  = 0;
      _peakTicks   = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
      if (_recentTicks != 0)
      {
        _lastTicks = _recentTicks;
      }

      _state       = EState.Stopped;
      _counter     = 0;
      _baseTicks   = 0;
      _recentTicks = 0;
      _totalTicks  = 0;
      _peakTicks   = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Return()
    {
      Return(this);
    }

    public void Combine(QuantumGraphTimer other)
    {
      if (other._state == EState.Running)
      {
        other.Update();
      }

      _totalTicks += other._totalTicks;

      if (_state == EState.Stopped)
      {
        _recentTicks = other._recentTicks;
        if (_recentTicks > _peakTicks)
        {
          _peakTicks = _recentTicks;
        }
      }

      if (other._peakTicks > _peakTicks)
      {
        _peakTicks = other._peakTicks;
      }

      _counter += other._counter;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetTotalSeconds()
    {
      return (float)TotalTime.TotalSeconds;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetTotalMilliseconds()
    {
      return (float)TotalTime.TotalMilliseconds;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetRecentSeconds()
    {
      return (float)RecentTime.TotalSeconds;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetRecentMilliseconds()
    {
      return (float)RecentTime.TotalMilliseconds;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetPeakSeconds()
    {
      return (float)PeakTime.TotalSeconds;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetPeakMilliseconds()
    {
      return (float)PeakTime.TotalMilliseconds;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetLastSeconds()
    {
      return (float)LastTime.TotalSeconds;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetLastMilliseconds()
    {
      return (float)LastTime.TotalMilliseconds;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static QuantumGraphTimer Get(bool start = false)
    {
      QuantumGraphTimer timer = QuantumGraphPool<QuantumGraphTimer>.Get();
      if (start == true)
      {
        timer.Restart();
      }
      return timer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Return(QuantumGraphTimer timer)
    {
      timer.Reset();
      QuantumGraphPool<QuantumGraphTimer>.Return(timer);
    }

    // PRIVATE METHODS

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Update()
    {
      long ticks = Stopwatch.GetTimestamp();

      _totalTicks  += ticks - _baseTicks;
      _recentTicks += ticks - _baseTicks;

      _baseTicks = ticks;

      if (_recentTicks > _peakTicks)
      {
        _peakTicks = _recentTicks;
      }

      if (_totalTicks < 0L)
      {
        _totalTicks = 0L;
      }
    }
  }
}


#endregion

#endif
