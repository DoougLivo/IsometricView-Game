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
    bool rDown;
    bool iDown;

    bool isJump;
    bool isDodge;
    bool isSwap;
    bool isReload;
    bool isFireReady = true;

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

    GameObject nearObj;
    Weapon equipWeapon; // 장착중인 무기

    [SerializeField] GameObject[] weapons;
    [SerializeField] bool[] hasWeapons;
    [SerializeField] GameObject[] grenades;
    [SerializeField] int hasGrenades;

    [SerializeField] Camera followCamera;

    int equipWeaponIndex = -1; // 초기값 설정
    float fireDelay;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>(); // Animator가 자식 오브젝트에 있기 때문
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

        rb.angularVelocity = Vector3.zero;
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal"); // 정수로 받음
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetButton("Fire1");
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
        
        transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero); // moveVec이 0이 아닐때 (움직일때)
        anim.SetBool("isWalk", wDown);
    }

    void Turn()
    {
        // 키보드에 의한 회전
        transform.LookAt(transform.position + moveVec); // 나아가는 방향으로 바라보게 함

        // 마우스에 의한 회전
        Ray ray = followCamera.ScreenPointToRay(Input.mousePosition); // 마우스 방향으로 레이를 쏨
        RaycastHit rayHit;
        if (Physics.Raycast(ray, out rayHit, 100))
        {

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
        if (rDown && !isJump && !isDodge && !isSwap && isFireReady && equipWeapon.curAmmo < equipWeapon.maxAmmo)
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 3f);
        }
    }

    void ReloadOut()
    {
        // 소지한 탄을 고려해서 계산하기
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
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
        }
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
        if (other.gameObject.tag == "Item")
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
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon")
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
    }
}
