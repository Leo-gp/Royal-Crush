using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour 
{
	public Character[] characters;
	public GameObject characterSelectionPrefab;
	public GameObject gameOverScreenPrefab;
	public Base basePrefab;
	public Vector3 player1BasePosition;
	public Vector3 player2BasePosition;

	[HideInInspector] public Base base1;
	[HideInInspector] public Base base2;

	private GameObject redCharacterSlot;
	private GameObject blueCharacterSlot; 
	private GameObject greenCharacterSlot;
	private GameObject brownCharacterSlot;

	public static GameController instance;

	void Awake ()
	{
		Time.timeScale = 1;
		instance = this;
		SetupCharacterSelectionUI ();
		SetupBases();
		SetupCharacters ();
	}

	void SetupCharacterSelectionUI ()
	{
		Instantiate (characterSelectionPrefab);
		redCharacterSlot = GameObject.FindGameObjectWithTag ("RedSlot");
		blueCharacterSlot = GameObject.FindGameObjectWithTag ("BlueSlot");
		greenCharacterSlot = GameObject.FindGameObjectWithTag ("GreenSlot");
		brownCharacterSlot = GameObject.FindGameObjectWithTag ("BrownSlot");
	}

	void SetupCharacters ()
	{
		foreach (Character character in characters) 
		{
			Transform charSlot = null;

			switch(character.type)
			{
				case GamePiece.Type.Red:
					charSlot = redCharacterSlot.transform;
					break;
				case GamePiece.Type.Blue:
					charSlot = blueCharacterSlot.transform;
					break;
				case GamePiece.Type.Green:
					charSlot = greenCharacterSlot.transform;
					break;
				case GamePiece.Type.Brown:
					charSlot = brownCharacterSlot.transform;
					break;
			}

			charSlot.name = character.name;
			Button charButton = charSlot.GetComponentInChildren<Button> ();
			charButton.GetComponentInChildren<Text> ().text = character.name;
			charButton.onClick.AddListener(() => SpawnManager.instance.Spawn (character, base1.transform));
			GameObject[] allManaCosts = GameObject.FindGameObjectsWithTag ("ManaCost");
			foreach (GameObject manaCost in allManaCosts) 
			{
				if (manaCost.transform.IsChildOf(charSlot))
				{
					manaCost.GetComponentInChildren<Text> ().text = character.manaCost.ToString();
					break;
				}
			}
		}
	}

	void SetupBases ()
	{
		Base baseP1 = Instantiate(basePrefab, player1BasePosition, Quaternion.identity);
		baseP1.name = "Player1Base";
		baseP1.tag = "Player 1";
		base1 = baseP1;

		Base baseP2 = Instantiate(basePrefab, player2BasePosition, Quaternion.identity);
		baseP2.name = "Player2Base";
		baseP2.tag = "Player 2";
		base2 = baseP2;
	}

	public void GameOver ()
	{
		GameObject screen = Instantiate(gameOverScreenPrefab);

		if (base2.destroyed)
		{
			screen.GetComponentInChildren<Text>().text = "YOU WIN!";
		}
		else if (base1.destroyed)
		{
			screen.GetComponentInChildren<Text>().text = "YOU LOSE!";
		}

		Time.timeScale = 0;

		FindObjectOfType<Board>().enabled = false;
		FindObjectOfType<SpawnManager>().enabled = false;

		screen.GetComponentInChildren<Button>().onClick.AddListener(() => RestartGame());
	}

	public void RestartGame ()
	{
		SceneManager.LoadScene("Game");
	}
}