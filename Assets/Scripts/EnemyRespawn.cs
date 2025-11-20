using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyRespawn : MonoBehaviour
{
    public EnemyAI enemyPrefab;
    private EnemyAI currentEnemy;

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
        currentEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
    }
}
