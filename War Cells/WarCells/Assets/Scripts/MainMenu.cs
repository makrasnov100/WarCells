using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public InputField inPlayers;
    public Dropdown inMapSize;

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
            GameObject mg = GameObject.FindGameObjectWithTag("mapGenerator");
            if(pm != null) //If player manager exists - Setup settings
                pm.GetComponent<PlayerManager>().SetHumanPlayers(int.Parse(inPlayers.text));

            if (mg != null) //If mapGenerator exists - Setup map settings
            {
                int tempMapSizeVar = 4 * (1 +  inMapSize.value);
                mg.GetComponent<MapGenerator>().SetMapSize(tempMapSizeVar);

            }
                
            Destroy(gameObject);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }



}

