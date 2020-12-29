using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyByTime : MonoBehaviour
{
    public float timeBeforeDestoy = 4f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, timeBeforeDestoy);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
