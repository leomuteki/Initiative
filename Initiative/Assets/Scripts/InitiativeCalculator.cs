using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Global struct 
public struct Player
{
    public string name;
    public int modifier;
    public int initiative;
    
    public Player(string new_name, int new_modifier, int new_initiative)
    {
        name = new_name;
        modifier = new_modifier;
        initiative = new_initiative;
    }

    public void SetInitiative(int i)
    {
        initiative = i;
    }
}

public class InitiativeCalculator : MonoBehaviour {

    // Constants
    private int DiceSize;
    [SerializeField, Range(0, 100)] private int DefaultDiceSize = 10;
    [SerializeField] private InputField DiceField;

    // References
    [SerializeField] private CanvasManager Canvas;
    [SerializeField] public Text Output;

    // Use this for initialization
    void Start()
    {
        DiceSize = DefaultDiceSize;
        DiceField.text = "10";
    }

    public void CalculateInitiative()
    {
        Canvas.ResetContent();
        // Dice size input validation
        try
        {
            DiceSize = int.Parse(DiceField.text);
            if (DiceSize < 0)
            {
                DiceSize = 0;
            }
        }
        catch
        {
            DiceSize = DefaultDiceSize;
        }

        // Get data
        List<Player> players = Canvas.GetData();

        // Sort by Initiative and solve any ties
        players = SortInitiative(players);
        
        // Print Results
        Output.text = "";
        foreach (Player p in players)
        {
            Output.text += "(" + p.initiative + ") " + p.name + "\n\n";
        }
    }

    public int GetRandRoll()
    {
        return Random.Range(0, DiceSize) + 1;
    }

    /// <summary>
    /// Performs selection sort which allows time to sort any ties
    /// </summary>
    /// <param name="players"></param>
    /// <returns></returns>
    private List<Player> SortInitiative(List<Player> players)
    {
        for (int i = 0; i < players.Count - 1; ++i)
        {
            int max_index = i;
            for (int j = i + 1; j < players.Count; ++j)
            {
                // if the players[j] is smaller then it is the new min
                if (ComparePlayers(players[max_index], players[j]) < 0)
                {
                    max_index = j;
                }
                else if (ComparePlayers(players[max_index], players[j]) == 0)
                {
                    // Break a tie
                    int rand_num_1 = GetRandRoll();
                    int rand_num_2 = GetRandRoll();
                    // Reroll until tie is broken
                    while (rand_num_1 == rand_num_2)
                    {
                        rand_num_1 = GetRandRoll();
                        rand_num_2 = GetRandRoll();
                    }

                    if (rand_num_1 < rand_num_2)
                    {
                        max_index = j;
                    }
                }
            }
            // min is now the next smallest. if it is not player[i], swap with player[i]
            if (max_index != i)
            {
                Player temp = players[i];
                players[i] = players[max_index];
                players[max_index] = temp;
            }
        }
        return players;
    }

    /// <summary>
    /// Returns if p1 < p2: returns -1, 0 if they are equal, otherwise 1.
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    private int ComparePlayers(Player p1, Player p2)
    {
        if (p1.initiative < p2.initiative)
        {
            return -1;
        }
        else if (p1.initiative == p2.initiative)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }
    
}
