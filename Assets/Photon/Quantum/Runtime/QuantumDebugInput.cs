namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine;

  /// <summary>
  /// A Unity script that creates empty input for any Quantum game.
  /// </summary>
  public class QuantumDebugInput : MonoBehaviour {

    private Vector3 _previousPosition;
    private void OnEnable() {
      QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
    }

    public void PollInput(CallbackPollInput callback) {
      
      
      var ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
      if (Physics.Raycast(ray, out var hit, 100, 1 << UnityEngine.LayerMask.NameToLayer("Ground")))
        _previousPosition = hit.point;
      
      Quantum.Input i = new Quantum.Input {
        Movement = new FPVector2(UnityEngine.Input.GetAxis("Horizontal").ToFP(),
          UnityEngine.Input.GetAxis("Vertical").ToFP()),
        Jump = UnityEngine.Input.GetButton("Jump"),
        MousePosition = _previousPosition.ToFPVector3().XZ
      };
      callback.SetInput(i, DeterministicInputFlags.Repeatable);
    }
  }
}
