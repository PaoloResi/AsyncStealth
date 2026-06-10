using UnityEngine;
using UnityEngine.InputSystem;


public class IsometricCameraPanner : MonoBehaviour
{
    public float panSpeed = 15f;
    private Camera _camera;
    public InputActionReference camMoveAction;
    void OnEnable() => camMoveAction.action.Enable();
    void OnDisable() => camMoveAction.action.Disable();
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _camera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 panPosition = camMoveAction.action.ReadValue<Vector2>();

        transform.position += Quaternion.Euler(0, _camera.transform.eulerAngles.y, 0) * new Vector3(panPosition.x, 0, panPosition.y) * (panSpeed * Time.deltaTime);
    }
}
