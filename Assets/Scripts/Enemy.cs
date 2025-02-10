using NUnit.Framework.Constraints;
using System.Collections;
using UnityEngine;
using UnityEngine.AI; // Nav 관련 클래스 사용 시 필요

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C }; // 일반형, 돌격형, 원거리형
    public Type enemyType; // 타입을 지정할 변수

    [SerializeField] int maxHealth;
    [SerializeField] int curHealth;
    [SerializeField] Transform target;
    [SerializeField] BoxCollider meleeArea; // 일반형 몬스터 공격 범위
    [SerializeField] GameObject bullet; // 원거리형 몬스터 총알
    [SerializeField] bool isChase; // 추적을 결정
    [SerializeField] bool isAttack;

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
        if (nav.enabled) // nav가 활성화일 때만 추적함
        { 
            nav.SetDestination(target.position); // 도착할 목표 위치 지정 함수
            nav.isStopped = !isChase; // isChase가 true면 멈추지 않고, 반대로 false면 stop 함
        }
    }

    void FreezeVelocity() // 플레이어와 부딪혔을 때 밀리는 현상 방지
    {
        if (isChase) // 추적 중일 때만 가능, 적이 죽었을 때의 리액션을 살리기 위함
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero; // angularVelocity - 물리 회전 속도    
        }
    }

    void Targeting()
    {
        float targetRedius = 0; // 공격 반지름 (폭)
        float targetRange = 0; // 공격 범위

        // 각 타겟팅 수치 정하기
        switch (enemyType)
        {
            case Type.A: // 일반형
                targetRedius = 1.5f;
                targetRange = 3f;
                break;
            case Type.B: // 돌격형
                targetRedius = 1f; // 정확하게 돌격하기 위해 폭은 좁게 함
                targetRange = 12f; // 범위는 일반형의 네배
                break;
            case Type.C: // 원거리형
                targetRedius = 0.5f; // 더 정확하게 발사하기 위함
                targetRange = 25f; // 범위는 길게
                break;
        }

        // 전방으로 구체 레이를 쏨
        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position,
            targetRedius, transform.forward, targetRange, LayerMask.GetMask("Player"));

        // rayHits 변수에 데이터가 들어오면 공격 코루틴 실행
        if (rayHits.Length > 0 && !isAttack) // 공격 중이지 않을 때만 공격 가능
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        // 범위에 들어오면 먼저 정지한 다음, 애니메이션과 함께 공격 범위 활성화
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);
        
        switch (enemyType)
        {
            case Type.A: // 일반형
                yield return new WaitForSeconds(0.5f); // 공격 콜라이더 활성화
                meleeArea.enabled = true;

                yield return new WaitForSeconds(1f); // 공격 콜라이더 비활성화
                meleeArea.enabled = false;

                yield return new WaitForSeconds(1f); // 적 공격 속도 조정
                break;

            case Type.B: // 돌격형
                yield return new WaitForSeconds(0.5f); // 선 딜레이
                rb.AddForce(transform.forward * 20f, ForceMode.Impulse); // 앞으로 돌격
                meleeArea.enabled = true;

                yield return new WaitForSeconds(0.5f); // 돌격 시간
                rb.linearVelocity = Vector3.zero; // 멈춤
                meleeArea.enabled = false;

                yield return new WaitForSeconds(2f); // 적 공격 속도 조정
                break;

            case Type.C: // 원거리형
                yield return new WaitForSeconds(0.5f); // 미사일 발사 준비 동작
                // 미사일 만들고 발사
                GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation);
                Rigidbody rbBullet = instantBullet.GetComponent<Rigidbody>();
                rbBullet.AddForce(transform.forward * 20, ForceMode.Impulse);

                yield return new WaitForSeconds(2f); // 적 공격 속도 조정
                break;
        }

        // 공격 끝난 후
        isChase = true;
        isAttack = false;
        anim.SetBool("isAttack", false);
    }

    void FixedUpdate()
    {
        Targeting();
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

        //// 플레이어 감지
        //if (other.CompareTag("Player"))
        //{
        //    isChase = true;
        //}
    }

    //void OnTriggerExit(Collider other)
    //{
    //    isChase = false;
    //}

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
