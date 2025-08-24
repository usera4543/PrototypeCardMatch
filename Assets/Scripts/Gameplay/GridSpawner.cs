using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dynamically spawns a grid of card objects inside a given RectTransform.
/// Uses object pooling for performance.
/// </summary>
public class GridSpawner : MonoBehaviour
{
    [Header("Layout")]
    [Tooltip("The parent RectTransform where the grid of cards will be placed.")]
    [SerializeField] private RectTransform gameArea;

    [Tooltip("Spacing between each card in the grid.")]
    [SerializeField] private float spacing = 8f;

    [Header("Pool Key")]
    [Tooltip("The object pool key used to fetch card prefabs.")]
    [SerializeField] private string poolKey = "Card";

    // Keeps track of all currently spawned card GameObjects
    private readonly List<GameObject> spawned = new List<GameObject>();


    /// <summary>
    /// Builds a grid of cards inside the gameArea.
    /// </summary>
    /// <param name="rows">Number of rows in the grid.</param>
    /// <param name="cols">Number of columns in the grid.</param>
    /// <param name="deckSymbols">List of symbol IDs assigned to each card.</param>
    /// <param name="symbolSprites">List of sprites corresponding to symbols.</param>
    /// <param name="flipDuration">How long each card flip should take.</param>
    public void BuildGrid(int rows, int cols, List<int> deckSymbols, List<Sprite> symbolSprites, float flipDuration)
    {
        // Clear previous grid before spawning new one
        Clear();

        // Available area size
        float width = gameArea.rect.width;
        float height = gameArea.rect.height;

        // Compute individual cell size considering spacing
        float cellW = (width - (cols - 1) * spacing) / cols;
        float cellH = (height - (rows - 1) * spacing) / rows;

        // Use square cells, constrained by smallest dimension
        float size = Mathf.Min(cellW, cellH);

        // Compute total grid size
        float gridW = cols * size + (cols - 1) * spacing;
        float gridH = rows * size + (rows - 1) * spacing;

        // Start coordinates (top-left corner of grid relative to center)
        float startX = -gridW / 2f + size / 2f;
        float startY = gridH / 2f - size / 2f;

        // Loop through rows & columns and spawn cards
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int idx = y * cols + x; // card index

                // Get card from object pool
                var go = PoolManager.I.Get(poolKey);
                go.transform.SetParent(gameArea, false);

                // Resize card to fit cell
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(size, size);

                // Position card in grid
                float posX = startX + x * (size + spacing);
                float posY = startY - y * (size + spacing);
                rt.anchoredPosition = new Vector2(posX, posY);

                // Setup card content (symbol + sprite + flip duration)
                var card = go.GetComponent<Card>();
                int symbol = deckSymbols[idx];
                card.Setup(symbol, symbolSprites[symbol], flipDuration);

                // Track spawned cards for later cleanup
                spawned.Add(go);
            }
        }
    }

    /// <summary>
    /// Clears all spawned cards by returning them to the pool.
    /// </summary>
    public void Clear()
    {
        foreach (var g in spawned)
        {
            PoolManager.I.Return(poolKey, g);
        }
        spawned.Clear();
    }

    /// <summary>
    /// Returns how many cards are currently active in the grid.
    /// </summary>
    public int RemainingActiveCards()
    {
        int cnt = 0;
        foreach (var g in spawned)
        {
            if (g.activeInHierarchy) cnt++;
        }
        return cnt;
    }
}
