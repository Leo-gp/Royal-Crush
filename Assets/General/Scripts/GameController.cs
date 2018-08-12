using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController :  MonoBehaviour
{
	public Character[] characters;
	//public GameObject characterSelectionPrefab;
	public GameObject gameOverScreenPrefab;
    public GameObject pauseScreen;
	public Base base1;
	public Base base2;
	public GameObject redCharacterSlot;
	public GameObject blueCharacterSlot; 
	public GameObject greenCharacterSlot;
	public GameObject brownCharacterSlot;
	public Mana[] allManas;
    public AudioSource gameSoundtrack;
    public AudioSource victorySound;
    public AudioSource defeatSound;

    [HideInInspector] public bool gameControllerReady;
    [HideInInspector] public bool gameIsPaused;

	public static GameController instance;

	void Awake ()
	{
		Time.timeScale = 1;
		if (instance == null)
			instance = this;
		else
			Destroy(this.gameObject);
		//print("A wild GameController instance appeared!");
		//SetupCharacterSelectionUI();
    }
        
    void Start ()
    {
		SetupCharacters();
    }

    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameIsPaused)
            {
                pauseScreen.SetActive(false);
                Unpause();
            }
            else
            {
                pauseScreen.SetActive(true);
                Pause();
            }
        }
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

    void SetupCharacters ()
	{
		for(int i = 0; i < characters.Length; i++)
		{
			Transform charSlot = null;

			switch(characters[i].type)
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

			int charId = i;
			charSlot.name = characters[i].charName;
			Button charButton = charSlot.GetComponentInChildren<Button> ();
			charButton.GetComponentInChildren<Text> ().text = characters[i].charName;
			charButton.onClick.AddListener(() => SpawnManager.instance.Spawn(characters[charId], base1.transform));
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

		gameControllerReady = true;
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
	}*/

	public void GameOver ()
	{
        gameOverScreenPrefab.SetActive(true);

		if (base1.destroyed)
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

		FindObjectOfType<Board>().enabled = false;
		FindObjectOfType<SpawnManager>().enabled = false;
	}

    public void Pause ()
    {
        Time.timeScale = 0;
        gameIsPaused = true;
        Board.instance.m_playerInputEnabled = false;
    }

    public void Unpause()
    {
        Time.timeScale = 1;
        gameIsPaused = false;
        Board.instance.m_playerInputEnabled = true;
    }
}