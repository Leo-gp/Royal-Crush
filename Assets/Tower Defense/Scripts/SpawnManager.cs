using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour 
{
	public Character[] characters;
	public bool enemySpawnerEnabled;
	public float enemySpawnerRate;
	public bool freeSpawn;

	//private GameObject base1;
	private Base base2;

	private Mana[] allManas;

	private bool enemySpawnerRunning;

	public static SpawnManager instance;

	void Awake ()
	{
		instance = this;
	}

	void Start ()
	{
		//base1 = GameController.instance.base1;
		base2 = GameController.instance.base2;
		allManas = FindObjectsOfType<Mana> ();
	}

	void Update ()
	{
		if (!enemySpawnerRunning && enemySpawnerEnabled)
		{
			StartCoroutine (EnemySpawner ());
		}
	}

	public void Spawn (Character character, Transform position)
	{
		if (CanSpawn(character))
		{
			Character c = Instantiate (character, position);
			c.transform.position = new Vector3(position.position.x, position.position.y, position.position.z + 1);
			c.tag = "Player 1";
			Mana targetMana = Mana.GetMana (c.type, allManas);
			if (targetMana != null)
				targetMana.SpendMana (c.manaCost);
		}
	}

	IEnumerator EnemySpawner ()
	{
		enemySpawnerRunning = true;

		while (enemySpawnerEnabled)
		{
			yield return new WaitForSeconds (enemySpawnerRate);
			Character enemy = Instantiate (GetRandomCharacter(), base2.transform);
			enemy.transform.position = new Vector3(base2.transform.position.x, base2.transform.position.y, base2.transform.position.z + 1);
			enemy.tag = "Player 2";
		}

		enemySpawnerRunning = false;
	}

	Character GetRandomCharacter ()
	{
		int random = Random.Range (0, characters.Length);

		Character character = characters [random];

		return character;
	}

	public bool CanSpawn (Character character)
	{
		if (freeSpawn)
			return true;

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