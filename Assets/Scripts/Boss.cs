using UnityEngine;

public class Boss : Enemy
{
    [SerializeField] GameObject missile;
    [SerializeField] Transform missilePortA;
    [SerializeField] Transform missilePortB;

    Vector3 lookVec; // 플레이어 움직임 예측 벡터 변수 생성 (보스는 지능이 더 높음)
    Vector3 tauntVec; // 점프 공격 위치
    bool isLook; // 점프 공격 시 플레이어를 바라보지 않고 그 방향 그대로 유지 하기 위한 플레그

    void Start()
    {
        isLook = true;
    }

    
    void Update()
    {
        if (isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            // 플레이어 입력 값으로 예측 벡터값 생성
            lookVec = new Vector3(h, 0, v) * 5f;
            transform.LookAt(target.position + lookVec); // 예측을 해서 플레이어를 바라봄
        }
    }
}
