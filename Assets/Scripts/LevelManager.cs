using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using TMPro;

[System.Serializable]
public class LevelList
{
    public LevelData[] levels;
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public List<LevelData> levels = new List<LevelData>();
    public int currentLevelIndex = 0;
    private HashSet<int> unlockedLevels = new HashSet<int>();

    [SerializeField] private Transform levelGrid;
    [SerializeField] private GameObject levelButtonPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
            LoadLevelsFromJSON();
            SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to the sceneLoaded event
        }
        else
        {
            Debug.LogWarning("⚠️ Duplicate LevelManager detected. Destroying new instance.");
            Destroy(gameObject);
            return;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LevelManager")
        {
            ReassignLevelGrid();
        }
    }

    void Start()
    {
        CreateLevelButtons();
    }

    // ✅ Loads levels from JSON
    void LoadLevelsFromJSON()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "levels.json");

        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            LevelList levelList = JsonUtility.FromJson<LevelList>(jsonData);

            if (levelList != null && levelList.levels.Length > 0)
            {
                levels = new List<LevelData>(levelList.levels);
                Debug.Log("✅ Levels loaded from JSON.");
            }
            else
            {
                Debug.LogError("❌ JSON file is empty or invalid.");
            }
        }
        else
        {
            Debug.LogError($"❌ JSON file not found at: {path}");
        }
    }


    // ✅ Generates buttons for all levels
    
    public void CreateLevelButtons()
    {
        if (levelGrid == null || levelButtonPrefab == null)
        {
            Debug.LogError("❌ Level Grid or Button Prefab is missing!");
            return;
        }

        foreach (Transform child in levelGrid)
        {
            Destroy(child.gameObject);
        }

        Debug.Log($"🟢 Creating {levels.Count} level buttons...");

        for (int i = 0; i < levels.Count; i++)
        {
            GameObject buttonObj = Instantiate(levelButtonPrefab, levelGrid);
            Button button = buttonObj.GetComponent<Button>();
            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
            Image buttonImage = buttonObj.GetComponentInChildren<Image>();

            if (button == null || buttonText == null || buttonImage == null)
            {
                Debug.LogError($"❌ Missing components on level button {i}");
                continue;
            }

            buttonText.text = $"Level {i + 1}";
            button.interactable = unlockedLevels.Contains(i) || i == 0;

            Sprite iconSprite = Resources.Load<Sprite>($"Icons/{levels[i].icon}");
            if (iconSprite != null)
            {
                buttonImage.sprite = iconSprite;
            }
            else
            {
                Debug.LogWarning($"⚠️ Icon '{levels[i].icon}' not found in Resources/Icons/");
            }

            int levelIndex = i;
            button.onClick.AddListener(() => SelectLevel(levelIndex));
        }
    } 
