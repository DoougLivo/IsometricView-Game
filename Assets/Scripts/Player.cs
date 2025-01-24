using UnityEngine;

public class Player : MonoBehaviour
{
    float hAxis;
    float vAxis;
    bool wDown;
    bool jDown;

    bool isJump;
    bool isDodge;

    [SerializeField] int jPower;
    [SerializeField] float speed;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rb;
    Animator anim;

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
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal"); // 정수로 받음
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized; // 방향 값이 1로 보정된 벡터

        if (isDodge) // 회피 중일 때
        {
            moveVec = dodgeVec; // 움직임 벡터를 회피 벡터로 바꿈 (회피중엔 방향 못바꾸게 함)
        }

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
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge) // 움직이지 않을 때 스페이스바 누르면 점프
        {
            rb.AddForce(Vector3.up * jPower, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge) // 움직이는 도중 스페이스바 누르면 회피
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

    void OnCollisionEnter(Collision collision)
    {
        // 점프 후 착지
        if (collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }
}
