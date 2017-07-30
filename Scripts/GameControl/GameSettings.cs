using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSettings {

    public const string gameVersion = "0.84";
    public static bool timedMode = true;
    public static float turnDuration = 30f;
    public static byte teamSize = 4;
    public static bool crateSpawnEnabled = true;
    public static int crateSpawnChance = 7; 
    public static bool usePointingArrow = true;

    public static string[] teamColors = { "Blue", "Red", "Green", "Yellow" };
}
