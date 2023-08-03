using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MetaRL_Agent_Continuous_Save_data : Agent
{
    // ############ Parameters ############
    // ML-Agent
    public float max_action = 0.5f;

    // weights
    public float c1, c2, c3, c4, c5;

    // Observation
    public Transform acturatorA; // EndPoint : X rotation
    public Transform acturatorB; // Manipulator : Z rotation

    // ball
    public Rigidbody ballRigidbody;

    // name
    public string type; // approach, balance, cheep, expensive
    //####################################

    // cube info.
    float thetaX, thetaZ;
    float dthetaX = 0f, dthetaZ = 0f;
    float ddthetaX = 0f, ddthetaZ = 0f;

    // ball info.
    List<Vector3> fixed_init_point = new List<Vector3>();
    int num_point = 5;
    float dist = 0f;

    // Reward
    float relativePos, relativeVel, relativeTheta, relativeDtheta, relativeAction;
    float rewardPos, rewardVel, rewardTheta, rewardDtheta, rewardAction;

    // Time
    float runningTime = 0f;
    float completeTime = 0f;

    // File
    StreamWriter streamWriter;
    int index = 0;

    private void Create_DumyEnv()
    {
        //##################Initialization ##################
        //float x = Random.Range(Ball_dynamics.x_lim[0] - 0.2f, Ball_dynamics.x_lim[1] - 0.2f);
        //float y = Random.Range(Ball_dynamics.y_lim[0] - 0.2f, Ball_dynamics.y_lim[1] - 0.2f);
        //float z = Random.Range(Ball_dynamics.z_lim[0], Ball_dynamics.z_lim[1]);

        //ballRigidbody.transform.position = new Vector3(x, z, y);        
        
        ballRigidbody.transform.position = fixed_init_point[index];
        Ball_dynamics.dx = 0f;
        Ball_dynamics.dy = 0f;
        
        acturatorA.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        acturatorB.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        runningTime = 0f;
        completeTime = 0f;
        dthetaX = 0f;
        dthetaZ = 0f;
        index++;
        streamWriter = new StreamWriter("./Assets/Data/" + type + index + ".csv");
    }

    public override void Initialize()
    {
        Time.timeScale = 3;
        float x_upper = Ball_dynamics.x_lim[1];
        float x_lower = Ball_dynamics.x_lim[0];
        float y_lim = Ball_dynamics.z_lim[0];
        float z_upper = Ball_dynamics.y_lim[1];
        float z_lower = Ball_dynamics.y_lim[0];


        // for four point data collection
        //float alpha = 0;
        //fixed_init_point.Add(new Vector3(x_upper - 0.4f, y_lim, z_upper - 0.4f));
        //fixed_init_point.Add(new Vector3(x_upper - 0.4f , y_lim, z_lower + 0.4f));
        //fixed_init_point.Add(new Vector3(x_lower + 0.4f, y_lim, z_upper - 0.4f));
        //fixed_init_point.Add(new Vector3(x_lower + 0.4f, y_lim, z_lower + 0.4f));

        //fixed_init_point.Add(new Vector3(2.5f, y_lim, 0f));
        //fixed_init_point.Add(new Vector3(-2.5f, y_lim, 0f));
        //fixed_init_point.Add(new Vector3(0f, y_lim, 2.5f));
        //fixed_init_point.Add(new Vector3(0f, y_lim, -2.5f));

        //fixed_init_point.Add(new Vector3(2.3f, y_lim, 0.98f));
        fixed_init_point.Add(new Vector3(2.3f, y_lim, -0.98f));
        fixed_init_point.Add(new Vector3(-2.3f, y_lim, 0.98f));
        //fixed_init_point.Add(new Vector3(-2.3f, y_lim, -0.98f));

        //fixed_init_point.Add(new Vector3(0.98f, y_lim, 2.3f));
        //fixed_init_point.Add(new Vector3(0.98f, y_lim, -2.3f));
        //fixed_init_point.Add(new Vector3(-0.98f, y_lim, 2.3f));
        //fixed_init_point.Add(new Vector3(-0.98f, y_lim, -2.3f));

        //fixed_init_point.Add(new Vector3(1.7678f, y_lim, 1.7678f));
        //fixed_init_point.Add(new Vector3(1.7678f, y_lim, -1.7678f));
        //fixed_init_point.Add(new Vector3(-1.7678f, y_lim, 1.7678f));
        //fixed_init_point.Add(new Vector3(-1.7678f, y_lim, -1.7678f));

        //fixed_init_point.Add(new Vector3(2.27f, y_lim, 1.047f));
        //fixed_init_point.Add(new Vector3(2.27f, y_lim, -1.047f));
        fixed_init_point.Add(new Vector3(-2.27f, y_lim, 1.047f));
        //fixed_init_point.Add(new Vector3(-2.27f, y_lim, -1.047f));

    }

    public override void OnEpisodeBegin()
    {
        Create_DumyEnv();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //print(Ball_dynamics.x + ", " + Ball_dynamics.y + ", " + Ball_dynamics.dx + ", " + Ball_dynamics.dy + ", " + Ball_dynamics.theta_x + ", " + Ball_dynamics.theta_y + ", " + dthetaX + ", " + dthetaZ);

        // -------------------------------------------------------
        sensor.AddObservation(Ball_dynamics.x);
        sensor.AddObservation(Ball_dynamics.y);
        // -------------------------------------------------------
        sensor.AddObservation(Ball_dynamics.dx);
        sensor.AddObservation(Ball_dynamics.dy);
        // -------------------------------------------------------
        thetaX = acturatorA.rotation.eulerAngles.x;
        thetaZ = acturatorA.rotation.eulerAngles.z;
        thetaX = thetaX < 180 ? thetaX : (thetaX - 360);
        thetaZ = thetaZ < 180 ? thetaZ : (thetaZ - 360);
        sensor.AddObservation(thetaX);
        sensor.AddObservation(thetaZ);
        // -------------------------------------------------------
        sensor.AddObservation(dthetaX);
        sensor.AddObservation(dthetaZ);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        # region Preprocessing
        runningTime = runningTime + Time.deltaTime;

        dist = Ball_dynamics.dist;

        # endregion Preprocessing
        var continuousActionsOut = actionBuffers.ContinuousActions;
        var actionX = Mathf.Clamp(continuousActionsOut[0], - max_action, max_action);
        var actionZ = Mathf.Clamp(continuousActionsOut[1], - max_action, max_action);
        //Debug.Log(actionX + ", "+ actionZ);

        // Action
        ddthetaX = actionX; // Degree
        ddthetaZ = actionZ; // Degree
        dthetaX = dthetaX + ddthetaX * Time.deltaTime;  // Degree
        dthetaZ = dthetaZ + ddthetaZ * Time.deltaTime;  // Degree

        //print("###########################################\n"
        //    + "action1 : " + continuousActionsOut[0] + " dthetaX // dthetaX : " + " // ddthetaX : " + ddthetaX + "\n"
        //    + "action2 : " + continuousActionsOut[1] + " // dthetaZ : " + dthetaZ + " // ddthetaZ : " + ddthetaZ);
        acturatorA.transform.Rotate(new Vector3(1f, 0f, 0f), dthetaX * Time.deltaTime);
        acturatorB.transform.Rotate(new Vector3(0f, 0f, 1f), dthetaZ * Time.deltaTime);

        thetaX = acturatorA.rotation.eulerAngles.x;
        thetaZ = acturatorA.rotation.eulerAngles.z;
        thetaX = thetaX < 180 ? thetaX : (thetaX - 360);
        thetaZ = thetaZ < 180 ? thetaZ : (thetaZ - 360);
        // 과꺾 방지 X축
        if (thetaX > 3f)
        {
            acturatorA.transform.Rotate(new Vector3(1f, 0f, 0f), -dthetaX * Time.deltaTime);
        }
        else if (thetaX < -3f)
        {
            acturatorA.transform.Rotate(new Vector3(1f, 0f, 0f), -dthetaX * Time.deltaTime);
        }

        // 과꺾 방지 Z축
        if (thetaZ > 3f)
        {
            acturatorB.transform.Rotate(new Vector3(0f, 0f, 1f), -dthetaZ * Time.deltaTime);
        }
        else if (thetaZ < -3f)
        {
            acturatorB.transform.Rotate(new Vector3(0f, 0f, 1f), -dthetaZ * Time.deltaTime);
        }

        // File Stream Save
        streamWriter.WriteLine(runningTime
                + "," + Ball_dynamics.x + "," + Ball_dynamics.y
                + "," + Ball_dynamics.dx + "," + Ball_dynamics.dy
                + "," + Ball_dynamics.ddx + "," + Ball_dynamics.ddy

                + "," + thetaX + "," + thetaZ
                + "," + dthetaX + "," + dthetaZ
                + "," + ddthetaX + "," + ddthetaZ
                );

        // for Terminate Condition1
        //print(runningTime);
        // 잘된거 200 맞아 의심하지마!!
        if (runningTime > 60f)
        {
            Debug.Log("Time out!");
            EndEpisode();
        }
        // for Terminate Condition2
        if (dist < 0.05f)
        {
            completeTime = completeTime + Time.deltaTime;
            
            if (completeTime > 1f)
            {
                streamWriter.Close();
                Debug.Log("Goal");
                EndEpisode();
            }
        }
        else
        {
            completeTime = 0f;
        }
    }
}
