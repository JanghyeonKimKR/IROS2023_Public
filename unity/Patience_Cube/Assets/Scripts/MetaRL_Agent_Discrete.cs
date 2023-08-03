using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MetaRL_Agent_Discrete : Agent
{
    // ############ Parameters ############
    // ML-Agent
    public float max_action = 0.5f;
    public int length;

    // weights
    public float c1, c2, c3, c4, c5;

    // Observation
    public Transform acturatorA; // EndPoint : X rotation
    public Transform acturatorB; // Manipulator : Z rotation

    // ball
    public Rigidbody ballRigidbody;
    //####################################

    // cube info.
    float[] ddthetaX_list;
    float[] ddthetaZ_list;
    float thetaX, thetaZ;
    float dthetaX = 0f, dthetaZ = 0f;
    float ddthetaX = 0f, ddthetaZ = 0f;

    // ball info.
    List<Vector3> fixed_init_point = new List<Vector3>();
    int num_point = 10;
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
        int random_idx = UnityEngine.Random.Range(0, num_point * 4);
        ballRigidbody.transform.position = fixed_init_point[random_idx];
        Ball_dynamics.dx = 0f;
        Ball_dynamics.dy = 0f;

        acturatorA.transform.localEulerAngles = new Vector3(0, 0, 0);
        acturatorB.transform.localEulerAngles = new Vector3(0, 0, 0);


        runningTime = 0f;
        completeTime = 0f;
        dthetaX = 0f;
        dthetaZ = 0f;
    }

    private void Reward()
    {
        // weights
        //float c1 = -1.0f, c2 = -0.1089f, c3 = -0.3173f, c4 = -0.1033f, c5 = -7.1231f, c6 = -7.1231f;  // approach1
        float c1 = -1.0f, c2 = -0.0006f, c3 = -0.2360f, c4 = -0.0387f, c5 = -7.1106f, c6 = -7.1106f;  // approach2
        //float c1 = -1.0f, c2 = -0.2774f, c3 = -0.3470f, c4 = -0.1313f, c5 = -7.1061f, c6 = -7.1061f;   // approach3

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
        relativeAction = c5 * Mathf.Pow(ddthetaX, 2) + c6 * Mathf.Pow(ddthetaZ, 2);
        rewardAction = relativeAction;

        SetReward(rewardPos + rewardVel + rewardTheta + rewardDtheta + rewardAction);
        //print("Pos: " + rewardPos.ToString("N2") + " // Vel: " + rewardVel.ToString("N2") + " // Theta: " + 
        //    rewardTheta.ToString("N2") + " // Action: " + rewardAction.ToString("N2") + " // Total:" + (rewardPos + rewardVel + rewardTheta + rewardDtheta + rewardAction).ToString("N2"));
        // Debug.Log("Reward: " + (alpha * rewardPos + beta * rewardTilt + gamma * rewardAction));
    }

    public override void Initialize()
    {
        float x_upper = Ball_dynamics.x_lim[1];
        float x_lower = Ball_dynamics.x_lim[0];
        float y_lim = Ball_dynamics.z_lim[0];
        float z_upper = Ball_dynamics.y_lim[1];
        float z_lower = Ball_dynamics.y_lim[0];
        for (int i = 0; i < num_point; i++)
        {
            float alpha = i / (num_point - 1f);
            fixed_init_point.Add(new Vector3(x_upper, y_lim, alpha * z_lower + (1 - alpha) * z_upper));
            fixed_init_point.Add(new Vector3(x_lower, y_lim, alpha * z_lower + (1 - alpha) * z_upper));
            fixed_init_point.Add(new Vector3(alpha * x_lower + (1 - alpha) * x_upper, y_lim, z_upper));
            fixed_init_point.Add(new Vector3(alpha * x_lower + (1 - alpha) * x_upper, y_lim, z_lower));
        }

        ddthetaX_list = new float[length];
        ddthetaZ_list = new float[length];
        for (int i = 0; i < length; i++)
        {
            //ddthetaX_list[i] = (1f - 2f * (i / (length - 1.0f))) * max_action;
            //ddthetaZ_list[i] = (1f - 2f * (i / (length - 1.0f))) * max_action;
            ddthetaX_list[i] = (-1f + 2f * (i / (length - 1.0f))) * max_action;
            ddthetaZ_list[i] = (-1f + 2f * (i / (length - 1.0f))) * max_action;
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

        //print("thetaX: " + thetaX.ToString("N2") + "thetaX: " + thetaX.ToString("N2"));
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
           
        # region Preprocessing
        runningTime = runningTime + Time.deltaTime;

        dist = Ball_dynamics.dist;

        #endregion Preprocessing
        var action1 = actionBuffers.DiscreteActions[0];
        var action2 = actionBuffers.DiscreteActions[1];

        //Debug.Log(action1+", "+action2);

        // Action1 수행, Yaw 회전(X축)
        ddthetaX = ddthetaX_list[action1];
        ddthetaZ = ddthetaZ_list[action2];
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
        if (runningTime > 30f)
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
