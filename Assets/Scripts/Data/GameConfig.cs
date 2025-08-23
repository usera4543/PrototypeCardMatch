using UnityEngine;

[CreateAssetMenu(menuName = "CardMatch/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Board")]
    public int rows = 2;
    public int cols = 3;
    public int seed = 0;

    [Header("Timings")]
    public float flipDuration = 0.25f;
    public float compareDelay = 0.6f;

    [Header("Scoring")]
    public int matchScore = 100;
    public int mismatchPenalty = 10;
}