using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawRotation : MonoBehaviour
{
    public float rotationSpeed = 200f; // Speed of rotation, adjustable in the Inspector

    // Update is called once per frame
    void Update()
    {
        // Rotate the saw around the Z-axis at a constant speed
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}