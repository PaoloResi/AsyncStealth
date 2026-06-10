using UnityEngine;
using UnityEngine.InputSystem;

public class IsometricCameraRotation : MonoBehaviour
{
    public float rotationSpeed = 5f;
    public InputActionReference camRotateAction;
    void OnEnable() => camRotateAction.action.Enable();
    void OnDisable() => camRotateAction.action.Disable();



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            float mouseDeltaX = camRotateAction.action.ReadValue<Vector2>().x;
            //Input.GetAxis("Mouse X");

            transform.Rotate(Vector3.up, mouseDeltaX * rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}
