using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour 
{
	public Vector3 spawnOffsetFromBase;
	public bool freeSpawn;

	public Base base1;
	public Base base2;
	public Mana[] allManas;

	public static SpawnManager instance;

	void Awake ()
	{
		if (instance == null)
			instance = this;
		else
			Destroy(this.gameObject);
	}

	void Start ()
	{
		StartCoroutine(SetReferences());
	}

	IEnumerator SetReferences ()
	{
		while(GameController.instance.gameControllerReady == false)
		{
			yield return null;
			//print("Waiting for Game Controller to be ready...");
		}
		base1 = GameController.instance.base1;
		base2 = GameController.instance.base2;
		allManas = GameController.instance.allManas;
		foreach (Mana mana in allManas)
			mana.UpdateButtonInteractivity();
	}

	public void Spawn (Character character, Transform pos)
	{
		if (CanSpawn(character))
		{
			Character c = Instantiate(character, pos);
            c.transform.position = base1.transform.position + spawnOffsetFromBase;
			c.tag = "Player 1";
			Mana targetMana = Mana.GetMana(c.type, allManas);
			if (targetMana != null)
				targetMana.SpendMana(c.manaCost);
		}
	}

	public bool CanSpawn (Character character)
	{
		if (freeSpawn)
			return true;

		if (character == null)
		{
			Debug.LogWarning("Trying to spawn a null reference Character!");
			return false;
		}

		Mana targetMana = null;

		foreach (Mana mana in allManas) 
		{
			if (mana.manaType == character.type)
			{
				targetMana = mana;
				break;
			}
		}

		if (targetMana != null && character.manaCost <= targetMana.manaPoints)
		{
			return true;
		}

		return false;
	}
}