using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player //TODO: add accessors mutators
{
    int id;
    bool isBot;
    int unitAmount;
    public HashSet<GameObject> ownedCells = new HashSet<GameObject>();
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
    public int GetId()
    {
        return id;
    }
    public void AddCell(GameObject newCell)
    {
        ownedCells.Add(newCell);
    }
    public void RemoveCell(GameObject oldCell)
    {
        ownedCells.Remove(oldCell);
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
        string output = "";
        foreach (Player p in players)
        {
            output += "Player " + p.GetId() + " cells - " + System.Environment.NewLine;
            foreach (GameObject g in p.ownedCells)
            {
                output += " - " + g.name + System.Environment.NewLine;
                g.GetComponent<CellIdentity>().CompleteTurn();
            }
            output += "-------------------------------------" + System.Environment.NewLine;
        }
        print(output);
    }

    //MUTATORS/ACCESSORS
    public List<Player> GetPlayers()
    {
        return players;
    }
}
