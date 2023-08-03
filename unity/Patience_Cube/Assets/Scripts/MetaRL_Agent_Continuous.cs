using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MetaRL_Agent_Continuous : Agent
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

    private void Create_DumyEnv()
    {
        //##################Initialization ##################
        //float x = Random.Range(Ball_dynamics.x_lim[0] - 0.2f, Ball_dynamics.x_lim[1] - 0.2f);
        //float y = Random.Range(Ball_dynamics.y_lim[0] - 0.2f, Ball_dynamics.y_lim[1] - 0.2f);
        //float z = Random.Range(Ball_dynamics.z_lim[0], Ball_dynamics.z_lim[1]);

        //ballRigidbody.transform.position = new Vector3(x, z, y);        
        int random_idx = UnityEngine.Random.Range(0, num_point*4);
        ballRigidbody.transform.position = fixed_init_point[random_idx];
        Ball_dynamics.dx = 0f;
        Ball_dynamics.dy = 0f;
        

        acturatorA.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        acturatorB.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        runningTime = 0f;
        completeTime = 0f;
        dthetaX = 0f;
        dthetaZ = 0f;
    }

    private void Reward()
    {
        // weights
        float c1 = -1.0f,     c2 = -0.0001f,      c3 = -1.2538f,      c4 = -0.0004f,      c5 = -0.0201f,  c6 = -0.0201f;  // proposed approach
        //float c1 = -1.0f,     c2 = -0.01f,        c3 = -1.0f,         c4 = -0.001f,       c5 = -1f,       c6 = -1f;  // balance control
        //float c1 = -1.0f,     c2 = -0.01f,        c3 = -1.0f,         c4 = -0.001f,       c5 = -0.01f,    c6 = -0.01f;  // cheep control
        //float c1 = -1.0f,     c2 = -0.01f,        c3 = -1.0f,         c4 = -0.001f,       c5 = -100f,     c6 = -100f;  // expensive control

        // pos
        relativePos = Mathf.Pow(Ball_dynamics.x, 2) + Mathf.Pow(Ball_dynamics.y, 2);
        rewardPos = c1 * relativePos;

        // velocity
        relativeVel = Mathf.Pow(Ball_dynamics.dx, 2) + Mathf.Pow(Ball_dynamics.dy, 2);
        rewardVel = c2 * relativeVel;

        // theta
        relativeTheta = Mathf.Pow(Ball_dynamics.theta_x * Mathf.Rad2Deg, 2) + Mathf.Pow(Ball_dynamics.theta_y * Mathf.Rad2Deg, 2);
        rewardTheta = c3 * relativeTheta;

        // dtheta
        relativeDtheta = Mathf.Pow(dthetaX, 2) + Mathf.Pow(dthetaZ, 2);
        rewardDtheta = c4 * relativeDtheta;

        // effort
        relativeAction = c5*Mathf.Pow(ddthetaX, 2) + c6*Mathf.Pow(ddthetaZ, 2);
        rewardAction = relativeAction;

        SetReward(rewardPos + rewardVel + rewardTheta + rewardDtheta + rewardAction);
        //print("Pos: " + rewardPos.ToString("N2") + " // Vel: " + rewardVel.ToString("N2") + " // Theta: " + 
        //    rewardTheta.ToString("N2") + " // Action: " + rewardAction.ToString("N2") + " // Total:" + (rewardPos + rewardVel + rewardTheta + rewardDtheta + rewardAction).ToString("N2"));
        // Debug.Log("Reward: " + (alpha * rewardPos + beta * rewardTilt + gamma * rewardAction));
    }

    public override void Initialize()
    {
        Time.timeScale = 10;
        float x_upper = Ball_dynamics.x_lim[1];
        float x_lower = Ball_dynamics.x_lim[0];
        float y_lim = Ball_dynamics.z_lim[0];
        float z_upper = Ball_dynamics.y_lim[1];
        float z_lower = Ball_dynamics.y_lim[0];
        for (int i = 0; i < num_point; i++)
        {
            float alpha = i / (num_point - 1f);
            fixed_init_point.Add(new Vector3(x_upper,                                   y_lim,      alpha * z_lower + (1 - alpha) * z_upper));
            fixed_init_point.Add(new Vector3(x_lower,                                   y_lim,      alpha * z_lower + (1 - alpha) * z_upper));
            fixed_init_point.Add(new Vector3(alpha * x_lower + (1 - alpha) * x_upper,   y_lim,      z_upper));
            fixed_init_point.Add(new Vector3(alpha * x_lower + (1 - alpha) * x_upper,   y_lim,      z_lower));
        }
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


        // for Terminate Condition1
        Reward();
        //print(runningTime);
        // 잘된거 200 맞아 의심하지마!!
        if (runningTime > 200f)
        {
            //Debug.Log("Time out!");
            EndEpisode();
        }
        // for Terminate Condition2
        if (dist < 0.05f)
        {
            completeTime = completeTime + Time.deltaTime;
            
            if (completeTime > 1f)
            {
                AddReward(100f);
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
