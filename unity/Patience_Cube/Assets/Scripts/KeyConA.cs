using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyConA : MonoBehaviour
{
    public float settings = 5;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(new Vector3(0f, 0f, settings*Time.deltaTime));
        }
        else if(Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(new Vector3(0f, 0f, -settings * Time.deltaTime));
        }

    }
    
}
