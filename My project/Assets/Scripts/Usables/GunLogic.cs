using UnityEngine;
using UnityEngine.InputSystem;

public class GunLogic : MonoBehaviour
{

    private int ammo;
    private int reloadTime;
    public GameObject bulletPrefab;
    private Transform bulletSpawn;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bulletSpawn = transform.GetChild(0).transform;
    }

    // Update is called once per frame
    void Update()
    {
        //shoot();
    }

    public void shoot(Transform player)
    {
        Vector3 aimPoint = player.position + Vector3.up * 1f;
        bulletSpawn.LookAt(aimPoint);
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
        bullet.GetComponent<bullet>().Initalize(10, 100);

    }
}
