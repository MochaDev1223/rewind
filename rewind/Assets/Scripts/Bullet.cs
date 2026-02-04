using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float flySpeed = 15f;
    public float returnSpeed = 25f;
    public float maxDistance = 25f;

    private Rigidbody2D rigid;
    private Transform player;
    private bool isReturning = false;
    private bool isEmbedded = false;
    private Collider2D col; // 콜라이더 캐싱
    public GameObject bulletEffect; // 인스펙터에서 이펙트 담당

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        // 씬에서 Player 태그를 가진 오브젝트를 찾습니다.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    public void Launch(Vector2 direction)
    {
        rigid.linearVelocity = direction * flySpeed;
        RotateTowards(direction);
        // // 탄환이 날아가는 방향을 바라보게 회전 (선택 사항)
        // float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void Update()
    {
        if (isReturning)
        {
            if (player == null) return;

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

    // 방향을 입력받아 그쪽을 바라보게 회전시키는 헬퍼 함수
    void RotateTowards(Vector2 direction)
    {
        if (direction == Vector2.zero) return;

        // 아크탄젠트(Atan2)로 벡터의 각도 계산 (라디안 -> 도)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // Z축을 기준으로 회전 적용
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    // void OnCollisionEnter2D(Collision2D collision)
    // {
    //     // 돌아오는 중이 아닐 때만 박힘 (Ground나 Enemy 레이어/태그 확인)
    //     if (!isReturning && (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Enemy")))
    //     {
    //         Embed(collision.transform);
    //     }
    // }

    // Collision 대신 Trigger를 사용합니다.
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isReturning && other.CompareTag("Enemy"))
        {

            // 적에게 데미지 입히기
            Enemy enemy = other.GetComponent<Enemy>();
            Animator enemyAnim = enemy.GetComponent<Animator>();

            if (enemy != null)
            {
                enemy.EnemyHP -= 1f;
                if (enemy.EnemyHP >= 1)
                {
                    enemyAnim.SetTrigger("Hit");
                }    
            }

            // 이펙트 생성
            GameObject effect = Instantiate(bulletEffect, transform.position, Quaternion.identity);
            Destroy(effect, 0.2f);

            // 적에게는 박히지 않고 바로 리턴
            StartReturn();
        }
        else if (!isReturning && other.CompareTag("Ground"))
        {
            // Trigger는 물리적 힘을 전달하지 않으므로 적이 전혀 밀리지 않습니다.
            Embed(other.transform);

            // Vector2 hitPoint = other.ClosestPoint(transform.position);
            GameObject effect = Instantiate(bulletEffect, transform.position, Quaternion.identity);
            Destroy(effect, 0.2f);
        }
    }


    void Embed(Transform target)
    {
        isEmbedded = true;
        rigid.linearVelocity = Vector2.zero;
        rigid.bodyType = RigidbodyType2D.Kinematic; // 물리 엔진 영향 정지
        transform.SetParent(target, true); // 박힌 대상(적/움직이는 발판 등)을 따라다님
    }

    public void StartReturn()
    {
        isReturning = true;
        isEmbedded = false;
        transform.SetParent(null);

        // 박혀있던 물리 설정을 풀고 Trigger로 바꿔서 벽을 뚫고 오게 함
        GetComponent<Collider2D>().isTrigger = true;
    }
}