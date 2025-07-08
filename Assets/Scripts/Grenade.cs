using System.Collections;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] GameObject meshObj;
    [SerializeField] GameObject effectObj;
    [SerializeField] Rigidbody rb;

    public AudioSource grenadeSound; // 폭탄 소리

    public int grenadeDamage; // 수류탄 대미지

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
        grenadeSound.Play(); // 수류탄 소리 재생

        // 피격 범위 설정 / SphereCastAll - 구체 모양의 레이캐스팅 (모든 오브젝트)
        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, 
                                                        15, Vector3.up, 0f, 
                                                        LayerMask.GetMask("Enemy"));
        
        // foreach 문으로 수류탄 범위 적들의 피격함수를 호출
        foreach (RaycastHit hitObj in rayHits)
        {
            hitObj.transform.GetComponent<Enemy>().HitByGrenade(transform.position, grenadeDamage);
        }

        Destroy(gameObject, 4); // 수류탄은 파티클이 사라지는 시간을 고려하여 호출 (4초 정도 후 파괴)
    }
}
