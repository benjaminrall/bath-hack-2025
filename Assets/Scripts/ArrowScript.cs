using System;
using UnityEngine;

public class ArrowScript : MonoBehaviour
{
    public float speed = 250f;
    public float maxHeight = 340;

    public bool[] hits;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        hits = new[] { false, false };
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }
}
