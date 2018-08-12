using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SceneController : MonoBehaviour 
{
	public static SceneController instance;

	void Awake ()
	{
		if (instance == null)
			instance = this;
		else
			Destroy(this.gameObject);
	}

    void Start ()
    {
        NetworkManagerHUD netHUD = FindObjectOfType<NetworkManagerHUD>();

        if (netHUD != null)
        {
            if (SceneManager.GetActiveScene().name == "Lobby")
            {
                netHUD.showGUI = true;
                NetworkLobbyManager lobbyManager = FindObjectOfType<NetworkLobbyManager>();
                Button backButton = FindObjectOfType<Button>();
                backButton.onClick.AddListener(() => lobbyManager.StopHost());
            }
            else
                netHUD.showGUI = false;
        }
    }

    public void LoadScene (string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}

	public void QuitGame ()
	{
		Application.Quit();
	}
}