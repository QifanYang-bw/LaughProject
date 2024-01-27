using System;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;

[Serializable]
public class LevelScoreModel {
    public float passDuration;
    public GlobalDifficultyType difficulty;

    public LevelScoreModel(float passDuration) {
        this.passDuration = passDuration;
        difficulty = GameManager.Instance.ConfigModel.Difficulty;
    }
    public static LevelScoreModel EmptyScore()
    {
        return new LevelScoreModel(
            0
        );
    }

    public float RemainingTimeScore() {
        return 5000 / passDuration;
    }

    public int TotalScore() {
        float score = RemainingTimeScore();
        return (int)Mathf.Round(score);
    }
}