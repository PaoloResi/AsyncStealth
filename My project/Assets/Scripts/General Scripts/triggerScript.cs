using UnityEngine;

public class triggerScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GetComponentInParent<endLevel>().HandleTrigger(other);
    }
}
