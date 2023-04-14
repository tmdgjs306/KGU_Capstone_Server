using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    public float moveSpeed;
    public Weapon equipWeapon; // 장착중인 무기
    public int maxHealth; // 최대 체력
    public int curHealth; // 현재 체력

    private Animator anim; // 플레이어 객체의 Animator
    private Rigidbody rigid; // 플레이어 객체의 rigidbody
    
    public float hAxis;
    public float vAxis;
    
    public bool rDown; // 구르기 버튼
    public bool aDown; // 공격 버튼
    
    public bool isRoll; // 구르기 작동 여부
    private bool isAttackReady = true; // 공격 가능 여부
    private bool isHit;
    private bool isInvincible;
    public bool isAttack; // 공격 작동 여부
    
    
    private Vector3 rollVec; // 구르기 할 때의 방향
    
    public Vector3 curPos; //
    public Vector3 moveVec;// 이동방향
    
    void Start()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
    }

    public void Update()
    {
        MoveToFixedPoint();;
        Turn();
        Roll();
        Attack();
        rDown = false;
        aDown = false;
    }
    
    public void MoveToFixedPoint()
    {
        if (isRoll)
        {
            moveVec = rollVec;
        }
        else if (isAttack)
        {
            moveVec = Vector3.zero;
        }
        anim.SetBool("isRun", moveVec != Vector3.zero);
        rigid.MovePosition(transform.position + 5 * Time.fixedDeltaTime * moveVec);
    }
 
    void Turn()
    {
        transform.LookAt(transform.position + moveVec);
    }

    void Roll()
    {
        if (rDown && !isRoll && !isAttack && !isHit)
        {
            rollVec = moveVec; // 구르기시 방향 저장
            
            anim.SetTrigger("doRoll");
            
            isRoll = true;

            StartCoroutine(Rolling(0.833f));
        }
    }

    // 구르기 도중 이동속도 2배, 방향전환 불가능 기능
    IEnumerator Rolling(float time)
    {
        moveSpeed *= 1.5f;
        yield return new WaitForSeconds(time);
        
        isRoll = false;
        moveSpeed /= 1.5f;
    }
    

    void Attack()
    {
        if (aDown)
        {
            anim.SetTrigger("comboAttack");
            //equipWeapon.Use();
        }
    }

    public void Hit(int damage)
    {
        if (!isRoll && !isHit)
        {
            StartCoroutine(HitAnimDelay(0.2f, damage));
            StartCoroutine(Hitting(0.5f));
            StartCoroutine(Invincible(1.0f));
        }
    }

    // 일정 시간 후 피격 애니메이션 실행
    IEnumerator HitAnimDelay(float time, int damage)
    {
        yield return new WaitForSeconds(time);
            
        anim.SetTrigger("isHit");
        curHealth -= damage;
    }

    // 공격 받는 중
    IEnumerator Hitting(float time)
    {
        isHit = true;
        yield return new WaitForSeconds(time);

        isHit = false;
    }
    
    // 피격 후 무적시간
    IEnumerator Invincible(float time)
    {
        isInvincible = true;
        yield return new WaitForSeconds(time);

        isInvincible = false;
    }
}
