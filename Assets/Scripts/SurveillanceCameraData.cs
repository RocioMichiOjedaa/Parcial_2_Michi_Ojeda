using UnityEngine;

[CreateAssetMenu(fileName = "SurveillanceCameraData", menuName = "Enemies/SurveillanceCamera")]
public class SurveillanceCameraData : ScriptableObject
{
    public float maxHealth = 100f;

    [Header("Vision Settings")]
    [Range(0, 180)]
    public float visionAngle = 60f;

    public float visionRange = 5f;
}
