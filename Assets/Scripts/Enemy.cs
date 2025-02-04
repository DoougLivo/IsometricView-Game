using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] int maxHealth;
    [SerializeField] int curHealth;

    Rigidbody rb;
    BoxCollider boxCollider;
    Material mat;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponent<MeshRenderer>().material;
        rb.linearDamping = 5f; // 마찰력
        rb.angularDamping = 5f; // 회전 마찰력
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;

            // 넉백
            Vector3 reactVec = transform.position - other.transform.position;
            StartCoroutine(OnDamage(reactVec));

            Debug.Log("Melee : " + curHealth);
        } else if (other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;

            // 넉백
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject); // 총알 파괴
            StartCoroutine(OnDamage(reactVec));

            Debug.Log("Range : " + curHealth);
        }
    }

    IEnumerator OnDamage(Vector3 reactVec)
    {
        mat.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0) // 아직 안죽음
        {
            mat.color = Color.white;

            // 넉백
            reactVec = reactVec.normalized; // 값 통일
            rb.AddForce(reactVec * 3, ForceMode.Impulse);
            rb.angularVelocity = Vector3.zero; // 회전 안되게 함
        } else // 죽음
        {
            mat.color = Color.gray;
            gameObject.layer = 12; // EnemyDead 레이어로 변경

            // 넉백
            reactVec = reactVec.normalized; // 값 통일
            reactVec += Vector3.up * 3; // 살짝 점프시킴
            rb.AddForce(reactVec * 5, ForceMode.Impulse);
            rb.angularVelocity = Vector3.zero; // 회전 안되게 함

            Destroy(gameObject, 4);
        }
    }
}
