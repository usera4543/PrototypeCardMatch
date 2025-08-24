using UnityEngine;

[CreateAssetMenu(menuName = "CardMatch/GameConfig")]
public class GameConfig : ScriptableObject
{
    //Start

    [Header("Start Game On Play Button -> (Customize as required)")]
    [Tooltip("Number of rows for the initial game board. Can be adjusted with slider.")]
    [Range(2, 4)] // Set min/max as required
    public int rows = 2;

    [Tooltip("Number of columns for the initial game board. Can be adjusted with slider.")]
    [Range(2, 4)] // Set min/max as required
    public int cols = 3;


    //Timing

    [Header("Timings")]
    [Tooltip("Time it takes for a card to flip over.")]
    public float flipDuration = 0.25f;

    [Tooltip("Delay before comparing two flipped cards.")]
    public float compareDelay = 0.5f;

    [Header("Scoring")]
    [Tooltip("Score awarded for a correct match.")]
    public int matchScore = 100;

    [Tooltip("Penalty for a mismatch.")]
    public int mismatchPenalty = 10;


    //Restart

    [Header("Restart Game Button -> Random Board Range")]

    [Tooltip("Minimum rows allowed for random board.")]
    public int minRows = 2;

    [Tooltip("Maximum rows allowed for random board.")]
    public int maxRows = 4;

    [Tooltip("Minimum columns allowed for random board.")]
    public int minCols = 2;

    [Tooltip("Maximum columns allowed for random board.")]
    public int maxCols = 4;
}
