using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeSplit : MonoBehaviour {
	//=======================================
	//      Variables
	//=======================================
	public List<GameObject> splitModels = new List<GameObject>();

	public float splittingSlimeDamage = 1;
	public float eattingSlimeRecover = 1;

	//=======================================
	//      Functions
	//=======================================
	void Start()
	{
		Utilities.TryCatchError(((splitModels.Count <= 0)), "No split model for current SlimeSplit.");
		GameObject model = transform.Find("Model").gameObject;

		GameObject obj = Instantiate(splitModels[Random.Range(0, splitModels.Count)], model.transform.position, Quaternion.Euler(0, 0, 0));
		obj.transform.parent = model.transform;
	}
}
