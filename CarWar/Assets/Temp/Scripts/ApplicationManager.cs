using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ApplicationManager : MonoBehaviour 
{
	public void NewGame() 
	{
		SaveGameManager.DeleteSave();
		SceneManager.LoadScene(1);
	}

	public void Continue() 
	{
		SceneManager.LoadScene(1);
	}

	public void MainMenu()
	{
		SceneManager.LoadScene(0);
	}


	public void Quit () 
	{
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}
}
