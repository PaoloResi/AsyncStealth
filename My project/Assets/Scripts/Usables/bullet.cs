using UnityEngine;

public class bullet : MonoBehaviour
{
    float speed;
    float damage;

    public void Initalize(int dmg, float spd)
    {
        damage = dmg;
        speed = spd;
        Destroy(gameObject, 3f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    public void HandleHit(Collider other)
    {
        //Destroy(gameObject);
        print("destroyed bullet");
    }
}
