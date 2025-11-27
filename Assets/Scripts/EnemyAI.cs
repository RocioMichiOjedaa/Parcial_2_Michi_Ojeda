using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Normal, Chase, Damage, Dead }

    [Header("Enemy Type")]
    public Soldier soldierData;

    [Header("UI")]
    public TMP_Text stateText;

    [Header("Vision Debug")]
    public bool debugVision = true;

    [SerializeField] private Transform eyePoint;

    private EnemyState currentState = EnemyState.Normal;
    private NavMeshAgent agent;
    private Transform player;
    private PlayerStats playerStats;
    private Vector3 spawnPos;
    private Transform playerLookTarget;

    private bool hasSeenPlayer = false;
    private float currentHealth;

    private float nextFireTime = 0f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        player = GameObject.FindWithTag("Player").transform;
        playerStats = player.GetComponent<PlayerStats>();

        spawnPos = transform.position;
        currentHealth = soldierData.maxHealth;

        playerLookTarget = player.Find("Face");

        UpdateStateText();
    }

    private void Update()
    {
        if (currentState == EnemyState.Dead) return;
        if (!agent.enabled || !agent.isOnNavMesh) return;

        if (playerStats.Died)
        {
            SetState(EnemyState.Normal);
            agent.ResetPath();
            hasSeenPlayer = false;
            return;
        }

        if (hasSeenPlayer)
        {
            SetState(EnemyState.Chase);
            ChasePlayer();
            TryShootPlayer();
            return;
        }

        if (PlayerInVisionCone())
        {
            hasSeenPlayer = true;
            SetState(EnemyState.Chase);
            ChasePlayer();
            TryShootPlayer();

            playerStats.DrainStamina(soldierData.staminaDrainRate);
        }
        else
        {
            SetState(EnemyState.Normal);

            if (agent.enabled && agent.isOnNavMesh)
                agent.ResetPath();

            playerStats.StopDraining();
        }
    }

    private void LateUpdate()
    {
        if (stateText != null && Camera.main != null)
        {
            stateText.transform.LookAt(Camera.main.transform);
            stateText.transform.Rotate(0, 180, 0);
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
            float angle = Mathf.Lerp(-soldierData.visionAngle, soldierData.visionAngle, pct);

            Vector3 dir = Quaternion.Euler(0, angle, 0) * eyePoint.forward;
            Gizmos.DrawRay(eyePoint.position, dir * soldierData.visionRange);
        }
    }

    private void ChasePlayer()
    {
        if (playerStats.Died) return;

        agent.stoppingDistance = 2.2f;
        agent.SetDestination(player.position);
    }

    private void TryShootPlayer()
    {
        if (!soldierData.hasWeapon) return;
        if (Time.time < nextFireTime) return;
        if (playerStats.Died) return;

        Vector3 toPlayer = (playerLookTarget.position - eyePoint.position).normalized;

        float angle = Vector3.Angle(eyePoint.forward, toPlayer);
        if (angle > soldierData.visionAngle) return;

        if (Vector3.Distance(eyePoint.position, player.position) > soldierData.attackRange) return;

        if (Physics.Raycast(eyePoint.position, toPlayer, out RaycastHit hit, soldierData.attackRange))
        {
            Debug.DrawLine(eyePoint.position, hit.point, Color.yellow, 0.3f);

            if (hit.collider.CompareTag("Player"))
            {
                playerStats.TakeDamage(soldierData.attackDamage);
                Debug.Log($"Enemy SHOT player → {soldierData.attackDamage} damage");

                nextFireTime = Time.time + soldierData.attackCooldown;
            }
        }
    }

    private void SetState(EnemyState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            UpdateStateText();
        }
    }

    private void UpdateStateText()
    {
        if (stateText != null)
            stateText.text = currentState.ToString();
    }

    public void TakeDamage(float dmg)
    {
        if (currentState == EnemyState.Dead) return;

        hasSeenPlayer = true;

        currentHealth -= dmg;
        Debug.Log($"Enemy takes {dmg} damage. HP: {currentHealth}");

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
        currentHealth = soldierData.maxHealth;
        transform.position = spawnPos;
        agent.enabled = true;
        hasSeenPlayer = false;
        SetState(EnemyState.Normal);
        gameObject.SetActive(true);
    }

    private bool PlayerInVisionCone()
    {
        if (playerLookTarget == null) return false;
        if (playerStats.Died) return false;

        Vector3 toPlayer = (playerLookTarget.position - eyePoint.position);
        Vector3 dir = toPlayer.normalized;

        if (toPlayer.magnitude > soldierData.visionRange) return false;

        float angle = Vector3.Angle(eyePoint.forward, dir);
        if (angle > soldierData.visionAngle) return false;

        if (Physics.Raycast(eyePoint.position, dir, out RaycastHit hit, soldierData.visionRange))
        {
            return hit.collider.CompareTag("Player");
        }

        return false;
    }
}
