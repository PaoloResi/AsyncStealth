using UnityEngine;
using UnityEngine.InputSystem;

public class GunLogic : MonoBehaviour
{

    public InputActionReference shootAction;
    private int ammo;
    private int reloadTime;
    public GameObject bulletPrefab;
    private Transform bulletSpawn;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bulletSpawn = transform.GetChild(2).transform;
    }

    // Update is called once per frame
    void Update()
    {
        shoot();
    }

    public void shoot()
    {
        if (shootAction.action.WasPressedThisFrame())
        {
           GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
           bullet.GetComponent<bullet>().Initalize(5, 5);
        }
    }
}
