using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //void OnTriggerEnter2D(Collider2D other)
    //{
    //    // todo test other and modify MoodValue
    //    if (other.gameObject.GetComponent<Player>()!=null)
    //    {
    //        Debug.Log("hit by player");
    //        MoodValue -= 1;
    //    }
    //
    //    UpdateByMood();
    //}

    // for 
    void RaiseMood()
    {
        MoodValue -= 1;
        UpdateByMood();
    }

    private void UpdateByMood()
    {
        if (MoodValue > 0)
        {
            // todo switch to angry assets
        }
        else
        {
            // todo switch to laugh assets
        }
    }

    // mood value
    public int MoodValue = 0;

}
