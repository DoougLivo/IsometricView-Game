using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] int damage;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor") {
            Destroy(gameObject, 3f);
        } else if (collision.gameObject.tag == "Wall")
        {
            Destroy(gameObject);
        }
    }
}
