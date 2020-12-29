using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "BossData", menuName = "ScriptableObjects/BossData", order = 1)]
public class Boss_Data : ScriptableObject
{
    public int shotsBeforeDeath = 20;
    public float speed = 5f;
    public float luckyChance = 40; // Chance for Boss lucky strike
    public float bossTurnSpeed = 20f;
    public float bossShootDelay = 1f;
    public float bossTurretReloadTime = 5f;
}
