using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;
    [SerializeField] private Transform gameTransform;
    [SerializeField] private Transform piecePrefab;
    private GameObject winScreen;
    private List<Transform> pieces;
    private int emptyLocation;
    private int size;
    private bool shuffling = false;
    private bool gameStarted = false;  // Track if the game has officially started



    // Create the game setup with size x size pieces.
    private void CreateGamePieces(float gapThickness, Material puzzleMaterial)
    {
        float width = 1 / (float)size; // Proporción de cada pieza en relación con el rompecabezas completo
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameTransform);
                pieces.Add(piece);

                piece.localPosition = new Vector3(-1 + (2 * width * col) + width,
                                                  +1 - (2 * width * row) - width,
                                                  0);
                piece.localScale = ((2 * width) - gapThickness) * Vector3.one;
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

                    if (renderer != null && meshFilter != null)
                    {
                        renderer.material = puzzleMaterial; // Asignar material
                        AdjustUVs(meshFilter.mesh, row, col, size); // Ajustar las coordenadas UV
                    }
                }
            }
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        winScreen = GameObject.Find("WinScreen");
        if (winScreen != null) winScreen.SetActive(false);

        pieces = new List<Transform>();

        // Obtener el nivel actual desde LevelManager
        LevelData level = LevelManager.Instance.GetCurrentLevel();
        if (level == null) return;

        size = level.size;
        Material puzzleMaterial = Resources.Load<Material>($"Materials/{level.material}");

        if (puzzleMaterial == null)
        {
            Debug.LogError($"❌ Material '{level.material}' not found in Resources.");
        }

        CreateGamePieces(0.01f, puzzleMaterial);
        gameStarted = false;
        StartCoroutine(WaitShuffle(0.5f));
        shuffling = true;
    }



    // Update is called once per frame
    void Update()
    {
        // Stop everything if puzzle is solved
        if (shuffling) return;

        // On touch, check for valid moves
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
                        if (SwapIfValid(i, -size, size)) { break; }
                        if (SwapIfValid(i, +size, size)) { break; }
                        if (SwapIfValid(i, -1, 0)) { break; }
                        if (SwapIfValid(i, +1, size - 1)) { break; }
                    }
                }
            }
        }
    }

    private void AdjustUVs(Mesh mesh, int row, int col, int size)
    {
        Vector2[] uv = new Vector2[4];

        float pieceSize = 1.0f / size; // Tamaño relativo de cada pieza en UVs

        float uMin = col * pieceSize;
        float uMax = (col + 1) * pieceSize;
        float vMin = 1.0f - ((row + 1) * pieceSize);
        float vMax = 1.0f - (row * pieceSize);

        uv[0] = new Vector2(uMin, vMin);
        uv[1] = new Vector2(uMax, vMin);
        uv[2] = new Vector2(uMin, vMax);
        uv[3] = new Vector2(uMax, vMax);

        mesh.uv = uv;
    }


    // colCheck is used to stop horizontal moves wrapping.
    private bool SwapIfValid(int i, int offset, int colCheck)
    {
        if (((i % size) != colCheck) && ((i + offset) == emptyLocation))
        {
            // Swap them in game state.
            (pieces[i], pieces[i + offset]) = (pieces[i + offset], pieces[i]);

            // Swap their transforms.
            (pieces[i].localPosition, pieces[i + offset].localPosition) =
                (pieces[i + offset].localPosition, pieces[i].localPosition);

            // Update empty location.
            emptyLocation = i;

            // **Check if puzzle is solved after move**
            if (CheckCompletion())
            {
                return true;
            }

            return true;
        }
        return false;
    }

    private bool CheckCompletion()
    {
        if (!gameStarted) return false;  // Ignore completion before shuffling

        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].name != $"{i}")
            {
                return false;
            }
        }

        Debug.Log("✅ Puzzle Completed! Showing WinScreen...");

        // Stop all interactions
        shuffling = true;

        // Show win screen
        ShowWinScreen();
        return true;
    }

    // Function to activate the win screen
    private void ShowWinScreen()
    {
        if (winScreen != null)
        {
            winScreen.SetActive(true);
            Debug.Log("✅ WinScreen is now active!");
        }
        else
        {
            Debug.LogError("❌ WinScreen was not found at the start! Check Hierarchy.");
        }
    }

    private IEnumerator WaitShuffle(float duration)
    {
        yield return new WaitForSeconds(duration);
        Shuffle();
        gameStarted = true;
        shuffling = false;
    }

    private void Shuffle()
    {
        int count = 0;
        int last = -1; // Prevent undoing the last move

        while (count < (size * size * size))
        { // More swaps = better shuffle
            int rnd = Random.Range(0, size * size);

            if (rnd == last) { continue; } // Prevent immediate backtracking
            last = emptyLocation;

            // Try swapping in valid directions
            if (SwapIfValid(rnd, -size, size) ||
                SwapIfValid(rnd, +size, size) ||
                SwapIfValid(rnd, -1, 0) ||
                SwapIfValid(rnd, +1, size - 1))
            {
                CheckCompletion();
                count++;
            }
        }
    }

    public void LoadLevel(LevelData level)
    {
        if (level == null) return;

        if (winScreen != null) winScreen.SetActive(false);  // 🔹 Hide WinScreen

        size = level.size;
        Material puzzleMaterial = Resources.Load<Material>($"Materials/{level.material}");

        if (puzzleMaterial == null)
        {
            Debug.LogError($"❌ Material '{level.material}' not found in Resources.");
            return;
        }

        // Borra las piezas anteriores
        foreach (Transform piece in pieces)
        {
            Destroy(piece.gameObject);
        }
        pieces.Clear();

        // Crea el nuevo nivel
        CreateGamePieces(0.01f, puzzleMaterial);
        StartCoroutine(WaitShuffle(0.5f));
    }

}