using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    float hAxis;
    float vAxis;

    bool wDown;
    bool jDown;
    bool iDown;

    bool isJump;
    bool isDodge;
    bool isSwap;

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
    GameObject equipWeapon; // 장착중인 무기

    [SerializeField] GameObject[] weapons;
    [SerializeField] bool[] hasWeapons;
    [SerializeField] GameObject[] grenades;
    [SerializeField] int hasGrenades;

    int equipWeaponIndex = -1; // 초기값 설정
    

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

        /*if (isSwap) // 스왑할 때 움직이지 못하게 함
            moveVec = Vector3.zero;
        */
        transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero); // moveVec이 0이 아닐때 (움직일때)
        anim.SetBool("isWalk", wDown);
    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec); // 나아가는 방향으로 바라보게 함
    }

    void Jump()
    {
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap) // 움직이지 않을 때 스페이스바 누르면 점프
        {
            rb.AddForce(Vector3.up * jPower, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap) // 움직이는 도중 스페이스바 누르면 회피
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

        if ((sDown1 || sDown2 || sDown3) && !isJump && !isDodge)
        {
            if (equipWeapon != null) equipWeapon.SetActive(false); // 장작중인 무기가 있을 때만 비활성화 해야 에러가 나지 않음
            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex];
            equipWeapon.SetActive(true); // 무기 활성화

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
        
        if (iDown && nearObj != null && !isJump && !isDodge)
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
