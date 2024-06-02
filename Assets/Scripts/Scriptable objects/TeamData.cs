using UnityEngine;

[CreateAssetMenu(fileName = "NewTeam", menuName = "ScriptableObjects/Team data", order = 1)]
public class TeamData : ScriptableObject
{
    
    public string teamName;
    public string longTeamName;
    public char teamTag;
    public Color teamColor;
    public Material teamEquipMaterial;
}