using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // ============================
    // Panels
    // ============================
    [Header("Panels")]
    [SerializeField] private GameObject startPanel;     // Panel shown at the start menu
    [SerializeField] private GameObject hudPanel;       // Panel showing in-game HUD (score, moves, etc.)
    [SerializeField] private GameObject gameOverPanel;  // Panel shown when the game ends

    // ============================
    // HUD Texts
    // ============================
    [Header("HUD Texts")]
    [SerializeField] private TMP_Text scoreText;        // Displays current score
    [SerializeField] private TMP_Text movesText;        // Displays remaining moves or moves made
    [SerializeField] private TMP_Text matchesText;      // Displays number of successful matches
    [SerializeField] private TMP_Text highScoreText;    // Displays the high score

    // ============================
    // Panel Control Methods
    // ============================

    /// <summary>
    /// Shows the Start Panel and hides other panels.
    /// Called at game launch or when returning to main menu.
    /// </summary>
    public void ShowStart()
    {
        startPanel.SetActive(true);
        hudPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    /// <summary>
    /// Shows the HUD panel during gameplay and hides other panels.
    /// </summary>
    public void ShowHUD()
    {
        startPanel.SetActive(false);
        hudPanel.SetActive(true);
        gameOverPanel.SetActive(false);
    }

    /// <summary>
    /// Shows the Game Over panel.
    /// Optionally, can also hide other panels if needed.
    /// </summary>
    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    // ============================
    // HUD Update Method
    // ============================

    /// <summary>
    /// Updates all HUD text elements with the current game values.
    /// </summary>
    /// <param name="score">Current player score</param>
    /// <param name="moves">Moves made</param>
    /// <param name="matches">Number of successful matches</param>
    /// <param name="highScore">Highest score achieved</param>
    public void UpdateHUD(int score, int moves, int matches, int highScore)
    {
        scoreText.text = $"Score: {score}";
        movesText.text = $"Moves: {moves}";
        matchesText.text = $"Matches: {matches}";
        highScoreText.text = $"High Score: {highScore}";
    }
}
