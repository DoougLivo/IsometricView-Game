using System.Collections;
using UnityEngine;
using UnityEngine.AI; // Nav 관련 클래스 사용 시 필요

public class Enemy : MonoBehaviour
{
    [SerializeField] int maxHealth;
    [SerializeField] int curHealth;
    [SerializeField] Transform target;
    [SerializeField] bool isChase; // 추적을 결정

    Rigidbody rb;
    BoxCollider boxCollider;
    Material mat;
    NavMeshAgent nav;
    Animator anim;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponentInChildren<MeshRenderer>().material; // 메쉬 렌더러가 자식에게 있음
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        rb.linearDamping = 5f; // 마찰력
        rb.angularDamping = 5f; // 회전 마찰력

        Invoke("ChaseStart", 2); // 2초 뒤 추적함
    }

    void ChaseStart() // 추적 시작
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }

    void Update()
    {
        if (isChase) // isChase가 true 일 때만 추적함
            nav.SetDestination(target.position); // 도착할 목표 위치 지정 함수
    }

    void FreezeVelocity() // 플레이어와 부딪혔을 때 밀리는 현상 방지
    {
        if (isChase) // 추적 중일 때만 가능, 적이 죽었을 때의 리액션을 살리기 위함
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero; // angularVelocity - 물리 회전 속도    
        }
    }

    void FixedUpdate()
    {
        FreezeVelocity();
    }

    void OnTriggerEnter(Collider other)
    {
        // 몬스터가 무기에 피격 시
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;

            // 피격,넉백 구현
            Vector3 reactVec = transform.position - other.transform.position;
            StartCoroutine(OnDamage(reactVec, false));

            Debug.Log("Melee : " + curHealth);
        } else if (other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;

            // 피격,넉백 구현
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject); // 총알 파괴
            StartCoroutine(OnDamage(reactVec, false));

            Debug.Log("Range : " + curHealth);
        }

        // 플레이어 감지
        if (other.CompareTag("Player"))
        {
            isChase = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        isChase = false;
    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        curHealth -= 100; // 수류탄 대미지

        // 피격,넉백 구현
        Vector3 reacVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reacVec, true));
    }

    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
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
        } 
        else // 죽음
        {
            mat.color = Color.gray;
            gameObject.layer = 12; // EnemyDead 레이어로 변경
            isChase = false; // 추적 안함
            nav.enabled = false; // 수류탄 맞고 위로 날라가는걸(y축) 살리기 위해 NavMeshAgent를 끔
            anim.SetTrigger("doDie"); // 죽는 애니메이션
            
            // 넉백
            if (isGrenade) // 수류탄
            {
                reactVec = reactVec.normalized; // 값 통일
                reactVec += Vector3.up * 6; // 높이 점프시킴
                rb.freezeRotation = false; // 수류탄에 사망 시 회전 가능하도록 함
                rb.AddForce(reactVec * 5, ForceMode.Impulse); // 넉백
                rb.AddTorque(reactVec * 15, ForceMode.Impulse); // 회전
            } 
            else // 나머지
            {
                reactVec = reactVec.normalized; // 값 통일
                reactVec += Vector3.up * 3; // 살짝 점프시킴
                rb.AddForce(reactVec * 5, ForceMode.Impulse); // 넉백
                rb.angularVelocity = Vector3.zero; // 회전 안되게 함
            }
            
            Destroy(gameObject, 4); // 4초 후 파괴
        }
    }
}
