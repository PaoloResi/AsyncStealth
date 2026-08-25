using UnityEngine;

public class HealthBarBillboard : MonoBehaviour
{
    private GameObject _mainCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position + _mainCamera.transform.forward);
    }
}
