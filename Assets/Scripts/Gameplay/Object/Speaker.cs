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

    public void Trigger()
    {
        Debug.Log("speaker trigger");
        PlaySound();
    }

    private void PlaySound()
    {
        GameObject wave = Instantiate(AssetHelper.instance.WavePrefab, transform.parent) as GameObject;
        wave.transform.parent = transform.parent;
        wave.transform.position = transform.position;
        var voiceWave = wave.GetComponent<VoiceWave>();
        voiceWave.SoundType = SoundType;
        voiceWave.Arc.Center = transform.position;
        voiceWave.Arc.Angle.StartAngle = 0;
        voiceWave.Arc.Angle.AngleRange = 2 * Math.PI;
    }

    public SoundTypes SoundType = SoundTypes.Laugh;
}
