using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Colliders_Network : MonoBehaviour 
{
	void OnTriggerEnter2D (Collider2D col)
	{
		if(this.tag == "RangeCollider" && col.tag == "BodyCollider")
		{
			if (this.transform.parent.tag != col.transform.parent.tag)
			{
				Character_Network character = GetComponentInParent<Character_Network> ();
				Character_Network enemyChar = col.GetComponentInParent<Character_Network> ();
				Base enemyBase = col.GetComponentInParent<Base>();

				if (character != null)
				{
					if (enemyChar != null)
					{
						character.targets.Add (enemyChar.gameObject);

						if (character.target == null)
							character.target = enemyChar.gameObject;
					}
					else if (enemyBase != null)
					{
						character.targets.Add (enemyBase.gameObject);

						if (character.target == null)
							character.target = enemyBase.gameObject;	
					}
				}
			}
		}
	}
}