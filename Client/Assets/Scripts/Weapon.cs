using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // 무기의 종류
    public enum Type
    {
        DoubleSword,
        TwoHandSword,
        SwordAndShield,
        MagicWand,
        Spear
    };

    public Type type;
    
    // 무기의 공격 범위
    public BoxCollider meleeArea;
    public int damage;

    private Player player;
    
    // 공격 모션 한번당 한번의 데미지만을 적용하기 위한 리스트 (리스트에 추가되어 있는 경우 이미 데미지를 가한 적)
    public List<Enemy> recentDamageList;

    public void Awake()
    {
        player = GetComponentInParent<Player>();
        recentDamageList = new List<Enemy>();
    }

    public void Use()
    {
        recentDamageList.Clear();
        
        if (type == Type.SwordAndShield)
        {
            StopCoroutine("SAS");
            StartCoroutine("SAS");
        }
        
        else if (type == Type.DoubleSword)
        {
            
        }
        
        else if (type == Type.Spear)
        {
            
        }
        
        else if (type == Type.MagicWand)
        {
            
        }
        
        else if (type == Type.TwoHandSword)
        {
            
        }
    }

    // Sword And Shield의 공격 코루틴
    IEnumerator SAS()
    {
        while (player.isAttack)
        {
            meleeArea.enabled = true;
            
            yield return new WaitForFixedUpdate();
        }

        meleeArea.enabled = false;
    }
}
