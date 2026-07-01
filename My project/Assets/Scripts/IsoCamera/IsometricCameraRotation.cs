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
        float rotateInput = camRotateAction.action.ReadValue<float>();
        if (rotateInput != 0)
        {
            transform.Rotate(Vector3.up, rotateInput * rotationSpeed * Time.deltaTime);
        }
    }
}
