using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public InputField inPlayers;

    public void OnPlayBtn()
    {
        Object.DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("GameScene");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            GameObject pm = GameObject.FindGameObjectWithTag("playerManager");
            pm.GetComponent<PlayerManager>().SetHumanPlayers(int.Parse(inPlayers.text));
            Destroy(gameObject);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }



}

