using UnityEngine;

[System.Serializable]
public struct TeamData {
    public int Id;
    public Color color;

    public TeamData(int ID, Color color) {
        Id = ID;
        this.color = color;
    }
}
