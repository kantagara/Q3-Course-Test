# Online Documentation

https://doc.photonengine.com/quantum

# Content

Quantum is split into four assemblies:

  - `Quantum.Simulation`: contains simulation code. Any user simulation code should be added to this assembly with `AssemblyDefinitionReferences`. Unity/Odin property attributes can be used at will, but any use of non-deterministic Unity API is heavy discouraged. Code form this assembly can be easily worked on as a standalone `.csproj`, similar to `quantum.code.csproj` in Quantum 2.
  - `Quantum.Unity`: contains code specific to Quantum's integration with Unity. Additionally, CodeGen emits `MonoBehaviours` that wrap component prototypes.
  - `Quantum.Unity.Editor`: contains editor code for `Quantum.Simulation` and `Quantum.Unity`
  - `Quantum.Unity.Editor.CodeGen`: contains CodeGen integration code. It is fully independent of other Quantum assemblies, so can be always run, even if there are compile errors - this may require exiting Safe Mode.

After installing Quantum, user is presented with following folder structure:

```
Assets
├───Photon
│   ├───PhotonLibs
│   ├───PhotonRealtime
│   └───Quantum
└───QuantumUser
    ├───Editor
    │   ├───CodeGen
    |   └───Generated       
    ├───Resources
    ├───Scenes
    ├───Simulation
    │   └───Generated
    └───View
        └───Generated       
```

All the deterministic simulation code should be placed in `QuantumUser/Simulation`. Any code extending the view represented by either `Quantum.Unity` or `Quantum.Unity.Editor` classes should be placed in `Runtime` and `Editor`, respectively.

The Quantum SDK is structured into the following folders:

* `Assets/Photon/Quantum/Assemblies` - contain `netstandard2.1` Quantum dlls (Quantum.Deterministic, Quantum.Engine, Quantum.Corium, and Quantum.Log) in release and debug configuration. The dlls have dependencies to UnityEngine.dll for inspector purposes
* `Assets/Photon/Quantum/Editor` - contains Quantum editor scripts that are compiled into Quantum.Unity.Editor.dll
* `Assets/Photon/Quantum/Editor/Assemblies` - contains Quantum CodeGen dependencies
* `Assets/Photon/Quantum/Editor/CodeGen` - contains Quantum CodeGen tools that compile into an extra dll Quantum.Unity.Editor.CodeGen.dll
* `Assets/Photon/Quantum/Simulation` - contains Quantum simulation code that is compiled into Quantum.Simulation.dll
* `Assets/Photon/Quantum/Resources` - contains fixed point math lookup tables (LUT), gizmos, a Quantum stats and multi runner prefab
* `Assets/Photon/Quantum/Runtime` - contains Quantum Unity scripts that compile into Quantum.Unity.dll
