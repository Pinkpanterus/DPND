using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData", order = 1)]
public class Player_Data : ScriptableObject
{
    public float speed;
    public float rotationSpeed;
    public float turretRotationSpeed;
    public float fireRate;
}
