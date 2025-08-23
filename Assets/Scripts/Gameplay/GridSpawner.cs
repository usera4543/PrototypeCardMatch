using System.Collections.Generic;
using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private RectTransform gameArea;
    public float spacing = 8f;

    [Header("Pool Key")]
    [SerializeField] private string poolKey = "Card";

    List<GameObject> spawned = new List<GameObject>();

    public void BuildGrid(int rows, int cols, List<int> deckSymbols, List<Sprite> symbolSprites, float flipDuration)
    {
        Clear();

        float width = gameArea.rect.width;
        float height = gameArea.rect.height;
        float cellW = (width - (cols - 1) * spacing) / cols;
        float cellH = (height - (rows - 1) * spacing) / rows;
        float size = Mathf.Min(cellW, cellH);

        // compute grid total size
        float gridW = cols * size + (cols - 1) * spacing;
        float gridH = rows * size + (rows - 1) * spacing;

        float startX = -gridW / 2f + size / 2f;
        float startY = gridH / 2f - size / 2f;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int idx = y * cols + x;
                var go = PoolManager.I.Get(poolKey);
                go.transform.SetParent(gameArea, false);

                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(size, size);

                float posX = startX + x * (size + spacing);
                float posY = startY - y * (size + spacing);
                rt.anchoredPosition = new Vector2(posX, posY);

                var card = go.GetComponent<Card>();
                int symbol = deckSymbols[idx];
                card.Setup(symbol, symbolSprites[symbol], flipDuration);

                spawned.Add(go);
            }
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