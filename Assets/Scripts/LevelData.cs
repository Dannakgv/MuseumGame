using UnityEngine;
using System;

[System.Serializable]
public class LevelData
{
    public int size; // Puzzle size (e.g., 3x3, 4x4)
    public string material; // Material name (used for the puzzle texture)
    public string icon; // NEW: Icon name for UI Button

    public LevelData(int size, string material, string icon)
    {
        this.size = size;
        this.material = material;
        this.icon = icon;
    }
}


