using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player //TODO: add accessors mutators
{
    int id;
    bool isBot;
    int unitAmount;
    public List<GameObject> ownedCells = new List<GameObject>();
    Color color;

    public Player(int id, bool isBot, GameObject spawnCell, Color color)
    {
        this.id = id;
        this.isBot = isBot;
        this.color = color;
        unitAmount = 1;
        ownedCells.Add(spawnCell);
    }

    public bool GetIsBot()
    {
        return isBot;
    }

    public Color GetColor()
    {
        return color;
    }
}

public class PlayerManager : MonoBehaviour
{

    //Player Settings
    public int humanPlayers;
    public int botPlayers;

    List<List<GameObject>> cells;
    List<Player> players = new List<Player>();

    public void Construct(List<List<GameObject>> cells)
    {
        this.cells = cells;
    }

    // Start is called before the first frame update
    public void SpawnPlayers()
    {
        int totalPlayers = humanPlayers + botPlayers;
        for (int p = 0; p < totalPlayers; p++)
        {
            GameObject curCell = cells[Random.Range(0, cells.Count)][Random.Range(0, cells[0].Count)];
            while (curCell == null || curCell.GetComponent<CellIdentity>().GetOwner() != -1)
            {
                curCell = cells[Random.Range(0, cells.Count)][Random.Range(0, cells[0].Count)];
            }
            Color curColor = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f);
            //curCell.transform.Find("Sprite").GetComponent<SpriteRenderer>().color = curColor;
            curCell.GetComponent<CellIdentity>().SetOccupancy(1);
            curCell.GetComponent<CellIdentity>().SetOwner(p, curColor);

            bool isBot = true;
            if (p < humanPlayers)
                isBot = false;
            players.Add(new Player(p, isBot, curCell, curColor));
        }
    }

    public void CompleteCellActions()
    {
        foreach (Player p in players)
        {
            foreach (GameObject g in p.ownedCells)
            {
                g.GetComponent<CellIdentity>().CompleteTurn();
            }
        }
    }

    //MUTATORS/ACCESSORS
    public List<Player> GetPlayers()
    {
        return players;
    }
}
