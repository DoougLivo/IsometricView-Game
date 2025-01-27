using System.Collections;
using NUnit.Framework.Constraints;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range };
    public Type type;
    [SerializeField] int damage; // 데미지
    public float rate; // 공격 속도
    [SerializeField] BoxCollider meleeArea; // 공격 범위
    [SerializeField] TrailRenderer trailEffect; // 공격 잔상

    public void Use()
    {
        if (type == Type.Melee) // 근접 공격
        {
            StopCoroutine("Swing"); // 루틴 멈춘 후 시작
            StartCoroutine("Swing");
        }
    }

    IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.1f);
        meleeArea.enabled = true; // 공격 콜라이더 활성화
        trailEffect.enabled = true; // 공격 잔상 활성화

        yield return new WaitForSeconds(0.5f);
        meleeArea.enabled = false; // 공격 범위 끄기

        yield return new WaitForSeconds(0.3f);
        trailEffect.enabled = false; // 공격 잔상 끄기
    }
}
