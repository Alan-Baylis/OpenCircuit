using UnityEngine;

[System.Serializable]
public struct TeamData {
    public int Id;
    public Color color;
    public TagEnum teamTag;

    public TeamData(int ID, Color color, TagEnum teamTag) {
        Id = ID;
        this.color = color;
        this.teamTag = teamTag;
    }
}
