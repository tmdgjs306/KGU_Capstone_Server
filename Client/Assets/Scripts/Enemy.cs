using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public float maxHealth;

    public float curHealth;

    private Rigidbody rigid;
    private Animator anim;
    private BoxCollider boxCollider;

    private Vector3 moveVec;
    public bool isHit;
    private bool isHitReady = true;
    private bool isAttack;

    public Player player;
    public float moveSpeed;
    public float attackRange;
    public float attackDelay;
    public int attackDamage;

    public GameObject hpBarPrefab;
    public Vector3 hpBarOffset;
    
    private Canvas hpCanvas;
    private GameObject hpBar;
    private Slider hpSlider;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider>();
    }

    // Start is called before the first frame update
    void Start()
    {
        HPbarInit();
    }

    // Update is called once per frame
    void Update()
    {
        if (InRange()) Attack();
        else Move();
        
        if (curHealth <= 0)
        {
            Destroy();
        }

        //if (maxHealth == curHealth) hpBar.SetActive(false);
        //else
        //{
        //    hpBar.SetActive(true);
        //   HpSliderUpdate();
        //}
    }

    void Move()
    {
        if (!isHit && !isAttack)
        {
            moveVec = (player.transform.position - transform.position).normalized;
            transform.LookAt(player.transform);
            rigid.MovePosition(transform.position + moveSpeed * Time.fixedDeltaTime * moveVec);
        }
    }

    // attackRange(사거리)가 플레이어와 적 위치 차이 벡터보다 클 시 공격 가능
    void Attack()
    {
        if (!isHit && isHitReady)
        {
            anim.SetTrigger("isAttack");
            player.Hit(attackDamage);
            
            StopCoroutine("Attacking");
            StartCoroutine("Attacking", 0.5f);
            StartCoroutine(AttackDelay());
        }
    }

    bool InRange()
    {
        return (transform.position - player.transform.position).magnitude <= attackRange;
    }
    
    // 공격 도중 재공격 및 다른 모션 불가
    IEnumerator Attacking(float time)
    {
        isAttack = true;
        yield return new WaitForSeconds(time);
        
        isAttack = false;
    }

    // 공격 딜레이
    IEnumerator AttackDelay()
    {
        isHitReady = false;
        yield return new WaitForSeconds(attackDelay);

        isHitReady = true;
    }

    // Hp가 0이 될 시 Destroy, 죽는 애니메이션 실행 및 콜라이더 제거 후 일정 시간뒤에 삭제(추가 예정)
    void Destroy()
    {
        Destroy(hpBar);
        GameObject.Destroy(gameObject);
    }

    // 피격 시 웨폰의 최근 공격 리스트에 추가
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Sword")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            if (!weapon.recentDamageList.Contains(this)) {
                anim.SetTrigger("isHit");
                weapon.recentDamageList.Add(this);
                curHealth -= weapon.damage;

                StopCoroutine("Hit");
                StartCoroutine("Hit");
            }
        }
    }

    // 피격도중 다른 모션 불가
    IEnumerator Hit()
    {
        isHit = true;
        yield return new WaitForSeconds(1.0f);
        
        isHit = false;
    }

    void HPbarInit()
    {
        hpCanvas = GameObject.Find("HP Canvas").GetComponent<Canvas>();
        hpBar = Instantiate<GameObject>(hpBarPrefab, hpCanvas.transform);
        hpSlider = hpBar.GetComponentInChildren<Slider>();

        var hpbar = hpBar.GetComponent<HPbar>();
        hpbar.targetTr = gameObject.transform;
        hpbar.offset = hpBarOffset;
    }

    //void HpSliderUpdate()
   // {
        //hpSlider.value = Mathf.Lerp(hpSlider.value, curHealth / maxHealth, Time.deltaTime * 10);
    //}
}
