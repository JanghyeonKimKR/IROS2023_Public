using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyConB : MonoBehaviour
{
    public float settings = 1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Rotate(new Vector3(settings * Time.deltaTime, 0f, 0f));
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Rotate(new Vector3(-settings * Time.deltaTime, 0f, 0f));
        }
    }
}