/*
    public void CreateLevelButtons()
    {
        if (levelGrid == null || levelButtonPrefab == null)
        {
            Debug.LogError("❌ Level Grid or Button Prefab is missing!");
            return;
        }

        // Limpiar botones existentes
        foreach (Transform child in levelGrid)
        {
            Destroy(child.gameObject);
        }

        Debug.Log($"🟢 Creating {levels.Count} level buttons...");

        // Definir el espaciado entre botones
        float spacing = 10f; // Espacio entre botones (ajusta según sea necesario)
        float offsetX = 0f;  // Desplazamiento horizontal inicial

        for (int i = 0; i < levels.Count; i++)
        {
            // Instanciar el botón
            GameObject buttonObj = Instantiate(levelButtonPrefab, levelGrid);
            Button button = buttonObj.GetComponent<Button>();
            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
            Image buttonImage = buttonObj.GetComponentInChildren<Image>();

            if (button == null || buttonText == null || buttonImage == null)
            {
                Debug.LogError($"❌ Missing components on level button {i}");
                continue;
            }

            // Asignar el texto del botón
            buttonText.text = $"Level {i + 1}";
            button.interactable = unlockedLevels.Contains(i) || i == 0;

            // Cargar la imagen del nivel
            Sprite iconSprite = Resources.Load<Sprite>($"Icons/{levels[i].icon}");
            if (iconSprite != null)
            {
                buttonImage.sprite = iconSprite;

                // Ajustar el tamaño del botón al tamaño de la imagen
                RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    // Obtener el tamaño de la imagen
                    float imageWidth = iconSprite.rect.width;
                    float imageHeight = iconSprite.rect.height;

                    // Escalar el tamaño del botón (ajusta el factor de escala según sea necesario)
                    float scaleFactor = 0.5f; // Escala al 50% del tamaño original
                    buttonRect.sizeDelta = new Vector2(imageWidth * scaleFactor, imageHeight * scaleFactor);

                    // Ajustar la posición del botón
                    buttonRect.anchoredPosition = new Vector2(offsetX, 0f);

                    // Actualizar el desplazamiento horizontal para el siguiente botón
                    offsetX += (imageWidth * scaleFactor) + spacing;
                }
                else
                {
                    Debug.LogError("❌ RectTransform not found on button!");
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Icon '{levels[i].icon}' not found in Resources/Icons/");
            }

            // Asignar el evento de clic al botón
            int levelIndex = i;
            button.onClick.AddListener(() => SelectLevel(levelIndex));
        }

        // Ajustar el tamaño del levelGrid para que el ScrollRect funcione
        RectTransform levelGridRect = levelGrid.GetComponent<RectTransform>();
        levelGridRect.sizeDelta = new Vector2(offsetX, levelGridRect.sizeDelta.y);
    }
    */

    // ✅ Loads the selected level
    public void SelectLevel(int index)
    {
        currentLevelIndex = index;
        SceneManager.LoadScene("PuzzleScene");
    }

    // ✅ Unlocks the next level after completion
    public void UnlockNextLevel()
    {
        if (!unlockedLevels.Contains(currentLevelIndex + 1) && currentLevelIndex + 1 < levels.Count)
        {
            unlockedLevels.Add(currentLevelIndex + 1);
            SaveProgress();
        }
    }

    // ✅ Returns the current level
    public LevelData GetCurrentLevel()
    {
        if (currentLevelIndex < levels.Count)
        {
            return levels[currentLevelIndex];
        }
        Debug.LogError("❌ No more levels available!");
        return null;
    }

    // ✅ Save progress
    private void SaveProgress()
    {
        PlayerPrefs.SetString("UnlockedLevels", string.Join(",", unlockedLevels));
        PlayerPrefs.Save();
    }

    // ✅ Load progress from PlayerPrefs
    private void LoadProgress()
    {
        string savedData = PlayerPrefs.GetString("UnlockedLevels", "0");
        unlockedLevels = new HashSet<int>(System.Array.ConvertAll(savedData.Split(','), int.Parse));
    }

    // ✅ Load the next level
    public void LoadNextLevel()
    {
        if (currentLevelIndex + 1 < levels.Count)
        {
            currentLevelIndex++;
            SceneManager.LoadScene("PuzzleScene");
        }
        else
        {
            Debug.Log("🎉 No more levels! Returning to menu...");
            SceneManager.LoadScene("MainMenu");
        }
    }

    // ✅ Goes back to the main menu
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ReassignLevelGrid()
    {
        Debug.Log("🔄 Reassigning LevelGrid...");
        GameObject gridObject = GameObject.Find("LevelGrid");

        if (gridObject != null)
        {
            levelGrid = gridObject.transform;
            Debug.Log("✅ LevelGrid found and reassigned.");
            CreateLevelButtons(); // Reload buttons if needed
        }
        else
        {
            Debug.LogError("❌ LevelGrid not found after returning to LevelManagerScene!");
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe from the sceneLoaded event
    }
}