using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane : Singleton<Plane>
{
    public void UpdateImage(string name, Material mat) {
        GetComponent<MeshRenderer>().material = mat;
    }
}
