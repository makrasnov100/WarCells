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
            GameObject mg = null;
            GameObject mgNorm = GameObject.FindGameObjectWithTag("mapGenerator");
            GameObject mgExp = GameObject.FindGameObjectWithTag("mapGeneratorExp");

            if (mgNorm != null)
                mg = mgNorm;
            else if (mgExp != null)
                mg = mgExp;

            if (pm != null)
                pm.GetComponent<PlayerManager>().SetHumanPlayers(int.Parse(inPlayers.text));
            if (mg != null)
            {
                int tempMapSizeVar = 4 * (1 +  inMapSize.value);
                mg.GetComponent<MapGenerator>().SetMapSize(tempMapSizeVar);
            }
                
            Destroy(gameObject);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }



}

