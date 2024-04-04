namespace Quantum {
  using UnityEngine;
  using UnityEngine.Rendering;

  /// <summary>
  /// A unity script that updates the Quantum runner.
  /// Also manages calls to Gizmos and DebugDraw required to render Quantum debug gizmos.
  /// If you are writing a custom SRP, you must call RenderPipeline.EndCameraRendering to trigger OnPostRenderInternal().
  /// </summary>
  public class QuantumRunnerBehaviour : QuantumMonoBehaviour {
    /// <summary>
    /// The runner object set during <see cref="QuantumRunner.StartGame(SessionRunner.Arguments)"/>
    /// </summary>
    public QuantumRunner Runner;

    public void OnEnable() {
      Camera.onPostRender += OnPostRenderInternal;
      RenderPipelineManager.endCameraRendering += OnPostRenderInternal;
    }

    public void OnDisable() {
      Camera.onPostRender -= OnPostRenderInternal;
      RenderPipelineManager.endCameraRendering -= OnPostRenderInternal;
    }

    public void Update() {
      Runner?.Update();
    }

    void OnPostRenderInternal(ScriptableRenderContext context, Camera camera) {
      OnPostRenderInternal(camera);
    }

    void OnPostRenderInternal( Camera camera) {
      if (Runner == null) {
        return;
      }

      if (Runner.Session == null) {
        return;
      }

      if (Runner.HideGizmos) {
        return;
      }

      DebugDraw.OnPostRender(camera);
    }
  }
}