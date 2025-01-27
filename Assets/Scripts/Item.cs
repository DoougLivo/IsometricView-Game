using UnityEngine;

public class Item : MonoBehaviour
{
    float rotateSpeed = 50f;
    public enum Type { Ammo, Coin, Grenade, Heart, Weapon}; // 열거형 타입 (타입 이름 지정 필요)
    public Type type; // 아이템 종류와 값을 저장할 변수 선언
    public int value;

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }
}
