using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3.5f;
    
    [Header("Patrol Settings")]
    [SerializeField] private float groundCheckDistance = 1f;
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckOffset = new Vector2(0.7f, -0.5f);
    
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private LayerMask playerLayer;
    
    [Header("Attack Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float bulletSpeed = 10f;
    
    private Rigidbody2D rb;
    Animator anim;
    private bool facingRight = false;
    private Transform player;
    private float lastAttackTime;
    private float flipCooldown = 2f;
    private float lastFlipTime = -1f;
    
    private enum EnemyState { Patrol, Chase, Attack }
    private EnemyState currentState = EnemyState.Patrol;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.parent = transform;
            firePointObj.transform.localPosition = new Vector3(0.5f, 0f, 0);
            firePoint = firePointObj.transform;
        }
    }

    void Update()
    {
        DetectPlayer();
        
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                ChasePlayer();
                break;
            case EnemyState.Attack:
                AttackPlayer();
                break;
        }
    }

    void Patrol()
    {
        float direction = facingRight ? 1 : -1;
        
        // 캐릭터 발 위치에서 앞쪽으로 체크
        Vector2 frontCheckPos = (Vector2)transform.position + new Vector2(direction * groundCheckOffset.x, groundCheckOffset.y);
        
        // 앞쪽에 땅이 있는지 체크
        RaycastHit2D groundHit = Physics2D.Raycast(frontCheckPos, Vector2.down, groundCheckDistance, groundLayer);
        
        // 캐릭터 중심에서 앞쪽 벽 체크
        Vector2 wallCheckPos = (Vector2)transform.position + new Vector2(0, 0.2f);
        RaycastHit2D wallHit = Physics2D.Raycast(wallCheckPos, Vector2.right * direction, wallCheckDistance, groundLayer);
        
        // 방향 전환 쿨다운 체크
        bool canFlip = Time.time >= lastFlipTime + flipCooldown;
        
        // 땅이 없거나 벽이 있으면 방향 전환
        if (canFlip && (!groundHit.collider || wallHit.collider))
        {
            Flip();
            lastFlipTime = Time.time;
        }
        
        // ✅ Unity 6: linearVelocity 사용
        direction = facingRight ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        if (anim != null)
        {
            anim.SetBool("isWalk", true);
        }
    }

    void ChasePlayer()
    {
        if (player == null)
        {
            currentState = EnemyState.Patrol;
            return;
        }
        
        float directionToPlayer = Mathf.Sign(player.position.x - transform.position.x);
        
        // 플레이어 방향의 땅 체크
        Vector2 frontCheckPos = (Vector2)transform.position + new Vector2(directionToPlayer * groundCheckOffset.x, groundCheckOffset.y);
        RaycastHit2D groundHit = Physics2D.Raycast(frontCheckPos, Vector2.down, groundCheckDistance, groundLayer);
        
        // 플레이어 방향의 벽 체크
        Vector2 wallCheckPos = (Vector2)transform.position + new Vector2(0, 0.2f);
        RaycastHit2D wallHit = Physics2D.Raycast(wallCheckPos, Vector2.right * directionToPlayer, wallCheckDistance, groundLayer);
        
        // 플레이어 방향으로 회전
        if ((directionToPlayer > 0 && !facingRight) || (directionToPlayer < 0 && facingRight))
        {
            Flip();
        }
        
        // ✅ Unity 6: linearVelocity 사용
        if (groundHit.collider && !wallHit.collider)
        {
            rb.linearVelocity = new Vector2(directionToPlayer * chaseSpeed, rb.linearVelocity.y);
            // ✅ 추격 중 걷는 애니메이션
            if (anim != null)
            {
                anim.SetBool("isWalk", true);
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
             // ✅ 멈췄을 때 걷기 애니메이션 끄기
            if (anim != null)
            {
                anim.SetBool("isWalk", false);
            }
        }
    }

    void AttackPlayer()
    {
        // ✅ Unity 6: linearVelocity 사용
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        
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
        anim.SetTrigger("Attack");

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        
        if (bulletRb != null)
        {         
            float direction = facingRight ? 1 : -1;
            // ✅ Unity 6: linearVelocity 사용
            bulletRb.linearVelocity = new Vector2(direction * bulletSpeed, 0);
            
            // 총알 스프라이트 방향 설정
            if (facingRight)
            {
                Vector3 bulletScale = bullet.transform.localScale;
                bulletScale.x *= -1;
                bullet.transform.localScale = bulletScale;
            }
        }
        
        Destroy(bullet, 3f);
    }

    void DetectPlayer()
    {
        Collider2D detectedPlayer = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        
        if (detectedPlayer != null)
        {
            player = detectedPlayer.transform;
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            
            if (distanceToPlayer <= attackRange)
            {
                currentState = EnemyState.Attack;
            }
            else if (distanceToPlayer <= detectionRange)
            {
                currentState = EnemyState.Chase;
            }
        }
        else
        {
            player = null;
            currentState = EnemyState.Patrol;
        }
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
        float direction = facingRight ? 1 : -1;
        
        // 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 땅 체크 라인
        Vector2 frontCheckPos = (Vector2)transform.position + new Vector2(direction * groundCheckOffset.x, groundCheckOffset.y);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(frontCheckPos, frontCheckPos + Vector2.down * groundCheckDistance);
        Gizmos.DrawWireSphere(frontCheckPos, 0.1f);
        
        // 벽 체크 라인
        Vector2 wallCheckPos = (Vector2)transform.position + new Vector2(0, 0.2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(wallCheckPos, wallCheckPos + Vector2.right * direction * wallCheckDistance);
        Gizmos.DrawWireSphere(wallCheckPos, 0.1f);
    }
}