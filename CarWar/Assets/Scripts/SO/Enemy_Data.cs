using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/EnemyData", order = 1)]
public class Enemy_Data : ScriptableObject
{
    public int enemyLevel;
    public float speed;
    public int scoreForDestroyEnemy;
}
