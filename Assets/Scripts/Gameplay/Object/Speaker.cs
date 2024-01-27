using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Gameplay.Model;
using UnityEngine;

public class Speaker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Trigger(SoundTypes type)
    {
        Debug.Log("speaker trigger");
        PlaySound(type);
    }

    private void PlaySound(SoundTypes soundType)
    {
        GameObject wave = Instantiate(AssetHelper.instance.WavePrefab, transform.parent) as GameObject;
        wave.transform.parent = transform.parent;
        wave.transform.position = transform.position;
        var voiceWave = wave.GetComponent<VoiceWave>();
        voiceWave.SoundType = soundType;
        voiceWave.Arc.Center = transform.position;
        voiceWave.Arc.Angle.StartAngle = 0;
        voiceWave.Arc.Angle.AngleRange = 2 * Math.PI;
    }
}
