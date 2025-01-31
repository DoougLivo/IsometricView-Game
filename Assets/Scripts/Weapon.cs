using System.Collections;
using NUnit.Framework.Constraints;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range };
    public Type type;
    [SerializeField] int damage;
    public float rate;
    public int maxAmmo;
    public int curAmmo;

    [SerializeField] BoxCollider meleeArea; 
    [SerializeField] TrailRenderer trailEffect; 
    public Transform bulletPos;
    public GameObject bullet;
    public Transform bulletCasePos;
    public GameObject bulletCase;

    public void Use()
    {
        if (type == Type.Melee) 
        {
            StopCoroutine("Swing"); 
            StartCoroutine("Swing");
        } else if (type == Type.Range && curAmmo > 0) 
        {
            curAmmo--; // 총알 1개씩 소모됨
            StartCoroutine("Shot");
        }
    }

    IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.1f);
        meleeArea.enabled = true; 
        trailEffect.enabled = true; 

        yield return new WaitForSeconds(0.5f);
        meleeArea.enabled = false; 

        yield return new WaitForSeconds(0.3f);
        trailEffect.enabled = false;
    }

    IEnumerator Shot()
    {
        // 총알 발사
        GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRb = instantBullet.GetComponent<Rigidbody>();
        //bulletRb.linearVelocity = bulletPos.forward * 50;
        bulletRb.AddForce(bulletPos.forward * 50, ForceMode.Impulse) ;
        
        yield return null;

        // 탄피 배출
        GameObject instantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRb = instantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
        caseRb.AddForce(caseVec, ForceMode.Impulse);
        caseRb.AddTorque(Vector3.up * 5, ForceMode.Impulse); // 회전
    }
}
