using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Lobby 
{
    private string lobbyName;
    private string password;
    private string ownerName;
    private int curPlayers;
    private GameObject lobbyObj;

    public void Construct(string lname, string pass, string owner, int curPlayers)
    {
        this.lobbyName = lname;
        this.password = pass;
        this.ownerName = owner;
        this.curPlayers = curPlayers;
    }

    public void SetupLabels()
    {
        if (lobbyObj == null)
            return;

        lobbyObj.transform.Find("LobbyName").GetComponent<Text>().text = lobbyName;
        if(password == "")
            lobbyObj.transform.Find("Locked").GetComponent<Text>().text = "Locked";
        else
            lobbyObj.transform.Find("Locked").GetComponent<Text>().text = "Public";
        lobbyObj.transform.Find("HostName").GetComponent<Text>().text = ownerName;
        lobbyObj.transform.Find("CurrentPlayers").GetComponent<Text>().text = curPlayers + " / 10";
    }

    public GameObject GetLobbyObj() { return lobbyObj; }
    public void SetLobbyObj(GameObject obj) { lobbyObj = obj; }

}
public class LobbyManager : MonoBehaviour
{
    public GameObject newLobbyPrefab;
    public Transform lobbyHolder;
    private List<Lobby> curLobbies = new List<Lobby>();

    private void Start()
    {
        CreateLobby("FizzKillsAll", "", "Fizz",1);
        CreateLobby("RussianOnlyxoox", "1234", "Kostia", 1);
        CreateLobby("Randoms", "", "ThatGuy", 2);
    }

    void CreateLobby(string lname, string pass, string owner, int curPlayers)
    {
        Lobby newLobby = new Lobby();
        newLobby.Construct(lname, pass, owner, curPlayers);
        curLobbies.Add(newLobby);
        RegenerateLobbies();
    }

    void RegenerateLobbies()
    {
        int lobbyPad = -60;
        //Destorys all current lobbies generated
        foreach (Lobby lobby in curLobbies)
        {
            GameObject obj = lobby.GetLobbyObj();
            if (obj != null)
            {
                Destroy(obj);
                lobby.SetLobbyObj(null);
            }
        }
        //creates all available lobbies.
        foreach (Lobby lobby in curLobbies)
        {
            GameObject lobbyObj = Instantiate(newLobbyPrefab);
            lobbyObj.transform.SetParent(lobbyHolder);
            lobbyObj.transform.localPosition = new Vector3(0, lobbyPad, 0);
            lobby.SetLobbyObj(lobbyObj);
            lobby.SetupLabels();
            lobbyPad -= 60;
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu"); 
    }
}
