using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class Player : MonoBehaviour
{
    [Header("Info")]
    // public HealthBar healthBar;
    public Image healthBar;
    public float hpValue = 1;
    public float currentHealth = 4f;
    public float maxHealth = 4f;
    private bool isDead = false;
    public GameObject GameOverUI;


    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Jump")]
    public float jumpPower = 12f;          // 점프 힘
    public float coyoteTime = 0.15f;       // 코요테 타임 (땅 떠난 후 점프 허용 시간)
    public float jumpBufferTime = 0.15f;   // 점프 입력 버퍼 (미리 누른 점프 유지 시간)

    [Header("Ground Check")]
    public float rayDistance = 0.25f;      // 바닥 체크 레이 길이

    [Header("Combat")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Image EnergyBar;
    public float EnergyValue = 1;
    public float maxEnergy = 5f;
    public float currentEnergy = 5f;


    SpriteRenderer spriteRenderer;
    Animator anim;
    Rigidbody2D rigid;
    CapsuleCollider2D capsule;

    float horiz;
    // 상태 변수
    bool isGrounded = false;

    // 코요테 타임 & 입력 버퍼 타이머
    float coyoteTimer = 0f;
    float jumpBufferTimer = 0f;


    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        capsule = GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        if (GameManager.isPaused || isDead) return;

        horiz = Input.GetAxisRaw("Horizontal");

        // 점프 입력을 "즉시 점프"하지 않고
        // 타이머에 저장해두는 게 핵심 (Jump Buffer)
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimer = jumpBufferTime;
        }

        // 사격 (마우스 좌클릭)
        if (Input.GetMouseButtonDown(0))
        {
            Fire();
        }

        // 탄환 회수 (마우스 우클릭)
        if (Input.GetMouseButtonDown(1))
        {
            ReturnAllBullets();
        }

        // HP바 UI
        if (currentHealth >= maxHealth)
        {
            hpValue = 1;
            healthBar.fillAmount = hpValue;
        }

        else if (currentHealth <= 0)
        {
            hpValue = 0;
            healthBar.fillAmount = hpValue;

            // 사망 처리 추가
            CheckDeath();

        }

        else
        {
            hpValue = currentHealth / maxHealth;
            healthBar.fillAmount = hpValue;
        }

        // 에너지바 UI
        if (currentEnergy >= maxEnergy)
        {
            EnergyValue = 1;
            EnergyBar.fillAmount = EnergyValue;
        }
        else if (currentEnergy <= 0)
        {
            EnergyValue = 0;
            EnergyBar.fillAmount = EnergyValue;

        }
        else
        {
            EnergyValue = currentEnergy / maxEnergy;
            EnergyBar.fillAmount = EnergyValue;
        }

    }

    void CheckDeath()
    {
        if (currentHealth <= 0 && !isDead)
        {
            PlayerDie();
        }
    }

    void PlayerDie()
    {
        isDead = true;
        if (currentHealth <= 0)
        {
            anim.SetTrigger("Dead");
        }
        if (rigid != null)
        {
            rigid.linearVelocity = Vector2.zero;
            rigid.bodyType = RigidbodyType2D.Kinematic; // 물리 엔진 영향 제거
            rigid.simulated = false;
        }
        StartCoroutine(GameOverRoutine());

    }

    IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        GameOverUI.SetActive(true);
    }

    void Fire()
    {
        if (currentEnergy <= 0)
        {
            return;
        }
        else
        {
            // 마우스 위치로 방향 계산
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = (Vector2)(mousePos - firePoint.position).normalized;

            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            bulletObj.GetComponent<Bullet>().Launch(dir);
            anim.SetTrigger("Shoot");
            currentEnergy--;
            AudioManager.instance.PlaySfx(AudioManager.Sfx.PlayerShoot);
        }
    }

    void ReturnAllBullets()
    {
        if (currentEnergy == maxEnergy)
        {
            return;
        }
        else
        {
            anim.SetTrigger("Rewind");
            // 씬에 있는 모든 탄환을 찾아 돌아오게 함 (최적화는 나중에!)
            Bullet[] bullets = FindObjectsByType<Bullet>(FindObjectsSortMode.None);
            foreach (Bullet b in bullets)
            {
                b.StartReturn();
            }

            currentEnergy = maxEnergy;
        }

    }

    void FixedUpdate()
    {
        if (isDead) return;

        rigid.linearVelocity = new Vector2(horiz * moveSpeed, rigid.linearVelocity.y);


        // 방향 전환
        if (Input.GetButton("Horizontal"))
        {
            spriteRenderer.flipX = horiz < 0f;
        }

        // // 걷기 애니메이션
        anim.SetBool("isWalk", Mathf.Abs(rigid.linearVelocity.x) > 0.2f);
        // if (Mathf.Abs(rigid.linearVelocity.x) < 0.2)
        // {
        //     anim.SetBool("isWalk", false);
        // }
        // else
        //     anim.SetBool("isWalk", true);

        // Ground Check
        Vector2 rayOrigin = (Vector2)transform.position
                  + Vector2.up * capsule.offset.y
                  - Vector2.up * (capsule.size.y * 0.5f - 0.02f);
        RaycastHit2D hit = Physics2D.Raycast(
            rayOrigin,
            Vector2.down,
            rayDistance,
            LayerMask.GetMask("Ground")
        );

        Debug.DrawRay(rayOrigin, Vector2.down * rayDistance, Color.green);

        bool wasGrounded = isGrounded;
        isGrounded = hit.collider != null;

        /* =============================
        * 4. 코요테 타임 처리
        * ============================= */

        if (isGrounded)
        {
            // 땅에 있으면 코요테 타이머를 최대값으로 리셋
            coyoteTimer = coyoteTime;

            if (!wasGrounded)
            {
                // 착지 순간
                anim.SetBool("isJump", false);
            }
        }
        else
        {
            // 공중에 있으면 코요테 타이머 감소
            coyoteTimer -= Time.fixedDeltaTime;
        }
        /* =============================
        * 5. 점프 입력 버퍼 감소
        * ============================= */
        jumpBufferTimer -= Time.fixedDeltaTime;


        /* =============================
         * 6. 실제 점프 실행 조건
         * ============================= */
        /*
         * ✔ 점프 입력이 남아 있고
         * ✔ 코요테 타임이 남아 있다면
         * → 점프 실행
         */
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            // 기존 Y속도를 초기화하고 점프
            rigid.linearVelocity = new Vector2(
                rigid.linearVelocity.x,
                0f
            );

            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);

            // 타이머 소모
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;

            anim.SetBool("isJump", true);
        }

    }
}
