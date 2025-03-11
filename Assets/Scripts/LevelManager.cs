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

    [SerializeField] private Transform levelGrid; // Reference to UI Grid (Assign in Inspector)
    [SerializeField] private GameObject levelButtonPrefab; // Prefab for level buttons

private void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadLevelsFromJSON();

        // 🛠 Fix: Ensure the Level Button Prefab is reassigned
        if (levelButtonPrefab == null)
        {
            levelButtonPrefab = Resources.Load<GameObject>("Prefabs/LevelButton");
            if (levelButtonPrefab == null)
            {
                Debug.LogError("❌ Level Button Prefab not found in Resources!");
            }
        }
    }
    else
    {
        Destroy(gameObject);
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
    if (levelGrid == null)
    {
        Debug.LogError("❌ Level Grid is NOT assigned in the Inspector!");
        return;
    }

    if (levelButtonPrefab == null)
    {
        Debug.LogError("❌ Level Button Prefab is NOT assigned in the Inspector!");
        return;
    }

    if (levels == null || levels.Count == 0)
    {
        Debug.LogError("❌ Levels list is empty! Make sure the JSON is loading correctly.");
        return;
    }

    // Clear previous buttons (if any)
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
        Image buttonImage = buttonObj.GetComponentInChildren<Image>(); // Get Image component

        if (button == null || buttonText == null || buttonImage == null)
        {
            Debug.LogError($"❌ Button, Text, or Image component missing on level button {i}");
            continue;
        }

        int levelIndex = i;
        buttonText.text = $"Level {levelIndex + 1}";

        // ✅ Assign icon if found
        Sprite iconSprite = Resources.Load<Sprite>($"Icons/{levels[i].icon}");
        if (iconSprite != null)
        {
            buttonImage.sprite = iconSprite;
        }
        else
        {
            Debug.LogWarning($"⚠️ Icon '{levels[i].icon}' not found in Resources/Icons/");
        }

        // Disable button if level is locked
        button.interactable = (levelIndex <= currentLevelIndex);

        // Assign function to button
        button.onClick.AddListener(() => SelectLevel(levelIndex));
    }
}




    // ✅ Loads the selected level
    public void SelectLevel(int index)
    {
        currentLevelIndex = index;
        SceneManager.LoadScene("PuzzleScene"); // Ensure scene name matches the puzzle scene
    }

    // ✅ Goes back to the main menu
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // ✅ Moves to the next level
    public void NextLevel()
    {
        if (currentLevelIndex < levels.Count - 1)
        {
            currentLevelIndex++;
            GameManager.Instance.LoadLevel(GetCurrentLevel());
        }
        else
        {
            Debug.Log("🎉 No more levels! Maybe return to the main menu?");
        }
    }

    // ✅ Returns the current level data
    public LevelData GetCurrentLevel()
    {
        if (currentLevelIndex < levels.Count)
        {
            return levels[currentLevelIndex];
        }
        Debug.LogError("❌ No more levels available!");
        return null;
    }
}
