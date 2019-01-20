using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    int id;
    bool isBot;
    int unitAmount;
    public HashSet<GameObject> ownedCells = new HashSet<GameObject>();
    Color color;

    ///[CONSTRUCTOR]
    public Player(int id, bool isBot, GameObject spawnCell, Color color)
    {
        this.id = id;
        this.isBot = isBot;
        this.color = color;
        unitAmount = 1;
        ownedCells.Add(spawnCell);
    }

    ///[ACCESSOR(S)/MUTATOR(S)]
    public int GetId() { return id; }
    public bool GetIsBot() { return isBot; }

    public void AddCell(GameObject newCell) { ownedCells.Add(newCell); }
    public void RemoveCell(GameObject oldCell) { ownedCells.Remove(oldCell); }
}

public class PlayerManager : MonoBehaviour
{

    //Player Type Settings
    public int humanPlayers;
    public int botPlayers;
    
    //Game Storage
    List<List<GameObject>> cells;               // - cells
    List<Player> players = new List<Player>();  // - players

    ///[CONSTRUCTOR*]
    public void Construct(List<List<GameObject>> cells)
    {
        this.cells = cells;
    }

    ///[PLAYER CONTROL]
    //Adds a preset amount of player to the map
    public void SpawnPlayers()
    {
        int totalPlayers = humanPlayers + botPlayers;
        for (int p = 0; p < totalPlayers; p++)
        {
            //Find an open cell for a player
            GameObject curCell = cells[Random.Range(0, cells.Count)][Random.Range(0, cells[0].Count)];
            while (curCell == null || curCell.GetComponent<CellIdentity>().GetOwner() != -1)
            {
                curCell = cells[Random.Range(0, cells.Count)][Random.Range(0, cells[0].Count)];
            }

            Color curColor = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f);   //SET color for currently created player
            curCell.GetComponent<CellIdentity>().SetOccupancy(5);               //SET initial player unit amount
            curCell.GetComponent<CellIdentity>().SetOwner(p, curColor);         //SET player as owner of found starting cell

            bool isBot = true;                                                  //SET player bot status
            if (p < humanPlayers)
                isBot = false;

            //Add created player to storage
            players.Add(new Player(p, isBot, curCell, curColor));
        }
    }

    ///[CELL FUNCTIONS]
    //Perform end turn procedure for each cell thats owned by a player or bot
    public void CompleteCellActions()
    {
        foreach (Player p in players)
            foreach (GameObject g in p.ownedCells)
                g.GetComponent<CellIdentity>().CompleteTurn();
    }

    ///[ACCESSOR(S)/MUTATOR(S)]
    public List<Player> GetPlayers() { return players; }
}
