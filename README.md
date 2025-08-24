# PrototypeCardMatch

Prototype project built in *Unity 2021.3.45f1 LTS* for the Senior Game Developer test.

## Features
- Dynamic board sizes (2x2 up to 6x6) driven by GameConfig ScriptableObject
- Continuous flipping (no input lock, multiple flips allowed at once)
- Smooth card flip animations
- Matching & mismatch logic with audio feedback
- Scoring system:
  - Score
  - Moves
  - Matches
  - High Score (persistent)
- Save/Load system:
  - High Score is saved and loaded via PlayerPrefs
- UI Flow:
  - Start Menu with Play button
  - HUD (Score, Moves, Matches, High Score)
  - Game Over panel with *Restart Random* and *Home* buttons
- Audio: flip, match, mismatch, and game over events

## Notes
- In this prototype, *only High Score is persisted* across play sessions using Unity's PlayerPrefs.
- This was chosen as it satisfies the test requirement while keeping the implementation simple.
- For a production-ready version, I would extend the Save/Load system to include:
  - Current board size (rows/cols)
  - Deck order and shuffled symbols
  - Card states (matched, face-up, face-down)
  - Current score, moves, and matches
- This can be achieved using:
  - *JSON serialization* to save state into a file under Application.persistentDataPath, or
  - Unity's *ScriptableObjects* or third-party persistence solutions.

## How to Run
1. Clone the repository:
   ```bash
   git clone https://github.com/usera4543/PrototypeCardMatch.git