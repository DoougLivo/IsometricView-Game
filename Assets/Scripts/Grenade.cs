using System.Collections;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] GameObject meshObj;
    [SerializeField] GameObject effectObj;
    [SerializeField] Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Explosion());
    }

    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(3f); // 3초 후 폭발
        rb.linearVelocity = Vector3.zero; // 속도 0
        rb.angularVelocity = Vector3.zero; // 회전 0
        meshObj.SetActive(false); // 메쉬는 끄고
        effectObj.SetActive(true); // 이펙트는 활성화
    }
}
