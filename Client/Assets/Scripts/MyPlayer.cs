using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;

public class MyPlayer : MonoBehaviour
{
    public string name;
    public float moveSpeed;
    public float rollCoolTime = 3.0f;
    public Weapon equipWeapon; // 장착중인 무기
    public float attackDelay = 0.3f; // 공격 딜레이
    public int maxHealth; // 최대 체력
    public int curHealth; // 현재 체력

    private Animator anim; // 플레이어 객체의 Animator
    private Rigidbody rigid; // 플레이어 객체의 rigidbody
    
    public float hAxis;
    public float vAxis;
    
    private bool rDown; // 구르기 버튼
    private bool aDown; // 공격 버튼

    public bool isRollReady = true; // 구르기 가능 여부
    public bool isRoll; // 구르기 작동 여부
    private bool isAttackReady = true; // 공격 가능 여부
    private bool isHit;
    private bool isInvincible;
    public bool isAttack; // 공격 작동 여부
    
    private Vector3 moveVec; // 이동방향
    private Vector3 rollVec; // 구르기 할 때의 방향

    private PositionInfo PositionInfo = new PositionInfo();
    public int Id { get; set; }
    
    void Start()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        Roll();
        Attack();
        PlayerCamera.targetTransform = GameObject.Find(name).transform;
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        rDown = Input.GetKeyDown(KeyCode.Space);
        aDown = Input.GetMouseButton(0);
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;
        
        Vector3 prePosition = transform.position;
        
        // 구르기중이라면 방향 고정
        if (isRoll)
        {
            moveVec = rollVec;
        }

        // 공격중이라면 이동 불가
        else if (isAttack)
        {
            moveVec = Vector3.zero;
        }
        anim.SetBool("isRun", moveVec != Vector3.zero);
        rigid.MovePosition(transform.position + moveSpeed * Time.fixedDeltaTime * moveVec);
        if (moveVec != Vector3.zero || transform.position != prePosition)
        {
            C_Move movePacket = new C_Move();
            PositionInfo.PosX = (int)hAxis;
            PositionInfo.PosZ = (int)vAxis;
            movePacket.PosInfo = PositionInfo;
            Managers.Network.Send(movePacket);
            Debug.Log("Packet_Send!!!");
        }
    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec);
    }

    void Roll()
    {
        if (rDown && !isRoll && isRollReady && !isAttack && !isHit)
        {
            rollVec = moveVec; // 구르기시 방향 저장
            
            anim.SetTrigger("doRoll");
            
            isRoll = true;
            isRollReady = false;
            
            StartCoroutine(RollCoolTime(rollCoolTime));
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

    // 구르기 쿨타임 적용
    IEnumerator RollCoolTime(float time)
    {
        yield return new WaitForSeconds(time);
        
        isRollReady = true;
    }

    void Attack()
    {
        if (aDown && isAttackReady && moveVec == Vector3.zero && !isRoll && !isHit && !isInvincible)
        {
            anim.SetTrigger("comboAttack");
            
            StopCoroutine("Attacking");
            StartCoroutine("Attacking", 0.5f);
            StartCoroutine(AttackDelay(attackDelay));
            
            equipWeapon.Use();
        }
    }
    // 공격 도중 재공격 및 다른 모션 불가
    IEnumerator Attacking(float time)
    {
        isAttack = true;
        yield return new WaitForSeconds(time);
        
        isAttack = false;
    }
    
    // 공격 딜레이
    IEnumerator AttackDelay(float time)
    {
        isAttackReady = false;
        yield return new WaitForSeconds(time);
        
        isAttackReady = true;
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
