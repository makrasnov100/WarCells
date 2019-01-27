using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    BotManager botManager;
    int id;
    bool isBot;
    string botDifficulty;
    bool isDead;
    int unitAmount;
    public HashSet<GameObject> ownedCells = new HashSet<GameObject>();
    Color color;

    ///[CONSTRUCTOR]
    public Player(int id, bool isBot, string difficulty, GameObject spawnCell, Color color)
    {
        this.id = id;
        this.isBot = isBot;
        this.botDifficulty = difficulty;
        this.isDead = false;
        this.color = color;
        botManager = GameObject.FindGameObjectWithTag("playerManager").GetComponent<BotManager>();
        unitAmount = 1;
        ownedCells.Add(spawnCell);
    }
    public Player() { }
    public void BotMove()
    {
        botManager.ProcessTurn(ownedCells, botDifficulty);
    }

    ///[ACCESSOR(S)/MUTATOR(S)]
    public int GetId() { return id; }
    public bool GetIsBot() { return isBot; }
    public string GetBotDifficulty() { return botDifficulty; }
    public bool GetIsDead() { return isDead; }
    public void SetIsDead(bool newDead) { isDead = newDead; }
    public Color GetPlayerColor() { return color; }
    public HashSet<GameObject> GetOwnedCells() { return ownedCells; }

    public void AddCell(GameObject newCell) { ownedCells.Add(newCell); }
    public void RemoveCell(GameObject oldCell) { ownedCells.Remove(oldCell); }
}

public class PlayerManager : MonoBehaviour
{

    //Player Type Settings
    public int humanPlayers;
    public int easyBots;
    public int mediumBots;
    public int hardBots;

    //Game Storage
    List<GameObject> cells = new List<GameObject>();    // - cells
    List<Player> players = new List<Player>();          // - players


    ///[CONSTRUCTORS*]
    public void Construct(List<GameObject> cells)
    {
        this.cells = cells;
    }
    public void Construct(List<List<GameObject>> newCells)
    {
        for (int y = 0; y < newCells.Count; y++)
            for(int x = 0; x < newCells[y].Count; x++)
                if(newCells[y][x] != null)
                    this.cells.Add(newCells[y][x]);
    }


    ///[PLAYER CONTROL]
    //Adds a preset amount of players to the map
    public void SpawnPlayers()
    {
        int easyBotsLeft = easyBots;
        int mediumBotsLeft = mediumBots;
        int hardBotsLeft = hardBots;

        int totalPlayers = humanPlayers + easyBots + mediumBots + hardBots;
        for (int p = 0; p < totalPlayers; p++)
        {
            //Find an open cell for a player
            GameObject curCell = cells[Random.Range(0, cells.Count)];
            while (curCell == null || curCell.GetComponent<CellIdentity>().GetOwner() != -1)
            {
                curCell = cells[Random.Range(0, cells.Count)];
            }

            Color curColor = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f);   //SET color for currently created player
            curCell.GetComponent<CellIdentity>().SetOccupancy(5);               //SET initial player unit amount
            curCell.GetComponent<CellIdentity>().SetOwner(p, curColor);         //SET player as owner of found starting cell
            curCell.GetComponent<CellIdentity>().SetOriginalOwner(p);

            bool isBot = true;                                                  //SET player bot status (after all human players are in)
            if (p < humanPlayers)
                isBot = false;


            //Add current player to player storage
            if(!isBot) // if player, add then continue
            {
                players.Add(new Player(p, isBot, "Player", curCell, curColor));
            }

            //Assign difficulty to bots - (if it is bot)
            if(isBot && easyBotsLeft > 0)
            {
                easyBotsLeft--;
                players.Add(new Player(p, isBot, "Easy" , curCell, curColor));
            }
            else if (isBot && mediumBotsLeft > 0)
            {
                mediumBotsLeft--;
                players.Add(new Player(p, isBot, "Medium", curCell, curColor));
            }
            else if (isBot && hardBotsLeft > 0)
            {
                hardBotsLeft--;
                players.Add(new Player(p, isBot, "Hard", curCell, curColor));
            }
        }
    }


    ///[CELL FUNCTIONS]
    //CompleteCellActions: Perform end turn procedure for each cell thats owned by a player or bot
    public void CompleteCellActions()
    {
        foreach (Player p in players)
            foreach (GameObject g in p.ownedCells)
                g.GetComponent<CellIdentity>().CompleteTurn();
    }

    //ShowAllAttackDeclarations: Displayes the declared attacks of all owned cells
    public void ShowAllAttackDeclarations()
    {
        foreach (Player p in players)
            foreach (GameObject g in p.ownedCells)
                g.GetComponent<CellIdentity>().SetBulkConnectionColor(false, true);
    }

    //ShowCurPlayerAttackDeclarations: Displays the declared attacks of all cells owned by a specific player
    public void ShowCurPlayerAttackDeclarations(int playerId)
    {
        foreach (Player p in players)
        {
            if (p.GetId() == playerId)
            {
                foreach (GameObject g in p.ownedCells)
                    g.GetComponent<CellIdentity>().SetBulkConnectionColor(false, false);
            }
            else
            {
                foreach (GameObject g in p.ownedCells)
                    g.GetComponent<CellIdentity>().ResetOutgoingColors(Color.white);
            }
        }
    }


    ///[ACCESSOR(S)/MUTATOR(S)]
    public List<Player> GetPlayers() { return players; }
    public Player GetPlayer(int searchId)
    {
        foreach(Player p in players)
        {
            if (p.GetId() == searchId)
                return p;
        }
        return null;
    }
    public void SetHumanPlayers(int newPlayers) { humanPlayers = newPlayers;  }
    public Player GetNextAlivePlayer(int previousPlayer)
    {
        if (previousPlayer == -1)
            return players[0];

        int nextPlayer = previousPlayer;
        do
        {
            nextPlayer++;
            if (nextPlayer == players.Count)
                nextPlayer = 0;

        } while (nextPlayer != previousPlayer && players[nextPlayer].GetIsDead());
        //players[nextPlayer].GetIsBot()

        return players[nextPlayer];
    }
    public int GetTotalAlivePlayers()
    {
        int totalAlive = 0;
        //Find next player in list
        foreach (Player p in players)
            if (!p.GetIsDead())
                totalAlive++;

        return totalAlive;
    }   
}
