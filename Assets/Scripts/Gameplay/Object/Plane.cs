using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane : MonoBehaviour
{
    public void UpdateImage(string name, Material mat) {
        GetComponent<MeshRenderer>().material = mat;
    }
}
