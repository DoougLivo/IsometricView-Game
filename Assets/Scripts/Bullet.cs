using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage;

    void OnCollisionEnter(Collision collision)
    {
        // ź��
        if (collision.gameObject.tag == "Floor") 
        {
            Destroy(gameObject, 3f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // �Ѿ�
        if (other.gameObject.tag == "Wall")
        {
            Destroy(gameObject);
        }
    }
}
