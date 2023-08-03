using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Output_Information : MonoBehaviour
{
    string filePath;

    FileStream fs = null;
    StreamWriter writer = null;

    public Transform ball;

    public bool NewTrial = true;


    void Start()
    {
    }


    void Update()
    {

        if (NewTrial)
        {
            filePath = "./Ball_Pos/" + DateTime.Now.ToString("MMdd_HHmm_ss") + "_Flat.txt"; //Flat or Curved
            fs = new FileStream(filePath, FileMode.Create);
            writer = new StreamWriter(fs);
        }
        NewTrial = false;
        
        writer.Write(ball.position.x + " " + ball.position.y + " " + ball.position.z + ";\n");

    }
}
