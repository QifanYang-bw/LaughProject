using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UserDataModel {
    public Dictionary<string, LevelScoreModel> levelScoreDict;

    public UserDataModel() {
        levelScoreDict = new Dictionary<string, LevelScoreModel>();
    }

    public int TotalScore() {
        int result = 0;
        foreach (KeyValuePair<string, LevelScoreModel> entry in levelScoreDict) {
            result += entry.Value.TotalScore();
        }
        return result;
    }
}