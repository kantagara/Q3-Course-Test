using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    private Camera _camera;
    private void Start()
    {
        _camera = Camera.main;
        canvas.worldCamera = _camera;
    }

    // Update is called once per frame
    private void Update()
    {
        transform.LookAt(_camera.transform);
    }
}