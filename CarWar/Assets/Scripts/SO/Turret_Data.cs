using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "TurretData", menuName = "ScriptableObjects/TurretData", order = 1)]
public class Turret_Data : ScriptableObject
{
    public float turretTurnSpeed = 40f;
    public float fireRate = 1f;
    public int ammoCount = 20;
    public float maxDeviation = 5f;
    public GameObject shell;
}
