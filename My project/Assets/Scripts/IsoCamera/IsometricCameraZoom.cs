using UnityEngine;
using UnityEngine.InputSystem;

public class IsometricCameraZoom : MonoBehaviour
{   
    public float zoomSpeed = 6;
    public float zoomSmoothness = 5;

    public float minZoom = 2;
    public float maxZoom = 40;

    private float _currentZoom;
    private Camera _camera;

    void Start()
    {
        _camera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        _currentZoom = Mathf.Clamp(_currentZoom - Mouse.current.scroll.ReadValue().y * zoomSpeed * Time.deltaTime, minZoom, maxZoom);
        _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _currentZoom, zoomSmoothness * Time.deltaTime);
    }
}
