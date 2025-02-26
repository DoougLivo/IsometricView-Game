using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    [SerializeField] GameObject missile;
    [SerializeField] Transform missilePortA;
    [SerializeField] Transform missilePortB;
    [SerializeField] Transform bossRockArea;
    [SerializeField] bool isLook; // 점프 공격 시 플레이어를 바라보지 않고 그 방향 그대로 유지 하기 위한 플래그

    Vector3 lookVec; // 플레이어 움직임 예측 벡터 변수 생성 (보스는 지능이 더 높음)
    Vector3 tauntVec; // 점프 공격 위치

    void Awake()
    {
        // 부모클래스(Enemy) 에서 가져와 그대로 복붙함, 부모인 Enemy의 Awake는 실행 안되기 때문
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>(); // 메쉬 렌더러가 자식에게 있음
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        rb.linearDamping = 5f; // 마찰력
        rb.angularDamping = 5f; // 회전 마찰력

        nav.isStopped = true; // 네비게이션 기능 막음 (플레이어한테 따라가지 않게 함)

        StartCoroutine(Think());
    }

    void Update()
    {
        if (isDead) // 죽었을 때
        {
            StopAllCoroutines(); // 모든 패턴 정지
            return; // 이 밑의 모든 로직은 실행 되지 않도록 막음
        }

        if (isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            // 플레이어 입력 값으로 예측 벡터값 생성
            lookVec = new Vector3(h, 0, v) * 5f;
            transform.LookAt(target.position + lookVec); // 예측을 해서 플레이어를 바라봄
        } else
        {
            nav.SetDestination(tauntVec); // 점프공격 할 때 목표지점으로 이동하도록 함
        }
    }

    IEnumerator Think() // 행동 패턴을 결정해주는 코루틴
    {
        yield return new WaitForSeconds(0.1f); // 높아질 수록 난이도가 쉬워짐

        int randomAction = Random.Range(0, 5); // 행동 패턴 랜덤 결정
        switch (randomAction) // switch 문에서 break를 생략하여 조건을 늘릴 수 있음
        {
            case 0:
            case 1:
                // 미사일 발사 패턴
                StartCoroutine(MissileShot());
                break;
            case 2:
            case 3:
                // 돌 굴리기 패턴
                StartCoroutine(RockShot());
                break;
            case 4:
                // 점프 공격 패턴
                StartCoroutine(Taunt());
                break;
        }
    }

    IEnumerator MissileShot()
    {
        anim.SetTrigger("doShot"); // 애니메이션 실행

        yield return new WaitForSeconds(0.2f); // 0.2초 뒤 미사일 A 발사
        GameObject instantMissileA = Instantiate(missile, missilePortA.position, missilePortA.rotation);
        BossMissile bossMissileA = instantMissileA.GetComponent<BossMissile>();
        bossMissileA.target = target; // 플레이어를 타켓으로 지정

        yield return new WaitForSeconds(0.3f); // 0.3초 뒤 미사일 B 발사
        GameObject instantMissileB = Instantiate(missile, missilePortB.position, missilePortB.rotation);
        BossMissile bossMissileB = instantMissileB.GetComponent<BossMissile>();
        bossMissileB.target = target; // 플레이어를 타켓으로 지정

        yield return new WaitForSeconds(2f); // 2초 뒤(미사일 다 발사 후)

        StartCoroutine(Think()); // 다음 패턴을 위해 다시 Think() 코루틴 실행
    }

    IEnumerator RockShot()
    {
        isLook = false; // 기 모을 때는 바라보기 중지
        anim.SetTrigger("doBigShot"); // 애니메이션 실행
        GameObject instantBossRock = Instantiate(bullet, bossRockArea.position, transform.rotation); // 보스 돌 생성
        BossRock bossRock = instantBossRock.GetComponent<BossRock>();
        //bossRock.rb.AddForce(transform.forward * 40, ForceMode.Impulse); // 보스 돌 발사 힘 추가

        yield return new WaitForSeconds(3f);
        isLook = true; // 다시 바라보도록 함

        StartCoroutine(Think()); // 다음 패턴을 위해 다시 Think() 코루틴 실행
    }

    IEnumerator Taunt()
    {
        tauntVec = target.position + lookVec; // 점프공격 할 위치

        isLook = false; // 점프공격시 시선 고정
        nav.isStopped = false; // 네비게이션이 정상적으로 따라가도록 함
        boxCollider.enabled = false; // 점프시 플레이어를 밀지 않도록 콜라이더 끔
        anim.SetTrigger("doTaunt"); // 애니메이션 실행

        yield return new WaitForSeconds(1.5f); // 1.5초 후
        meleeArea.enabled = true; // 공격 범위 콜라이더 활성화
        boxCollider.enabled = true; // 다시 활성화 (보스 피격 가능한 타이밍이 이쯤이 적당함)
        
        yield return new WaitForSeconds(0.5f); // 0.5초 후
        meleeArea.enabled = false; // 공격 범위 콜라이더 비활성화
        
        yield return new WaitForSeconds(1f);
        isLook = true; // 다시 활성화
        nav.isStopped = true; // 다시 따라가지 않도록 함
        
        StartCoroutine(Think()); // 다음 패턴을 위해 다시 Think() 코루틴 실행
    }
}
