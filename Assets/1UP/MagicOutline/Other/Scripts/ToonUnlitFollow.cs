using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ToonUnlitFollow : MonoBehaviour
{

	Material _toonlitMat;

	private Material ToonLitMat {
		get {
			if (_toonlitMat == null) {
				if (GetComponent<MeshRenderer> () != null)
					_toonlitMat = GetComponent<MeshRenderer> ().sharedMaterial;
				else if (GetComponent<SkinnedMeshRenderer> () != null)
					_toonlitMat = GetComponent<SkinnedMeshRenderer> ().sharedMaterial;
			}

			return _toonlitMat;
		}
	}

	public Transform lightTarget;

	void Update ()
	{
		if (lightTarget != null) {
			Vector3 lightDir = (lightTarget.position - transform.position).normalized;

			ToonLitMat.SetVector ("_LightDir", lightDir);
		}


	}
}
