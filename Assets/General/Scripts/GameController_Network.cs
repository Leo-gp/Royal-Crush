using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameController_Network :  NetworkBehaviour
{
	public Character_Network[] characters;
	//public GameObject characterSelectionPrefab;
	public GameObject gameOverScreenPrefab;
    public GameObject quitToMenuScreen;
	public Base base1;
	public Base base2;
	//public Base basePrefab;
	//public Vector3 player1BasePosition;
	//public Vector3 player2BasePosition;
	public GameObject redCharacterSlot;
	public GameObject blueCharacterSlot; 
	public GameObject greenCharacterSlot;
	public GameObject brownCharacterSlot;
	public Mana_Network[] allManas;
    public Button quitMatchButton;
    public Button gameOverButton;
    public AudioSource gameSoundtrack;
    public AudioSource victorySound;
    public AudioSource defeatSound;

    [HideInInspector] public PlayerNetwork myPlayer;
	[HideInInspector] public bool gameControllerReady;

	private Base myBase;
	private bool charactersSet;

	public static GameController_Network instance;

	void Awake ()
	{
		Time.timeScale = 1;
		instance = this;
		//print("A wild GameController instance appeared!");
		//SetupCharacterSelectionUI();
    }

    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (quitToMenuScreen.activeInHierarchy)
            {
                quitToMenuScreen.SetActive(false);
            }
            else
            {
                quitToMenuScreen.SetActive(true);
            }
        }
    }

    void Start ()
    {
		StartCoroutine(SetupGame());
    }

	/*void SetupCharacterSelectionUI ()
	{
		Instantiate (characterSelectionPrefab);
		redCharacterSlot = GameObject.FindGameObjectWithTag ("RedSlot");
		blueCharacterSlot = GameObject.FindGameObjectWithTag ("BlueSlot");
		greenCharacterSlot = GameObject.FindGameObjectWithTag ("GreenSlot");
		brownCharacterSlot = GameObject.FindGameObjectWithTag ("BrownSlot");
		allManas = FindObjectsOfType<Mana>();
	}*/

	IEnumerator SetupGame ()
	{
		StartCoroutine(SetupCharacters());

		while (charactersSet == false)
			yield return null;

		//SetupBases();

		if (myPlayer.isServer)
			myBase = base1;
		else
			myBase = base2;

		gameControllerReady = true;
	}

	IEnumerator SetupCharacters ()
	{
		while(myPlayer == null)
		{
			yield return null;
			//print("Waiting for player reference...");
		}

		for(int i = 0; i < characters.Length; i++)
		{
			Transform charSlot = null;

			switch(characters[i].type)
			{
				case GamePiece_Network.Type.Red:
					charSlot = redCharacterSlot.transform;
					break;
				case GamePiece_Network.Type.Blue:
					charSlot = blueCharacterSlot.transform;
					break;
				case GamePiece_Network.Type.Green:
					charSlot = greenCharacterSlot.transform;
					break;
				case GamePiece_Network.Type.Brown:
					charSlot = brownCharacterSlot.transform;
					break;
			}

			int charId = i;
			charSlot.name = characters[i].charName;
			Button charButton = charSlot.GetComponentInChildren<Button> ();
			charButton.GetComponentInChildren<Text> ().text = characters[i].charName;
			if(myPlayer != null)
				charButton.onClick.AddListener(() => myPlayer.Spawn(charId));
			else
				Debug.LogError("This player has not been set correctly!");
			GameObject[] allManaCosts = GameObject.FindGameObjectsWithTag ("ManaCost");
			foreach (GameObject manaCost in allManaCosts) 
			{
				if (manaCost.transform.IsChildOf(charSlot))
				{
					manaCost.GetComponentInChildren<Text> ().text = characters[i].manaCost.ToString();
					break;
				}
			}
		}

		charactersSet = true;
	}

	/*void SetupBases ()
	{
		Base baseP1 = Instantiate(basePrefab, player1BasePosition, Quaternion.identity);
		baseP1.name = "Player1Base";
		baseP1.tag = "Player 1";
		base1 = baseP1;

		Base baseP2 = Instantiate(basePrefab, player2BasePosition, Quaternion.identity);
		baseP2.name = "Player2Base";
		baseP2.tag = "Player 2";
		base2 = baseP2;

		if (myPlayer != null)
		{
			if (myPlayer.isServer)
				myBase = base1;
			else
				myBase = base2;
		}
		else
			Debug.LogError("myPlayer not found!");
	}*/

	public void GameOver ()
	{
        gameOverScreenPrefab.SetActive(true);

        gameOverButton.onClick.AddListener(() => myPlayer.CmdQuitMultiplayerGame());

        if (myBase.destroyed)
        {
            gameOverScreenPrefab.GetComponentInChildren<Text>().text = "YOU lOSE!";
            gameSoundtrack.Stop();
            defeatSound.Play();
        }
		else
        {
            gameOverScreenPrefab.GetComponentInChildren<Text>().text = "YOU WIN!";
            gameSoundtrack.Stop();
            victorySound.Play();
        }

		Time.timeScale = 0;

		FindObjectOfType<Board_Network>().enabled = false;
		FindObjectOfType<SpawnManager_Network>().enabled = false;
	}
}