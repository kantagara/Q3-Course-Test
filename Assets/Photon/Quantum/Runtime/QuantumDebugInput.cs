namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine;

  /// <summary>
  /// A Unity script that creates empty input for any Quantum game.
  /// </summary>
  public class QuantumDebugInput : MonoBehaviour {

    private Vector3 _mouseHitPosition;
    private void OnEnable() {
      QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
    }

    public void PollInput(CallbackPollInput callback) {
      
      
      var ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
      if (Physics.Raycast(ray, out var hit, 100, 1 << UnityEngine.LayerMask.NameToLayer("Ground")))
        _mouseHitPosition = hit.point;
      
      Quantum.Input i = new Quantum.Input {
        Movement = new FPVector2(UnityEngine.Input.GetAxis("Horizontal").ToFP(),
          UnityEngine.Input.GetAxis("Vertical").ToFP()),
        MousePosition = _mouseHitPosition.ToFPVector3().XZ,
        Fire = UnityEngine.Input.GetButton("Fire1"),
      };
      callback.SetInput(i, DeterministicInputFlags.Repeatable);
    }
  }
}
