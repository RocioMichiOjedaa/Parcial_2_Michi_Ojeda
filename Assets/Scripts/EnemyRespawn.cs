using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyRespawn : MonoBehaviour
{
    public EnemyAI enemyPrefab;
    private EnemyAI currentEnemy;

    [Tooltip("Puntos (Transforms) que el enemigo recorrerá en patrulla")]
    public Transform[] patrolPoints;

    private void Start()
    {
        Spawn();
    }

    private void Update()
    {
        if (Keyboard.current.f3Key.wasPressedThisFrame)
        {
            if (currentEnemy != null) currentEnemy.Respawn();
            else Spawn();
        }
    }

    void Spawn()
    {
        currentEnemy = Instantiate(enemyPrefab, transform.position, transform.rotation);
        currentEnemy.SetPatrolPoints(patrolPoints);
    }
}
