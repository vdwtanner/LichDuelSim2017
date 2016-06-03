using UnityEngine;
using System.Collections;

public class Config {
    /// <summary>
    /// This should be at god scale
    /// </summary>
    public static float HMDStadningHeight;

    public static float godScale = 10;

    public static float godTeleportationHeightOffset = 0;

    /// <summary>
    /// The name of the controller that the player has chosen as their dominant hand
    /// </summary>
    public static string dominantHand = "Controller (right)";
    /// <summary>
    /// The name of the controller that the player has chosen as their off hand (non-dominant)
    /// </summary>
    public static string offHand = "Controller (left)";
}
