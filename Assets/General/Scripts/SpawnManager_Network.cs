using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpawnManager_Network : NetworkBehaviour 
{
	public Vector3 spawnOffsetFromBase;
	public bool freeSpawn;
	public Base base1;
	public Base base2;
	public Mana_Network[] allManas;

	public static SpawnManager_Network instance;

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
		while(GameController_Network.instance.gameControllerReady == false)
		{
			yield return null;
			//print("Waiting for Game Controller to be ready...");
		}
		base1 = GameController_Network.instance.base1;
		base2 = GameController_Network.instance.base2;
		allManas = GameController_Network.instance.allManas;
		foreach (Mana_Network mana in allManas)
			mana.UpdateButtonInteractivity();
	}

	Character_Network GetRandomCharacter ()
	{
		return GameController_Network.instance.characters[Random.Range(0, GameController_Network.instance.characters.Length)];
	}

	public bool CanSpawn (Character_Network character)
	{
		if (freeSpawn)
			return true;

		if (character == null)
		{
			Debug.LogWarning("Trying to spawn a null reference Character!");
			return false;
		}

		Mana_Network targetMana = null;

		foreach (Mana_Network mana in allManas) 
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