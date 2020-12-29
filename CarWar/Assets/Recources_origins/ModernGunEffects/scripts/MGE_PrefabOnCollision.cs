using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGE_PrefabOnCollision : MonoBehaviour {

	public GameObject createThis;
	private ParticleSystem[] p;
	private GameObject vfxPool;

    private void Start()
    {
		vfxPool = GameObject.Find("VFX_Pool");
    }

    private void OnTriggerEnter(Collider other)
    {
		Instantiate(createThis, transform.position, transform.rotation, vfxPool.transform);

		Destroy(GetComponent<Rigidbody>());
		Destroy(GetComponent<Renderer>());

		p = GetComponentsInChildren<ParticleSystem>();

		foreach (ParticleSystem PS in p)
			PS.Stop();
	}
}
