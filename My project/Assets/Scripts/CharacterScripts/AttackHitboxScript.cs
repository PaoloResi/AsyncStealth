using System.Collections.Generic;
using UnityEngine;

public class AttackHitboxScript : MonoBehaviour
{
    public List<Collider> enemy = new List<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            print("enemy added to list");
            enemy.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (enemy.Contains(other))
        {
            print("enemy removed from list");
            enemy.Remove(other);
        }
    }
}
