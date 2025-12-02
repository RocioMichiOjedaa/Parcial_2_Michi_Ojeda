using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Patrol, Normal, Chase, Damage, Dead }

    [Header("Enemy Type")]
    public Soldier soldierData;

    [Header("UI")]
    public TMP_Text stateText;

    [Header("Vision Debug")]
    public bool debugVision = true;

    [SerializeField] private Transform eyePoint;

    [Header("Patrol")]
    private Transform[] patrolPoints;
    [Tooltip("Velocidad del agente mientras patrulla")]
    public float patrolSpeed = 2f;
    [Tooltip("Distancia para considerar 'llegado' al punto de patrulla")]
    public float patrolPointTolerance = 0.5f;

    private EnemyState currentState = EnemyState.Patrol;
    private NavMeshAgent agent;
    private Transform player;
    private PlayerStats playerStats;
    private Vector3 spawnPos;
    private Transform playerLookTarget;

    private bool hasSeenPlayer = false;
    private float currentHealth;

    // ataque
    private float nextFireTime = 0f;

    // patrol internals
    private int patrolIndex = 0;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("No se encontró GameObject con tag 'Player' en la escena.");
            enabled = false;
            return;
        }

        player = playerObj.transform;
        playerStats = playerObj.GetComponent<PlayerStats>();

        spawnPos = transform.position;
        currentHealth = soldierData != null ? soldierData.maxHealth : 50f;

        if (player != null)
            playerLookTarget = player.Find("Face");

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            currentState = EnemyState.Patrol;
            GoToPatrolPoint();
        }
        else
        {
            currentState = EnemyState.Normal;
        }

        UpdateStateText();
        Debug.Log($"Enemy started in state: {currentState}");
    }

    private void Update()
    {
        if (currentState == EnemyState.Dead) return;
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        if (playerStats == null) return;

        // Si el jugador está muerto → dejar de perseguir y volver a patrullar
        if (playerStats.Died)
        {
            hasSeenPlayer = false;

            if (currentState != EnemyState.Patrol)
                SetState(EnemyState.Patrol);

            agent.stoppingDistance = 0f;  // asegurarse de que patrulle normalmente

            Patrol(); // sigue patrullando

            return;
        }


        // Si ya lo vio (lo persigue hasta perderlo)
        if (hasSeenPlayer)
        {
            // Si perdió la visión y excede la distancia de chase, dejar de perseguir y volver a patrullar
            bool stillSees = PlayerInVisionCone();
            float distToPlayer = Vector3.Distance(transform.position, player.position);

            if (!stillSees && distToPlayer > soldierData.chaseDistance)
            {
                // perdió al jugador: reiniciar flags y volver a patrullar
                hasSeenPlayer = false;
                if (patrolPoints != null && patrolPoints.Length > 0)
                {
                    SetState(EnemyState.Patrol);
                    GoToPatrolPoint();
                }
                else
                {
                    SetState(EnemyState.Normal);
                    agent.ResetPath();
                }

                playerStats.StopDraining();
                return;
            }

            // sigue en chase
            SetState(EnemyState.Chase);
            ChasePlayer();
            TryShootPlayer();
            return;
        }

        // Si lo detecta por visión: pasa a chase
        if (PlayerInVisionCone())
        {
            hasSeenPlayer = true;
            SetState(EnemyState.Chase);
            ChasePlayer();
            TryShootPlayer();

            playerStats.DrainStamina(soldierData.staminaDrainRate);
            return;
        }

        // No lo ve -> patrulla (si tiene puntos) o estado Normal
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Patrol();
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

        // dibujar patrol points en escena
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.cyan;
            foreach (var p in patrolPoints)
            {
                if (p == null) continue;
                Gizmos.DrawSphere(p.position, 0.15f);
            }

            // líneas entre puntos
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                Transform a = patrolPoints[i];
                Transform b = patrolPoints[(i + 1) % patrolPoints.Length];
                if (a != null && b != null)
                    Gizmos.DrawLine(a.position, b.position);
            }
        }
    }

    public void SetPatrolPoints(Transform[] patrolPoints)
    {
        this.patrolPoints = patrolPoints;

        currentState = EnemyState.Patrol;
        GoToPatrolPoint();

        UpdateStateText();
        Debug.Log($"Enemy started in state: {currentState}");
    }

    private void ChasePlayer()
    {
        if (playerStats.Died) return;

        agent.stoppingDistance = 2.2f;
        agent.speed = soldierData != null ? Mathf.Max(agent.speed, soldierData.chaseDistance > 0 ? agent.speed : agent.speed) : agent.speed;
        agent.SetDestination(player.position);
    }

    private void TryShootPlayer()
    {
        if (!soldierData.hasWeapon) return;
        if (Time.time < nextFireTime) return;
        if (playerStats.Died) return;

        if (playerLookTarget == null) return;

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

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        // asegurarse que el agent tenga como velocidad la de patrulla
        agent.speed = patrolSpeed;

        Vector3 target = patrolPoints[patrolIndex].position;

        // usamos NavMeshAgent para moverse pero la lógica de llegada usa vectores
        agent.SetDestination(target);

        float dist = Vector3.Distance(transform.position, target);
        if (dist <= patrolPointTolerance)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;

            agent.ResetPath(); // ← otro FIX importante

            GoToPatrolPoint();
        }

    }

    private void GoToPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        agent?.ResetPath(); // ← FIX CLAVE

        Vector3 target = patrolPoints[patrolIndex].position;
        agent?.SetDestination(target);

        Debug.Log($"Enemy going to patrol point {patrolIndex}: {target}");
    }


    private void SetState(EnemyState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            Debug.Log("Enemy State → " + currentState);
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

        // volver a patrullar si hay puntos
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            patrolIndex = 0;
            SetState(EnemyState.Patrol);
            GoToPatrolPoint();
        }
        else
        {
            SetState(EnemyState.Normal);
        }

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
