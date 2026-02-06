using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class BossEnemy : MonoBehaviour
{
    [Header("HP Settings")]
    public float EnemyHP = 20f;

    [Header("Attack Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float bulletSpeed = 10f;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 10f; // 보스는 더 넓은 감지 범위
    [SerializeField] private LayerMask playerLayer;

    [Header("Death Settings")]
    [SerializeField] private float deathDestroyDelay = 2f;

    [Header("Battle Control")]
    [SerializeField] private bool battleStarted = false; // 전투 시작 여부
    public GameObject GameWinUI;

    private Rigidbody2D rb;
    private Animator anim;
    private bool facingRight = false;
    private Transform player;
    private float lastAttackTime;
    private bool isDead = false;
    

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.parent = transform;
            firePointObj.transform.localPosition = new Vector3(1f, 0f, 0);
            firePoint = firePointObj.transform;
        }
    }

    void Update()
    {
        // 전투가 시작되지 않았거나 죽었으면 아무것도 안 함
        if (!battleStarted || isDead) return;

        CheckDeath();
        DetectPlayer();

        if (player != null)
        {
            AttackPlayer();
        }
    }

    // AlertTextUI에서 호출할 함수
    public void StartBattle()
    {
        battleStarted = true;
    }

    public bool IsBattleStarted()
    {
        return battleStarted;
    }

    void CheckDeath()
    {
        if (EnemyHP <= 0 && !isDead)
        {
            EnemyDie();
        }
    }

    void EnemyDie()
    {
        isDead = true;

        if (anim != null)
        {
            anim.SetTrigger("isDead");
        }

        AudioManager.instance.PlaySfx(AudioManager.Sfx.BossDie); // 보스용 사운드


        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = false;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        StartCoroutine(GameWinRoutine());
        Destroy(gameObject, deathDestroyDelay);

    }

    IEnumerator GameWinRoutine()
    {
        yield return new WaitForSeconds(1.9f);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.GameVictory);
        GameWinUI.SetActive(true);
    }

    void DetectPlayer()
    {
        Collider2D detectedPlayer = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);

        if (detectedPlayer != null)
        {
            player = detectedPlayer.transform;
        }
        else
        {
            player = null;
        }
    }

    void AttackPlayer()
    {
        // 이동 멈춤
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        // 플레이어 방향 보기
        if (player != null)
        {
            float directionToPlayer = Mathf.Sign(player.position.x - transform.position.x);
            if ((directionToPlayer > 0 && !facingRight) || (directionToPlayer < 0 && facingRight))
            {
                Flip();
            }
        }

        // 쿨다운이 지나면 공격
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Shoot();
            lastAttackTime = Time.time;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        // AudioManager.instance.PlaySfx(AudioManager.Sfx.BossShoot); // 보스용 사운드
        AudioManager.instance.PlaySfx(AudioManager.Sfx.EnemyShoot);

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

        if (bulletRb != null)
        {
            float direction = facingRight ? 1 : -1;
            bulletRb.linearVelocity = new Vector2(direction * bulletSpeed, 0);

            // 총알 스프라이트 방향 설정
            if (facingRight)
            {
                Vector3 bulletScale = bullet.transform.localScale;
                bulletScale.x *= -1;
                bullet.transform.localScale = bulletScale;
            }
        }

        Destroy(bullet, 5f); // 보스 총알은 더 오래 유지
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        if (firePoint != null)
        {
            Vector3 firePos = firePoint.localPosition;
            firePos.x *= -1;
            firePoint.localPosition = firePos;
        }
    }

    void OnDrawGizmos()
    {
        // 감지 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}