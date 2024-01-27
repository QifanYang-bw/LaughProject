using Assets.Scripts.Gameplay.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Microphone : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnVoiceWaveHit(VoiceWave voiceWave)
    {
        Debug.Log("microphone OnVoiceWaveHit");
        OnTriggerd(voiceWave.SoundType);
    }


    private void OnTriggerd(SoundTypes soundType)
    {
        foreach (var speaker in RelatedSpeakerList)
        {
            speaker.GetComponent<Speaker>().Trigger(soundType);
        }
    }

    // related speakers
    public List<GameObject> RelatedSpeakerList = new List<GameObject>();
    // sound type
    public SoundTypes SoundType;
}
