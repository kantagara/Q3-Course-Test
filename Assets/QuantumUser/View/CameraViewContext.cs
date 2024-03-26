using Cinemachine;
using Quantum;
using UnityEngine;

public class CameraViewContext : MonoBehaviour, IQuantumViewContext
{
    [field: SerializeField] public CinemachineVirtualCamera VirtualCamera { get; private set; }
}