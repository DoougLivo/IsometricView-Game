using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage;
    [SerializeField] bool isMelee; // 적의 근접공격 범위가 파괴되지 않도록 조건 추가

    void OnCollisionEnter(Collision collision)
    {
        // 탄피
        if (collision.gameObject.tag == "Floor") 
        {
            Destroy(gameObject, 3f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 총알
        if (!isMelee && other.gameObject.tag == "Wall")
        {
            Destroy(gameObject);
        }
    }
}
