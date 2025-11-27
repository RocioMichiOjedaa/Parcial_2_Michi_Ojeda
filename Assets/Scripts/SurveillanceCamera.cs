using UnityEngine;

public class SurveillanceCamera : MonoBehaviour
{
    public SurveillanceCameraData data;

    private float currentHealth;
    private Transform player;

    [Header("Gizmos")]
    public bool debug = true;
    public Color insideColor = Color.red;
    public Color outsideColor = Color.green;

    private bool isSeeingPlayer = false;

    private void Start()
    {
        currentHealth = data.maxHealth;
        player = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        DetectPlayer();
    }

    private void DetectPlayer()
    {
        // 1) Vector hacia el jugador
        Vector3 toPlayer = player.position - transform.position;

        // 2) Distancia
        float distance = toPlayer.magnitude;

        if (distance > data.visionRange)
        {
            isSeeingPlayer = false;
            return;
        }

        // 3) Normalizamos
        Vector3 dir = toPlayer.normalized;

        // 4) Producto Punto
        float dot = Vector3.Dot(transform.forward, dir);

        // Convertimos ángulo de apertura a dot product
        float limit = Mathf.Cos(data.visionAngle * Mathf.Deg2Rad);

        if (dot >= limit)
        {
            // Está dentro del cono
            isSeeingPlayer = true;
        }
        else
        {
            isSeeingPlayer = false;
        }
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;

        if (currentHealth <= 0)
            Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (!debug)
            return;

        Gizmos.color = isSeeingPlayer ? insideColor : outsideColor;

        // Dibujar forward
        Vector3 forward = transform.forward * (data != null ? data.visionRange : 5f);
        Gizmos.DrawRay(transform.position, forward);

        // Bordes del cono
        float angle = data != null ? data.visionAngle : 60f;
        float distance = data != null ? data.visionRange : 5f;

        Quaternion leftRot = Quaternion.AngleAxis(-angle, Vector3.up);
        Quaternion rightRot = Quaternion.AngleAxis(angle, Vector3.up);

        Vector3 left = leftRot * transform.forward * distance;
        Vector3 right = rightRot * transform.forward * distance;

        Gizmos.DrawRay(transform.position, left);
        Gizmos.DrawRay(transform.position, right);

        // arco (similar al código del profe)
        int segments = 20;
        Vector3 prev = transform.position + left;

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float ang = Mathf.Lerp(-angle, angle, t);

            Quaternion rot = Quaternion.AngleAxis(ang, Vector3.up);
            Vector3 point = transform.position + rot * transform.forward * distance;

            Gizmos.DrawLine(prev, point);
            prev = point;
        }
    }
}
