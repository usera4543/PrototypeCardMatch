using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("HUD Texts")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text matchesText;
    [SerializeField] private TMP_Text highScoreText;

    public void ShowStart()
    {
        startPanel.SetActive(true);
        hudPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void ShowHUD()
    {
        startPanel.SetActive(false);
        hudPanel.SetActive(true);
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    public void UpdateHUD(int score, int moves, int matches, int highScore)
    {
        scoreText.text = $"Score: {score}";
        movesText.text = $"Moves: {moves}";
        matchesText.text = $"Matches: {matches}";
        highScoreText.text = $"High Score: {highScore}";
    }
}