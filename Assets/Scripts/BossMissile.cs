using UnityEngine;
using UnityEngine.AI; // nav 사용 시 필요

public class BossMissile : Bullet
{
    public Transform target;
    NavMeshAgent nav;

    void Awake()
    {
        nav = GetComponent<NavMeshAgent>();

        Destroy(gameObject, 8f); // 4초 뒤 파괴
    }
    void Update()
    {
        nav.SetDestination(target.position);
    }
}
