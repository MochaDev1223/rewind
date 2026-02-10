using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float flySpeed = 15f;
    public float returnSpeed = 25f;
    public float maxDistance = 25f;
    public float damage = 1f; // 데미지를 변수로 설정

    private Rigidbody2D rigid;
    private Transform player;
    private bool isReturning = false;
    private bool isEmbedded = false;
    private Collider2D col;
    public GameObject bulletEffect;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    public void Launch(Vector2 direction)
    {
        Vector2 normalizedDirection = direction.normalized;
        rigid.linearVelocity = normalizedDirection * flySpeed;
        RotateTowards(normalizedDirection);
    }

    void Update()
    {
        if (isReturning)
        {
            if (player == null)
            {
                Destroy(gameObject);
                return;
            }

            // 플레이어에게 부드럽게 돌아오기
            transform.position = Vector2.MoveTowards(transform.position, player.position, returnSpeed * Time.deltaTime);
            Vector2 dirToPlayer = (player.position - transform.position).normalized;
            RotateTowards(dirToPlayer);

            if (Vector2.Distance(transform.position, player.position) < 0.5f)
            {
                Destroy(gameObject); // 회수 완료
            }
        }
        else if (!isEmbedded)
        {
            // 거리 초과 시 자동 회수 시작
            if (player != null && Vector2.Distance(transform.position, player.position) > maxDistance)
            {
                StartReturn();
            }
        }
    }

    void RotateTowards(Vector2 direction)
    {
        if (direction == Vector2.zero) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isReturning) return; // 돌아오는 중에는 충돌 무시

        // Enemy 태그 체크 (일반 적)
        if (other.CompareTag("Enemy"))
        {
            HandleEnemyHit(other);
        }
        // Boss 태그 체크 (보스)
        else if (other.CompareTag("Boss"))
        {
            HandleBossHit(other);
        }
        // Ground 태그 체크
        else if (other.CompareTag("Ground"))
        {
            HandleGroundHit(other);
        }
    }

    // 일반 적 처리
    void HandleEnemyHit(Collider2D enemyCollider)
    {
        Enemy enemy = enemyCollider.GetComponent<Enemy>();

        if (enemy != null)
        {
            // 데미지 적용
            enemy.EnemyHP -= damage;

            // Hit 애니메이션 재생 (죽지 않았을 때만)
            if (enemy.EnemyHP > 0)
            {
                Animator enemyAnim = enemy.GetComponent<Animator>();
                if (enemyAnim != null)
                {
                    enemyAnim.SetTrigger("Hit");
                }
            }
        }

        // 이펙트 생성
        SpawnEffect();

        // 적에게는 박히지 않고 바로 리턴
        StartReturn();
    }

    // 보스 처리
    void HandleBossHit(Collider2D bossCollider)
    {
        BossEnemy boss = bossCollider.GetComponent<BossEnemy>();

        if (boss != null && boss.IsBattleStarted())
        {
            // 전투 시작 후에만 데미지 적용
            boss.EnemyHP -= damage;
        }

        // 이펙트 생성
        SpawnEffect();

        // 보스에게도 박히지 않고 바로 리턴
        StartReturn();
    }

    // 땅 처리
    void HandleGroundHit(Collider2D groundCollider)
    {
        // 이펙트 생성
        SpawnEffect();

        // 땅에 박힘
        Embed(groundCollider.transform);
    }

    // 이펙트 생성 헬퍼 함수
    void SpawnEffect()
    {
        if (bulletEffect != null)
        {
            GameObject effect = Instantiate(bulletEffect, transform.position, Quaternion.identity);
            Destroy(effect, 0.2f);
        }
    }

    void Embed(Transform target)
    {
        isEmbedded = true;
        rigid.linearVelocity = Vector2.zero;
        rigid.bodyType = RigidbodyType2D.Kinematic;
        transform.SetParent(target, true);
    }

    public void StartReturn()
    {
        isReturning = true;
        isEmbedded = false;
        transform.SetParent(null);

        // Trigger로 바꿔서 벽을 뚫고 오게 함
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
}