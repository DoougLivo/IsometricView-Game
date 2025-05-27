using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    float hAxis;
    float vAxis;

    bool wDown;
    bool jDown;
    bool fDown;
    bool gDown;
    bool rDown;
    bool iDown;

    bool isJump;
    bool isDodge;
    bool isSwap;
    bool isReload;
    bool isFireReady = true;
    bool isBorder;
    bool isDamage;

    bool sDown1; // 무기 스왑 변수
    bool sDown2;
    bool sDown3;

    [SerializeField] int ammo;
    [SerializeField] int coin;
    [SerializeField] int health;

    [SerializeField] int maxAmmo;
    [SerializeField] int maxCoin;
    [SerializeField] int maxHealth;
    [SerializeField] int maxHasGrenades;

    [SerializeField] int jPower;
    [SerializeField] float speed;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rb;
    Animator anim;
    MeshRenderer[] meshs; // 플레이어의 머리, 몸, 팔, 다리 등 여러 메쉬를 가져와야 해서 배열로 선언함

    GameObject nearObj; // 플레이어 근처에 있는 오브젝트
    Weapon equipWeapon; // 장착중인 무기

    [SerializeField] GameObject[] weapons;
    [SerializeField] bool[] hasWeapons;
    [SerializeField] GameObject[] grenades;
    [SerializeField] int hasGrenades;
    [SerializeField] GameObject grenadeObj;

    [SerializeField] Camera followCamera;

    int equipWeaponIndex = -1; // 초기값 설정
    float fireDelay;

    void Awake()
    {
        Application.targetFrameRate = 60; // 60 프레임 고정

        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>(); // Animator가 자식 오브젝트에 있기 때문
        meshs = GetComponentsInChildren<MeshRenderer>(); // 모든 자식의 MeshRenderer 컴포넌트를 다 가져옴
    }

    // Update is called once per frame
    void Update()
    {
        // 키입력
        GetInput();

        // 이동
        Move();

        // 회전
        Turn();

        // 점프
        Jump();

        // 수류탄 투척
        Grenade();

        // 공격
        Attack();

        // 재장전
        Reload();

        // 회피
        Dodge();

        // 무기 스왑 (1,2,3 키)
        Swap();

        // 상호작용 (e키)
        Interaction();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal"); // 정수로 받음
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButtonDown("Fire2");
        rDown = Input.GetButtonDown("Reload");
        iDown = Input.GetButtonDown("Interaction");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized; // 방향 값이 1로 보정된 벡터

        if (isDodge) // 회피 중일 때
            moveVec = dodgeVec; // 움직임 벡터를 회피 벡터로 바꿈 (회피중엔 방향 못바꾸게 함)

        if (/*isSwap*/!isFireReady/*isReload*/) // 공격, 스왑할 때 움직이지 못하게 함, 리로드
            moveVec = Vector3.zero;

        if (!isBorder) { // 벽을 넘어가지 못하게 막음
            transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;
        }

        anim.SetBool("isRun", moveVec != Vector3.zero); // moveVec이 0이 아닐때 (움직일때)
        anim.SetBool("isWalk", wDown);
    }

    void Turn()
    {
        // 키보드에 의한 회전
        transform.LookAt(transform.position + moveVec); // 나아가는 방향으로 바라보게 함

        // 마우스에 의한 회전
        if (fDown) { // 마우스 클릭 시
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition); // 마우스 방향으로 레이를 쏨
            RaycastHit rayHit; // 정보를 저장할 변수
            if (Physics.Raycast(ray, out rayHit, 100)) // out - return처럼 반환값을 변수에 저장하는 키워드
            {
                Vector3 nextVec = rayHit.point - transform.position; // 상대적 위치 구함
                nextVec.y = 0; // y축은 0으로 고정함
                transform.LookAt(transform.position + nextVec);
            }
        }
    }

    void Jump()
    {
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap && !isReload) // 움직이지 않을 때 스페이스바 누르면 점프
        {
            rb.AddForce(Vector3.up * jPower, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Grenade()
    {
        if (hasGrenades == 0) return;

        if (gDown && !isReload && !isSwap)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition); // 마우스 방향으로 레이를 쏨
            RaycastHit rayHit; // 정보를 저장할 변수
            if (Physics.Raycast(ray, out rayHit, 100)) // out - return처럼 반환값을 변수에 저장하는 키워드
            {
                Vector3 nextVec = rayHit.point - transform.position; // 상대적 위치 구함
                nextVec.y = 20; // 수류탄 위로 던짐
                
                // 수류탄 생성 후 던짐
                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
                Rigidbody rbGrenade = instantGrenade.GetComponent<Rigidbody>();
                rbGrenade.linearDamping = 1; // 마찰력
                rbGrenade.angularDamping = 1; // 회전 마찰력
                rbGrenade.AddForce(nextVec, ForceMode.Impulse);
                rbGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                // 수류탄을 던진 후 y를 0으로 만들고, 캐릭터가 던지는 방향을 바라보게 함
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
                
                hasGrenades--; // 던진 수류탄 감소
                grenades[hasGrenades].SetActive(false); // 해당 수류탄 비활성화
            }
        }
    }

    void Attack()
    {
        if (equipWeapon == null) return; // 무기를 장착중일 때만 실행되도록 현재 장비 체크

        // 딜레이에 시간을 더해주고 공격 가능 여부 확인
        fireDelay += Time.deltaTime; 
        isFireReady = equipWeapon.rate < fireDelay;

        // 공격
        if (fDown && isFireReady && !isDodge && !isSwap && !isReload)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");

            fireDelay = 0; // 0으로 초기화 (쿨타임)
        }
    }

    void Reload()
    {
        if (equipWeapon == null) return; // 장착 무기가 없을 때
        if (equipWeapon.type == Weapon.Type.Melee) return; // 근접 무기일 때
        if (ammo == 0) return; // 플레이어에게 총알이 아예 없을 때
        
        // 재장전
        if (rDown && !isJump && !isDodge && !isSwap && isFireReady && equipWeapon.curAmmo < equipWeapon.maxAmmo && !isReload)
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 3f);
        }
    }

    void ReloadOut()
    {
        // 소지한 탄을 고려해서 계산하기
        int reAmmo = ammo + equipWeapon.curAmmo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo - equipWeapon.curAmmo;
        equipWeapon.curAmmo += reAmmo; // 소지한 탄알 + 현재 탄알이 최대 탄알(30발) 보다 작으면 그 만큼만 장전하고, 반대로 크면 최대 탄알 - 현재 탄알 한 나머지만 장전한다.
        ammo -= reAmmo;
        isReload = false;
    }

    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap && !isReload) // 움직이는 도중 스페이스바 누르면 회피
        {
            dodgeVec = moveVec;
            speed *= 2; // 회피는 이동속도만 2배로 상승하도록 설정
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.5f); // 0.5초 후 회피를 다시 false로
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f; // 0.5를 곱하면 다시 speed가 1이 됨
        isDodge = false;
    }

    void Swap()
    {
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0)) // 해당 무기가 없거나, 같은 무기를 장착중일 때
            return; // 실행하면 안됨
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
            return;

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if ((sDown1 || sDown2 || sDown3) && !isJump && !isDodge && !isReload)
        {
            if (equipWeapon != null) equipWeapon.gameObject.SetActive(false); // 장작중인 무기가 있을 때만 비활성화 해야 에러가 나지 않음
            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true); // 무기 활성화

            anim.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }
    }

    void SwapOut()
    {
        isSwap = false;
    }

    void Interaction()
    {

        if (iDown && nearObj != null && !isJump && !isDodge && !isReload)
        {
            if (nearObj.tag == "Weapon")
            {
                Item item = nearObj.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObj);
            }
            else if (nearObj.tag == "Shop")
            {
                Shop shop = nearObj.GetComponent<Shop>(); // nearObj(tag가 Shop인 오브젝트)의 Shop 스크립트를 가져옴
                shop.Enter(this); // 자기자신(player)을 넣어줌
            }
        }
    }

    void FreezeRotation()
    {
        rb.angularVelocity = Vector3.zero; // angularVelocity - 물리 회전 속도
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, moveVec * 5, Color.green); // Scene화면에서 Ray를 봄 (시작위치, 방향 * 길이, 색깔)
        
        // (위치, 방향, 거리, 레이어마스크) 레이어마스크를 가진 물체에 닿으면 true로 바뀜
        isBorder = Physics.Raycast(transform.position, moveVec, 5, LayerMask.GetMask("Wall"));
    }

    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    void OnCollisionEnter(Collision collision)
    {
        // 점프 후 착지
        if (collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    // 아이템 처리
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch(item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxAmmo) ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin) coin = maxCoin;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth) health = maxHealth;
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if (hasGrenades > maxHasGrenades) hasGrenades = maxHasGrenades;
                    break;
            }
            Destroy(other.gameObject); // 먹은 아이템 파괴
        }
        else if (other.tag == "EnemyBullet") // 적 공격, 미사일에 피격 시
        {
            if (!isDamage) // 대미지를 입으면 무적 시간을 주기 위함
            {
                Bullet enemyBullet = other.GetComponent<Bullet>(); // 불릿 스크립트 재활용
                health -= enemyBullet.damage; // 적 미사일 맞고 피 깎임

                // 보스의 근접공격 오브젝트 이름으로 보스 공격을 인지
                bool isBossAttack = other.name == "Boss Melee Area";
                StartCoroutine(OnDamage(isBossAttack)); // 피격 로직. 보스의 근접공격임을 구분하기 위함
            }

            // 플레이어의 무적과 관계없이 투사체는 파괴 되도록 함
            // 근접 공격은 rigidbody가 없고, 미사일에만 rigidbody가 있음
            if (other.GetComponent<Rigidbody>() != null) // 미사일에 맞으면
                Destroy(other.gameObject); // 미사일 파괴
        }
    }

    IEnumerator OnDamage(bool isBossAttack)
    {
        isDamage = true;
        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.yellow; // 플레이어 피격 시 색상 변경
        }

        if (isBossAttack) // 보스 근접공격 이면
            rb.AddForce(transform.forward * -30, ForceMode.Impulse); // 플레이어 뒤로 밀어내기

        yield return new WaitForSeconds(1f);

        isDamage = false;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white; // 플레이어 피격 끝난 후 색상 변경
        }

        //if (isBossAttack) // 보스 근접공격 이면
            //rb.angularVelocity = Vector3.zero; // 계속 밀려나는 것을 방지
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon" || other.tag == "Shop")
        {
            nearObj = other.gameObject;
            //Debug.Log(nearObj.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObj = null;
        }
        else if (other.tag == "Shop")
        {
            Shop shop = nearObj.GetComponent<Shop>(); // Shop 스크립트 가져옴
            shop.Exit(); // 퇴장 함수 실행
            nearObj = null; // nearObj 초기화
        }
    }
}
