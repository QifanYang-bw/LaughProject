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

    private void OnTriggerd()
    {
        foreach (var speaker in RelatedSpeakerList)
        {
            //todo lwttai
        }
    }

    // related speakers
    public List<GameObject> RelatedSpeakerList = new List<GameObject>();
    // sound type
    public SoundTypes SoundType;
}
