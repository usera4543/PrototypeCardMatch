using System.Collections.Generic;
using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    [Header("Layout")]
    public RectTransform gameArea;
    public int rows = 4;
    public int cols = 4;
    public float spacing = 8f;

    [Header("Pool Key")]
    [SerializeField] private string poolKey = "Card";

    List<GameObject> spawned = new List<GameObject>();

    public void BuildGrid(List<int> deckSymbols, List<Sprite> symbolSprites, float flipDuration)
    {
        Clear();

        float width = gameArea.rect.width;
        float height = gameArea.rect.height;
        float cellW = (width - (cols - 1) * spacing) / cols;
        float cellH = (height - (rows - 1) * spacing) / rows;
        float size = Mathf.Min(cellW, cellH);

        // Need to fix this
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int idx = y * cols + x;
                var go = PoolManager.I.Get(poolKey);
                go.transform.SetParent(gameArea, false);
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(size, size);
                // anchored position (top-left origin)
                float posX = x * (size + spacing);
                float posY = -y * (size + spacing);
                rt.anchoredPosition = new Vector2(posX, posY);
                var card = go.GetComponent<Card>();
                int symbol = deckSymbols[idx];
                card.Setup(symbol, symbolSprites[symbol], flipDuration);
                spawned.Add(go);
            }
        }

        // center grid by offsetting
        CenterGrid(size);
    }

    void CenterGrid(float cellSize)
    {
        float gridW = cols * cellSize + (cols - 1) * spacing;
        float gridH = rows * cellSize + (rows - 1) * spacing;
        // compute offset to center
        float offsetX = (gameArea.rect.width - gridW) / 2f;
        float offsetY = (gameArea.rect.height - gridH) / 2f;
        foreach (var go in spawned)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition += new Vector2(offsetX, -offsetY);
        }
    }

    public void Clear()
    {
        foreach (var g in spawned)
        {
            PoolManager.I.Return(poolKey, g);
        }
        spawned.Clear();
    }

    public int RemainingActiveCards()
    {
        int cnt = 0;
        foreach (var g in spawned) if (g.activeInHierarchy) cnt++;
        return cnt;
    }
}