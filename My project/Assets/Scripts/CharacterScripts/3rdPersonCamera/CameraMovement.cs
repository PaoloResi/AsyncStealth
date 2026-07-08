using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] public Transform target;
    private const float YMin = -50.0f;
    private const float YMax = 50.0f;


    public float distance = 10.0f;
    private float currentX = 0.0f;
    private float currentY = 0.0f;
    public float sensivity = 4.0f;

    public InputActionReference lookAction;
    void OnEnable() => lookAction.action.Enable();
    void OnDisable() => lookAction.action.Disable();


    // Update is called once per frame
    void LateUpdate()
    {
        
        currentX += lookAction.action.ReadValue<Vector2>().x * sensivity * Time.deltaTime;
        currentY -= lookAction.action.ReadValue<Vector2>().y * sensivity * Time.deltaTime;

        currentY = Mathf.Clamp(currentY, YMin, YMax);

        Vector3 Direction = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        transform.position = target.position + rotation * Direction;

        transform.LookAt(target.position);
    }
}
