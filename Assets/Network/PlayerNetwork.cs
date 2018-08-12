using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerNetwork : NetworkBehaviour 
{
	private NetworkLobbyManager netLobbyManager;
	private Character_Network[] characters;

	void Start ()
	{
		StartCoroutine(SetCharactersReference());
        StartCoroutine(SetQuitButton());
        netLobbyManager = FindObjectOfType<NetworkLobbyManager>();
	}

	public override void OnStartLocalPlayer ()
	{
		StartCoroutine(SetPlayerReference());
	}

	IEnumerator SetPlayerReference ()
	{
		while (GameController_Network.instance == null)
		{
			yield return null;
			//print("Waiting for Game Controller to be instantiated...");
		}

		GameController_Network.instance.myPlayer = this;
	}

	IEnumerator SetCharactersReference ()
	{
		while (GameController_Network.instance == null || GameController_Network.instance.characters.Length < 1)
		{
			yield return null;
			//print("Waiting for Game Controller to set characters...");
		}

		characters = GameController_Network.instance.characters;
	}

    IEnumerator SetQuitButton ()
    {
        while (GameController_Network.instance == null)
            yield return null;
        Button quitButton = GameController_Network.instance.quitMatchButton;
        if (quitButton != null)
            quitButton.onClick.AddListener(() => CmdQuitMultiplayerGame());
    }
		
	public void Spawn (int characterId)
	{
		Character_Network character = null;

		if (characters[characterId] != null)
		{
			character = characters[characterId];
		}
		else
		{
			Debug.LogError("Character ID not found!");
			return;
		}

		if (SpawnManager_Network.instance.CanSpawn(character))
		{
			if (isServer)
				CmdSpawn(characterId, true);
			else
				CmdSpawn(characterId, false);
		}
		else
		{
			Debug.LogWarning("Cannot spawn this character!");
		}
	}

	[Command]
	void CmdSpawn (int characterId, bool isPlayer1)
	{
		Character_Network character = null;

		if (characters[characterId] != null)
		{
			character = characters[characterId];
		}
		else
		{
			Debug.LogError("Character ID not found!");
			return;
		}

		if (isPlayer1)
		{
			Character_Network c = Instantiate(character, SpawnManager_Network.instance.base1.transform);
            c.transform.position = SpawnManager_Network.instance.base1.transform.position + SpawnManager_Network.instance.spawnOffsetFromBase;
			NetworkServer.Spawn(c.gameObject);
			RpcSetCharacterTag(c.gameObject, true);
		}
		else
		{
			Character_Network c = Instantiate(character, SpawnManager_Network.instance.base2.transform);
			c.transform.position = new Vector3(SpawnManager_Network.instance.base2.transform.position.x - SpawnManager_Network.instance.spawnOffsetFromBase.x,
											   SpawnManager_Network.instance.base2.transform.position.y + SpawnManager_Network.instance.spawnOffsetFromBase.y,
											   SpawnManager_Network.instance.base2.transform.position.z + SpawnManager_Network.instance.spawnOffsetFromBase.z);
			NetworkServer.Spawn(c.gameObject);
			RpcSetCharacterTag(c.gameObject, false);
		}
		
		TargetSpendMana(connectionToClient, characterId);
	}

	[ClientRpc]
	void RpcSetCharacterTag (GameObject character, bool isPlayer1)
	{
		if (character != null)
		{
			if (isPlayer1)
				character.tag = "Player 1";
			else
				character.tag = "Player 2";
		}
		else
			Debug.LogError("Character not found!");
	}

	[TargetRpc]
	void TargetSpendMana (NetworkConnection conn, int characterId)
	{
		Character_Network character = null;

		if (characters[characterId] != null)
		{
			character = characters[characterId];
		}
		else
		{
			Debug.LogError("Character ID not found!");
			return;
		}

		Mana_Network targetMana = Mana_Network.GetMana(character.type, SpawnManager_Network.instance.allManas);
		if (targetMana != null)
			targetMana.SpendMana(character.manaCost);
		else
			Debug.LogError("TargetMana reference not found!");
	}

    [Command]
    public void CmdQuitMultiplayerGame ()
    {
        if (netLobbyManager != null)
            netLobbyManager.StopHost();
        else
            Debug.LogError("Network Lobby Manager reference not found!");
    }
}