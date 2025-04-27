using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    public override void OnNetworkSpawn()
    {
        Debug.Log("🧍 Player 생성됨 (ClientId): " + OwnerClientId);

        if (!IsOwner)
        {
            enabled = false; // 내 Player가 아니면 스크립트 비활성화
            return;
        }

        // ✅ 안전하게 Z축 앞으로
        Vector3 pos = transform.position;
        pos.z = -1f;
        transform.position = pos;

        Debug.Log("🧍‍♂️ 내 플레이어 컨트롤 활성화");

        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!IsOwner || rb == null) return;

        // 방향키 입력 (WASD)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;
    }

    void FixedUpdate()
    {
        if (!IsOwner || rb == null) return;

        // Rigidbody2D를 이용한 이동
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
