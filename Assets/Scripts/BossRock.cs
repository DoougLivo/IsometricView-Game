using System.Collections;
using UnityEngine;

public class BossRock : Bullet
{
    public Rigidbody rb;
    [SerializeField] float angularPower = 2; // 회전 파워
    [SerializeField] float scaleValues = 0.1f; // 크기 숫자 값
    [SerializeField] bool isShoot;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(GainPowerTimer()); // 타이머
        StartCoroutine(GainPower()); // 기 모으기

        rb.useGravity = false;
        //Destroy(gameObject, 8f); // 8초 뒤 돌 파괴
    }

    IEnumerator GainPowerTimer() // 기 모은 후 발사하기 위한 코루틴
    {
        yield return new WaitForSeconds(2.2f);
        isShoot = true;
        rb.useGravity = true;
    }

    IEnumerator GainPower()
    {
        while (!isShoot)
        {
            angularPower += 0.03f; // 점점 회전 파워가 증가함
            scaleValues += 0.005f; // 점점 크기가 커짐
            transform.localScale = Vector3.one * scaleValues; // 증가된 크기 적용
            rb.AddTorque(transform.right * angularPower, ForceMode.Acceleration); // 점점 속도를 올리기 위해 액셀레이션 씀
            yield return null; // while 문에는 이걸 넣어야 게임이 정지하지 않음
        }
    }
}
