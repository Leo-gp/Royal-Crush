using UnityEngine;
using System.Collections;

public class ParticleManager : MonoBehaviour
{
	public GameObject clearFXPrefab;

	public void ClearPieceFXAt(int x, int y, int z = 0)
	{
		if (clearFXPrefab != null)
		{
			GameObject clearFX = Instantiate(clearFXPrefab, new Vector3(x,y,z), Quaternion.identity) as GameObject;

			ParticlePlayer particlePlayer = clearFX.GetComponent<ParticlePlayer>();

			if (particlePlayer !=null)
			{
				particlePlayer.Play();
			}
		}
	}
}