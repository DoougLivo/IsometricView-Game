using UnityEngine;

public class Orbit : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float orbitSpeed;
    Vector3 offset;

    void Start()
    {
        offset = transform.position - target.position;
    }

    void Update()
    {
        // RotateAround - 타겟 주위를 회전하는 함수 (타겟위치, 축, 속도)
        transform.position = target.position + offset; // 위치를 직접 지정함
        transform.RotateAround(target.position, Vector3.up, orbitSpeed * Time.deltaTime);

        // RotateAround 후의 위치를 가지고 목표와의 거리를 유지
        offset = transform.position - target.position;
        // 목표 지정 -> 돌고 -> 다시 지정 -> 반복
    }
}
