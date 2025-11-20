using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Normal, Chase, Damage, Dead }

    [Header("Stats")]
    public float maxHealth = 50f;
    public float currentHealth;

    [Header("Chase Settings")]
    public float chaseDistance = 5f;
    public float staminaDrainRate = 1f;

    [Header("Vision Settings")]
    [Tooltip("Ángulo del cono en grados")]
    public float visionAngle = 45f;

    [Tooltip("Distancia máxima del cono de visión")]
    public float visionRange = 7f;

    [Tooltip("Si se activa, dibuja el cono")]
    public bool debugVision = true;

    [SerializeField] private Transform eyePoint;

    private EnemyState currentState = EnemyState.Normal;
    private NavMeshAgent agent;
    private Transform player;
    private PlayerStats playerStats;
    private Vector3 spawnPos;
    private Transform playerLookTarget;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        player = GameObject.FindWithTag("Player").transform;
        playerStats = player.GetComponent<PlayerStats>();

        spawnPos = transform.position;
        currentHealth = maxHealth;

        playerLookTarget = player.Find("Face");
    }

    private void Update()
    {
        if (currentState == EnemyState.Dead) return;
        if (!agent.enabled || !agent.isOnNavMesh) return;

        if (PlayerInVisionCone())
        {
            SetState(EnemyState.Chase);
            ChasePlayer();

            playerStats.DrainStamina(staminaDrainRate);
        }
        else
        {
            SetState(EnemyState.Normal);

            if (agent.enabled && agent.isOnNavMesh)
                agent.ResetPath();

            playerStats.StopDraining();
        }
    }

    private void OnDrawGizmos()
    {
        if (!debugVision || eyePoint == null) return;

        Gizmos.color = Color.red;
        int steps = 30;

        for (int i = 0; i <= steps; i++)
        {
            float pct = (float)i / steps;
            float angle = Mathf.Lerp(-visionAngle, visionAngle, pct);

            Vector3 dir = Quaternion.Euler(0, angle, 0) * eyePoint.forward;
            Gizmos.DrawRay(eyePoint.position, dir * visionRange);
        }
    }

    private void ChasePlayer()
    {
        agent.stoppingDistance = 1.2f;
        agent.SetDestination(player.position);
    }

    private void SetState(EnemyState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            Debug.Log("Enemy State → " + currentState);
        }
    }

    public void TakeDamage(float dmg)
    {
        if (currentState == EnemyState.Dead) return;

        currentHealth -= dmg;
        Debug.Log($"Enemy damaged! HP: {currentHealth}");

        SetState(EnemyState.Damage);

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        SetState(EnemyState.Dead);
        agent.enabled = false;
        gameObject.SetActive(false);
    }

    public void Respawn()
    {
        currentHealth = maxHealth;
        transform.position = spawnPos;
        agent.enabled = true;
        SetState(EnemyState.Normal);
        gameObject.SetActive(true);
    }

    private bool PlayerInVisionCone()
    {
        // dirección desde los ojos del enemigo al centro del jugador
        Vector3 toPlayer = (playerLookTarget.position - eyePoint.position);
        Vector3 dir = toPlayer.normalized;

        // 1) Rango
        if (toPlayer.magnitude > visionRange) return false;

        // 2) Ángulo
        float angle = Vector3.Angle(eyePoint.forward, dir);
        if (angle > visionAngle) return false;

        // 3) Line of sight real
        if (Physics.Raycast(eyePoint.position, dir, out RaycastHit hit, visionRange))
        {
            return hit.collider.CompareTag("Player");
        }

        return false;
    }
}
