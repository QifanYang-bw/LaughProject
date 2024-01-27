using System;
using System.Collections.Generic;
using UnityEngine;

public enum GlobalDifficultyType {
    Easy = 0,
    Normal = 1,
    Hard = 2,
}
[Serializable]
public class GlobalConfigModel {
    public float MusicVolume = 0.7f;
    public float SfxVolume = 0.7f;
    public GlobalDifficultyType Difficulty = GlobalDifficultyType.Easy;
}