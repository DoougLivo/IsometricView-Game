using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage;

    void OnCollisionEnter(Collision collision)
    {
        // ÅºÇÇ
        if (collision.gameObject.tag == "Floor") 
        {
            Destroy(gameObject, 3f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // ÃÑ¾Ë
        if (other.gameObject.tag == "Wall")
        {
            Destroy(gameObject);
        }
    }
}
