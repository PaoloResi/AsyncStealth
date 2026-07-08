using UnityEngine;

public class BulletCollider : MonoBehaviour
{
    bullet bullet;

    void Awake()
    {
        bullet = GetComponentInParent<bullet>();
    }

    void OnTriggerEnter(Collider other)
    {
        bullet.HandleHit(other);
    }
}
