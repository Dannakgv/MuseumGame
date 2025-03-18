using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the sliding puzzle game mechanics including initialization, piece movement,
/// shuffling, and win condition detection.
/// </summary>
public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private Transform gameTransform;    // Parent transform for all puzzle pieces
    [SerializeField] private Transform piecePrefab;      // Prefab used to create each puzzle piece
    [SerializeField] private GameObject winScreen;       // UI element shown when puzzle is completed

    private List<Transform> pieces;       // Collection of all puzzle pieces
    private int emptyLocation;            // Index of the empty space in the puzzle
    private int size;                     // Puzzle dimensions (size x size grid)
    private bool shuffling = false;       // Flag to prevent player input during shuffle
    private bool gameStarted = false;     // Flag to track if gameplay has begun
    private Material puzzleMaterial;      // Material with the image used for the puzzle

    /// <summary>
    /// Initializes the puzzle by loading level data, creating pieces, and shuffling the board.
    /// </summary>
    void Start()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError("LevelManager instance not found!");
            return;
        }

        // Load current level data from the LevelManager
        LevelData level = LevelManager.Instance.GetCurrentLevel();
        if (level == null) return;

        size = level.size;
        Material puzzleMaterial = Resources.Load<Material>($"Materials/{level.material}");

        if (puzzleMaterial == null)
        {
            Debug.LogError($"Material '{level.material}' not found!");
            return;
        }

        // Set up the puzzle board with the specified pieces and material
        CreateGamePieces(0.01f, puzzleMaterial);
        winScreen.SetActive(false);
        gameStarted = false;

        // Delay shuffling to allow the board to display properly first
        StartCoroutine(WaitShuffle(0.5f));
        shuffling = true;
    }

    /// <summary>
    /// Handles player input for moving puzzle pieces.
    /// </summary>
    void Update()
    {
        if (shuffling) return; // Block interaction while puzzle is being shuffled

        // Detect touch input
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPosition = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero);

            if (hit)
            {
                // Find which piece was touched and try to move it
                for (int i = 0; i < pieces.Count; i++)
                {
                    if (pieces[i] == hit.transform)
                    {
                        Debug.Log($"Trying to move piece {i}");

                        // Try to move in all four directions (up, down, left, right)
                        if (SwapIfValid(i, -size, size)) { Debug.Log("Moved Up"); break; }
                        if (SwapIfValid(i, +size, size)) { Debug.Log("Moved Down"); break; }
                        if (SwapIfValid(i, -1, 0)) { Debug.Log("Moved Left"); break; }
                        if (SwapIfValid(i, +1, size - 1)) { Debug.Log("Moved Right"); break; }
                    }
                }
            }
            else
            {
                Debug.Log("No piece was clicked!");
            }
        }
    }

    /// <summary>
    /// Creates all puzzle pieces with proper positioning and texturing.
    /// </summary>
    /// <param name="gapThickness">Spacing between puzzle pieces</param>
    /// <param name="material">Material with the puzzle image</param>
    private void CreateGamePieces(float gapThickness, Material material)
    {
        if (piecePrefab == null)
        {
            Debug.LogError("piecePrefab is not assigned!");
            return;
        }

        if (material == null)
        {
            Debug.LogError("puzzleMaterial is not assigned!");
            return;
        }

        puzzleMaterial = material; // Store material reference
        pieces = new List<Transform>();

        // Get the aspect ratio of the texture to ensure pieces are properly proportioned
        Texture2D texture = (Texture2D)puzzleMaterial.mainTexture;
        if (texture == null)
        {
            Debug.LogError("Texture not found in puzzleMaterial!");
            return;
        }

        float aspectRatio = (float)texture.width / texture.height;

        // Calculate dimensions for each puzzle piece
        float pieceWidth = 1 / (float)size;
        float pieceHeight = pieceWidth / aspectRatio; // Adjust height based on aspect ratio

        Debug.Log($"Creating {size * size} pieces with width={pieceWidth}, height={pieceHeight}");

        // Create grid of puzzle pieces
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameTransform);
                if (piece == null)
                {
                    Debug.LogError("Failed to instantiate piecePrefab!");
                    continue;
                }

                pieces.Add(piece);

                // Position each piece in the grid accounting for aspect ratio
                piece.localPosition = new Vector3(
                    -1 + (2 * pieceWidth * col) + pieceWidth,
                    +1 - (2 * pieceHeight * row) - pieceHeight,
                    0
                );

                // Scale pieces with gap between them
                piece.localScale = new Vector3(
                    (2 * pieceWidth) - gapThickness,
                    (2 * pieceHeight) - gapThickness,
                    1
                );

                // Name piece based on its correct position for later win checking
                piece.name = $"{(row * size) + col}";

                // Last piece is designated as the empty space
                if ((row == size - 1) && (col == size - 1))
                {
                    emptyLocation = (size * size) - 1;
                    piece.gameObject.SetActive(false);
                }
                else
                {
                    // Apply material and adjust UVs to show correct portion of the texture
                    MeshRenderer renderer = piece.GetComponent<MeshRenderer>();
                    MeshFilter meshFilter = piece.GetComponent<MeshFilter>();

                    if (renderer != null)
                    {
                        renderer.material = puzzleMaterial;
                        AdjustUVs(meshFilter.mesh, row, col, size);
                    }
                    else
                    {
                        Debug.LogError("MeshRenderer not found on piece!");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Coroutine that adds a delay before shuffling the puzzle.
    /// </summary>
    /// <param name="duration">Wait time in seconds before shuffling</param>
    private IEnumerator WaitShuffle(float duration)
    {
        yield return new WaitForSeconds(duration);
        Shuffle();
        gameStarted = true; // Enable win condition checking after shuffling
        shuffling = false;  // Allow player interaction
    }

    /// <summary>
    /// Randomly shuffles the puzzle pieces.
    /// </summary>
    private void Shuffle()
    {
        int count = 0;
        int last = -1;

        // Perform random valid moves to shuffle the board
        while (count < (size * size * size))
        {
            int rnd = Random.Range(0, size * size);
            if (rnd == last) continue;
            last = emptyLocation;

            // Try to move in all four directions
            if (SwapIfValid(rnd, -size, size) ||
                SwapIfValid(rnd, +size, size) ||
                SwapIfValid(rnd, -1, 0) ||
                SwapIfValid(rnd, +1, size - 1))
            {
                count++;
            }
        }
    }

    /// <summary>
    /// Attempts to swap a piece with the empty space if it's a valid move.
    /// </summary>
    /// <param name="i">Index of the piece to move</param>
    /// <param name="offset">Direction offset (-size: up, +size: down, -1: left, +1: right)</param>
    /// <param name="colCheck">Column boundary check value</param>
    /// <returns>True if the move was valid and performed</returns>
    private bool SwapIfValid(int i, int offset, int colCheck)
    {
        if (((i % size) != colCheck) && ((i + offset) == emptyLocation))
        {
            Debug.Log($"Swapping piece {i} with empty space {emptyLocation}");

            // Swap the pieces and their positions
            (pieces[i], pieces[i + offset]) = (pieces[i + offset], pieces[i]);
            (pieces[i].localPosition, pieces[i + offset].localPosition) =
                (pieces[i + offset].localPosition, pieces[i].localPosition);

            emptyLocation = i;
            
            // Add haptic feedback when pieces move
            if (HapticManager.Instance != null)
            {
                HapticManager.Instance.MediumFeedback();
            }

            // Check if the puzzle is solved after the move
            if (gameStarted && CheckCompletion())
            {
                Debug.Log("Puzzle solved! Win screen should appear!");
                
                // Add success haptic feedback for puzzle completion
                if (HapticManager.Instance != null)
                {
                    HapticManager.Instance.SuccessFeedback();
                }
            }

            return true;
        }

        Debug.Log($"Invalid move for piece {i}");
        
        // Add light haptic feedback for invalid moves
        if (HapticManager.Instance != null)
        {
            HapticManager.Instance.LightFeedback();
        }
        
        return false;
    }

    /// <summary>
    /// Checks if the puzzle is completed by verifying each piece is in its correct position.
    /// </summary>
    /// <returns>True if the puzzle is completed</returns>
    private bool CheckCompletion()
    {
        if (!gameStarted) return false;  // Ignore before game has started

        Debug.Log("Checking if the puzzle is complete...");

        // Check if each piece is in the position matching its name
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].name != $"{i}")
            {
                Debug.Log($"Piece {pieces[i].name} is not in the correct position.");
                return false;
            }
        }

        Debug.Log("Puzzle Completed! Showing WinScreen...");
        shuffling = true; // Prevent further moves
        CompleteLevel();
        return true;
    }

    /// <summary>
    /// Handles completion of the current level.
    /// </summary>
    public void CompleteLevel()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnlockNextLevel();
        }

        // Add success haptic feedback for level completion
        if (HapticManager.Instance != null)
        {
            HapticManager.Instance.SuccessFeedback();
        }

        winScreen.SetActive(true);
    }

    /// <summary>
    /// Advances to the next puzzle level.
    /// </summary>
    public void NextLevel()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadNextLevel();
        }
    }

    /// <summary>
    /// Returns to the level selection screen.
    /// </summary>
    public void BackToLevelManager()
    {
        SceneManager.LoadScene("LevelManager");
        StartCoroutine(WaitForLevelManager());
    }

    /// <summary>
    /// Coroutine to ensure LevelManager is properly initialized after scene change.
    /// </summary>
    private IEnumerator WaitForLevelManager()
    {
        yield return new WaitForSeconds(0.2f);

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ReassignLevelGrid();
        }
        else
        {
            Debug.LogError("LevelManager instance missing after scene reload!");
        }
    }

    /// <summary>
    /// Maps the correct portion of the texture to each puzzle piece.
    /// </summary>
    /// <param name="mesh">The mesh to adjust UVs for</param>
    /// <param name="row">Row position in the grid</param>
    /// <param name="col">Column position in the grid</param>
    /// <param name="size">Grid size</param>
    private void AdjustUVs(Mesh mesh, int row, int col, int size)
    {
        Vector2[] uv = new Vector2[4];

        float pieceWidth = 1.0f / size; // Proportional width of each piece
        float pieceHeight = pieceWidth / ((float)puzzleMaterial.mainTexture.width / puzzleMaterial.mainTexture.height); // Proportional height

        // Calculate UV coordinates for this piece
        float uMin = col * pieceWidth;
        float uMax = (col + 1) * pieceWidth;
        float vMin = 1.0f - ((row + 1) * pieceHeight);
        float vMax = 1.0f - (row * pieceHeight);

        // Assign UVs to each vertex of the quad
        uv[0] = new Vector2(uMin, vMin);
        uv[1] = new Vector2(uMax, vMin);
        uv[2] = new Vector2(uMin, vMax);
        uv[3] = new Vector2(uMax, vMax);

        mesh.uv = uv;
    }
}
