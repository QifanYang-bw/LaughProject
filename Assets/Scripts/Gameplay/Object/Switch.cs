using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
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
        Debug.Log("Switch OnVoiceWaveHit");
        OnTrigger();
    }


    private void OnTrigger()
    {
        foreach (var speaker in WallList)
        {
            speaker.GetComponent<Wall>().TriggerRotate();
        }
    }

    // related speakers
    public List<GameObject> WallList = new List<GameObject>();
}
