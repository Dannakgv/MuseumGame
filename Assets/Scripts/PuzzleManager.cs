using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private Transform gameTransform;
    [SerializeField] private Transform piecePrefab;
    [SerializeField] private GameObject winScreen;

    private List<Transform> pieces;
    private int emptyLocation;
    private int size;
    private bool shuffling = false;
    private bool gameStarted = false;
    private Material puzzleMaterial; 

    void Start()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError("❌ LevelManager instance not found!");
            return;
        }

        // ✅ Load Current Level
        LevelData level = LevelManager.Instance.GetCurrentLevel();
        if (level == null) return;

        size = level.size;
        Material puzzleMaterial = Resources.Load<Material>($"Materials/{level.material}");

        if (puzzleMaterial == null)
        {
            Debug.LogError($"❌ Material '{level.material}' not found!");
            return;
        }

        // ✅ Initialize Board
        CreateGamePieces(0.01f, puzzleMaterial);
        winScreen.SetActive(false);
        gameStarted = false;

        // ✅ Shuffle Board
        StartCoroutine(WaitShuffle(0.5f));
        shuffling = true;
    }

    void Update()
{
    if (shuffling) return; // 🛑 Prevent moves while shuffling

    // ✅ Detect click/tap
    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
    {
        Vector2 touchPosition = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
        RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero);

        if (hit)
        {

            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] == hit.transform)
                {
                    Debug.Log($"🔄 Trying to move piece {i}");

                    if (SwapIfValid(i, -size, size)) { Debug.Log("⬆️ Moved Up"); break; }
                    if (SwapIfValid(i, +size, size)) { Debug.Log("⬇️ Moved Down"); break; }
                    if (SwapIfValid(i, -1, 0)) { Debug.Log("⬅️ Moved Left"); break; }
                    if (SwapIfValid(i, +1, size - 1)) { Debug.Log("➡️ Moved Right"); break; }
                }
            }
        }
        else
        {
            Debug.Log("❌ No piece was clicked!");
        }
    }
}


private void CreateGamePieces(float gapThickness, Material material)
{
    if (piecePrefab == null)
    {
        Debug.LogError("❌ piecePrefab is not assigned!");
        return;
    }

    if (material == null)
    {
        Debug.LogError("❌ puzzleMaterial is not assigned!");
        return;
    }

    puzzleMaterial = material; // Assign the material to the class-level variable
    pieces = new List<Transform>();

    // Obtener la relación de aspecto de la textura del material
    Texture2D texture = (Texture2D)puzzleMaterial.mainTexture;
    if (texture == null)
    {
        Debug.LogError("❌ Texture not found in puzzleMaterial!");
        return;
    }

    float aspectRatio = (float)texture.width / texture.height;

    // Calcular el ancho y alto de cada pieza
    float pieceWidth = 1 / (float)size;
    float pieceHeight = pieceWidth / aspectRatio; // Ajustar la altura según la relación de aspecto

    Debug.Log($"🟢 Creating {size * size} pieces with width={pieceWidth}, height={pieceHeight}");

    for (int row = 0; row < size; row++)
    {
        for (int col = 0; col < size; col++)
        {
            Transform piece = Instantiate(piecePrefab, gameTransform);
            if (piece == null)
            {
                Debug.LogError("❌ Failed to instantiate piecePrefab!");
                continue;
            }

            pieces.Add(piece);

            // Ajustar la posición de las piezas según la relación de aspecto
            piece.localPosition = new Vector3(
                -1 + (2 * pieceWidth * col) + pieceWidth,
                +1 - (2 * pieceHeight * row) - pieceHeight,
                0
            );

            // Ajustar la escala de las piezas
            piece.localScale = new Vector3(
                (2 * pieceWidth) - gapThickness,
                (2 * pieceHeight) - gapThickness,
                1
            );

            piece.name = $"{(row * size) + col}";

            if ((row == size - 1) && (col == size - 1))
            {
                emptyLocation = (size * size) - 1;
                piece.gameObject.SetActive(false);
            }
            else
            {
                MeshRenderer renderer = piece.GetComponent<MeshRenderer>();
                MeshFilter meshFilter = piece.GetComponent<MeshFilter>();

                if (renderer != null)
                {
                    renderer.material = puzzleMaterial;
                    AdjustUVs(meshFilter.mesh, row, col, size);
                }
                else
                {
                    Debug.LogError("❌ MeshRenderer not found on piece!");
                }
            }
        }
    }
}

