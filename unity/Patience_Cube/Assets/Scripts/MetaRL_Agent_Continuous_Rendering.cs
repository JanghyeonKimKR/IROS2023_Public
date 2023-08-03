using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MetaRL_Agent_Continuous_Rendering : MonoBehaviour
{
    // ############ Parameters ############
    // Observation
    public Transform acturatorA; // EndPoint : X rotation
    public Transform acturatorB; // Manipulator : Z rotation

    // ball
    public Rigidbody ballRigidbody;

    // file
    public string fileName;
    //####################################
    // cube info
    List<float> theta_x = new List<float>();
    List<float> theta_z = new List<float>();

    // ball info
    List<float> x = new List<float>();
    List<float> y = new List<float>();
    float theta_X, theta_Z;
    float Xtmp, Ytmp, Ztmp;
    float X, Y, Z;

    public static float[] x_lim = { -2.946f, 2.946f };
    public static float[] y_lim = { -3.602f, 3.602f };
    public static float[] z_lim = { 0.4375001f, 0.4375001f };
    float L = 30f;

    // time info
    int index = 0;

    void Start()
    {
        Time.timeScale = 8;
        index = 0;
        string filePath = Application.dataPath + "/Scripts/render/" + fileName;
        ballRigidbody.isKinematic = true;
        if (File.Exists(filePath))
        {
            StreamReader reader = new StreamReader(filePath);

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(',');
                if (values.Length == 13)
                {
                    x.Add(float.Parse(values[1]));
                    y.Add(float.Parse(values[2]));
                    theta_x.Add(float.Parse(values[7]));
                    theta_z.Add(float.Parse(values[8]));
                }
                // Do something with the values, e.g. print to console
                //Debug.Log(string.Join(",", values));
            }

            reader.Close();
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }
        index = 0;
    }

    void FixedUpdate()
    {
        theta_X = theta_x[index] * Mathf.Deg2Rad;
        theta_Z = theta_z[index] * Mathf.Deg2Rad;

        // Chage of Basis(To Global Frame)
        X = -x[index];
        Z = -y[index];
        Y = z_lim[1];
        // Chage of Basis(acturatorA)
        Xtmp = X;
        Ztmp = Mathf.Cos(theta_X) * (L - Z) - Mathf.Sin(theta_X) * Y;
        Ytmp = Mathf.Sin(theta_X) * (L - Z) + Mathf.Cos(theta_X) * Y;
        // Chage of Basis(ActuratorB)
        X = Mathf.Cos(theta_Z) * Xtmp - Mathf.Sin(theta_Z) * Ytmp;
        Z = L - Ztmp;
        Y = Mathf.Sin(theta_Z) * Xtmp + Mathf.Cos(theta_Z) * Ytmp;

        // Update
        ballRigidbody.position = new Vector3(X, Y, Z);
        acturatorA.transform.eulerAngles = new Vector3(theta_X * Mathf.Rad2Deg, 0f, 0f);
        acturatorB.transform.eulerAngles = new Vector3(0f, 0f, theta_Z * Mathf.Rad2Deg);
        index++;
    }
}
