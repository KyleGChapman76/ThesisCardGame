using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
	public GameObject networkManager;
	public string gameLevelName;

	public void BeginGameSingleplayer()
	{
		Destroy(networkManager);
		SceneManager.LoadScene(gameLevelName);
	}
}