private IEnumerator WaitShuffle(float duration)
{
    yield return new WaitForSeconds(duration);
    Shuffle();
    gameStarted = true; // Ensure this is set to true after shuffling
    shuffling = false;
}

    private void Shuffle()
    {
        int count = 0;
        int last = -1;

        while (count < (size * size * size))
        {
            int rnd = Random.Range(0, size * size);
            if (rnd == last) continue;
            last = emptyLocation;

            if (SwapIfValid(rnd, -size, size) ||
                SwapIfValid(rnd, +size, size) ||
                SwapIfValid(rnd, -1, 0) ||
                SwapIfValid(rnd, +1, size - 1))
            {
                count++;
            }
        }
    }

private bool SwapIfValid(int i, int offset, int colCheck)
{
    if (((i % size) != colCheck) && ((i + offset) == emptyLocation))
    {
        Debug.Log($"✅ Swapping piece {i} with empty space {emptyLocation}");

        (pieces[i], pieces[i + offset]) = (pieces[i + offset], pieces[i]);
        (pieces[i].localPosition, pieces[i + offset].localPosition) =
            (pieces[i + offset].localPosition, pieces[i].localPosition);

        emptyLocation = i;

        // ✅ Check if the puzzle is solved (only after shuffling is complete)
        if (gameStarted && CheckCompletion())
        {
            Debug.Log("🏆 Puzzle solved! Win screen should appear!");
        }

        return true;
    }

    Debug.Log($"⚠️ Invalid move for piece {i}");
    return false;
}


private bool CheckCompletion()
{
    if (!gameStarted) return false;  // Ignore before shuffling

    Debug.Log("🔍 Checking if the puzzle is complete...");

    for (int i = 0; i < pieces.Count; i++)
    {
        if (pieces[i].name != $"{i}")
        {
            Debug.Log($"❌ Piece {pieces[i].name} is not in the correct position.");
            return false;
        }
    }

    Debug.Log("🏆 Puzzle Completed! Showing WinScreen...");
    shuffling = true;
    CompleteLevel();
    return true;
}


    public void CompleteLevel()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnlockNextLevel();
        }

        winScreen.SetActive(true);
    }

    public void NextLevel()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadNextLevel();
        }
    }

    public void BackToLevelManager()
    {
        SceneManager.LoadScene("LevelManager");
        StartCoroutine(WaitForLevelManager());
    }

    private IEnumerator WaitForLevelManager()
    {
        yield return new WaitForSeconds(0.2f);

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ReassignLevelGrid();
        }
        else
        {
            Debug.LogError("❌ LevelManager instance missing after scene reload!");
        }
    }

private void AdjustUVs(Mesh mesh, int row, int col, int size)
{
    Vector2[] uv = new Vector2[4];

    float pieceWidth = 1.0f / size; // Ancho proporcional de cada pieza
    float pieceHeight = pieceWidth / ((float)puzzleMaterial.mainTexture.width / puzzleMaterial.mainTexture.height); // Alto proporcional

    float uMin = col * pieceWidth;
    float uMax = (col + 1) * pieceWidth;
    float vMin = 1.0f - ((row + 1) * pieceHeight);
    float vMax = 1.0f - (row * pieceHeight);

    uv[0] = new Vector2(uMin, vMin);
    uv[1] = new Vector2(uMax, vMin);
    uv[2] = new Vector2(uMin, vMax);
    uv[3] = new Vector2(uMax, vMax);

    mesh.uv = uv;
}

}
