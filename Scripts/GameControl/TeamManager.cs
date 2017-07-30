using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TeamManager {

    static List<Team> teams;

    public static List<Team> getTeams { get { return teams; } }
    public static Team getTeamByID(int id) { return teams.Find(x => x.player.ID == id); }
    public static Team getLocalTeam () { return getTeamByID(PhotonNetwork.player.ID); }

    public static List<Team> aliveTeams { get { return teams.FindAll(x => x.getUnits != null && x.hasUnits); } } 
    public static int FindNextAliveIndex (int currentIndex)
    {
        int i = currentIndex + 1;
        for(int x = 0; x < aliveTeams.Count; x++)
        {
            if (i >= aliveTeams.Count)
                i = 0;
            if (aliveTeams[i].hasUnits)
                return i;
            i++;
        }
        return -1;
    }

    public static void Initialize ()
    {
        teams = new List<Team>();
    }

    public static void AddTeam (Team team)
    {
        if (!teams.Contains(team))
            teams.Add(team);
        else
            Debug.LogWarning("Duplicate team added, ignoring");
    }
}

public class Team
{
    public readonly PhotonPlayer player;
    public readonly string color;

    List<Player> units;

    public bool hasUnits { get { return units != null && units.Count > 0; } }
    public List<Player> getUnits { get { return units; } }

    public Team (PhotonPlayer player, string color)
    {
        this.player = player;
        this.color = color;
        this.units = new List<Player>();
    }

    public void SetUnits (List<Player> units)
    {
        this.units = units;
    }

    public void ClearUnits ()
    {
        this.units = new List<Player>();
    }

    public void AddUnit (Player unit)
    {
        this.units.Add(unit);
    }

    public void RemoveUnit (Player unit)
    {
        if(this.units.Contains(unit))
        {
            this.units.Remove(unit);
        }
    }
}


