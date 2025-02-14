using UnityEngine;
using UnityEngine.AI; // nav 사용 시 필요

public class BossMissile : Bullet
{
    [SerializeField] Transform target;
    NavMeshAgent nav;

    void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
    }
    void Update()
    {
        nav.SetDestination(target.position);
    }
}
