using System.Collections.Generic;
using UnityEngine;

public class AttackHitboxScript : MonoBehaviour
{
    public List<Collider> enemy = new List<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.gameObject.name == "Capsule")
        {
            enemy.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (enemy.Contains(other))
        {
            enemy.Remove(other);
        }
    }
}
