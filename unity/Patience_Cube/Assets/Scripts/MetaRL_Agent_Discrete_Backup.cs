using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MetaRL_Agent_Discrete_Backup : Agent
{
    // ML-Agent
    float angle = 1f;
    float accX = 0f, accZ = 0f;
    float dt;

    // Env
    public GameObject env;  // ball
    public Rigidbody ballRigidbody;
    List<Vector3> fixed_init_point = new List<Vector3>();

    // TODO ball, hole 위치 잡아주기.
    float ballX = 0f, ballZ = 0f;
    float holeX = 0f, holeZ = 0f;
    float dist = 0f;

    // Observation
    public Transform acturatorA; // EndPoint : X rotation
    public Transform acturatorB; // Manipulator : Z rotation
    Quaternion tmpEuler = Quaternion.Euler(0f, 0f, 0f);

    // Reward
    float relativePos, relativeTilt, relativeAction;
    float rewardPos, rewardTilt, rewardAction;
    float runningTime = 0.1f;

    // TODO: Random coefficient를 갖도록 설정
    private void Create_DumyEnv()
    {
        // Plane Curverture
        // float random_curverture = UnityEngine.Random.Range(0f, 0.5f);   // min -0.1f or 0f
        float curverture = -0.10f;

        var dummy_env = env.GetComponent<X_CurvedPlane_Dynamics>();
        dummy_env.SetCurveCoefficient(curverture);

        // Ball Position    X: -1.35 ~ 1.35, Z: -2.2 ~ 2.2

        // fixed_init_point.Add(new Vector3(-1.3f, 0.45f, -1.7f));
        // fixed_init_point.Add(new Vector3(1.3f, 0.45f, -1.7f));
        // fixed_init_point.Add(new Vector3(-1.3f, 0.45f, 1.7f));
        // fixed_init_point.Add(new Vector3(1.3f, 0.45f, 1.7f));

        // fixed_init_point.Add(new Vector3(0f, 0.45f, -1.5f));
        // fixed_init_point.Add(new Vector3(0f, 0.45f, 1.5f));
        // fixed_init_point.Add(new Vector3(1.3f, 0.45f, 0f));
        // fixed_init_point.Add(new Vector3(-1.3f, 0.45f, 0f));
        // int random_idx = UnityEngine.Random.Range(0, 9);

        // ballRigidbody.transform.position = fixed_init_point[random_idx];
        float bX = Random.Range(-1.35f, -1f);
        float bZ = Random.Range(-2.2f, -1.5f);

        int signX = Random.Range(0, 2);
        if (signX == 0) signX = -1;

        int signZ = Random.Range(0, 2);
        if (signZ == 0) signZ = -1;

        ballRigidbody.transform.position = new Vector3(bX * signX, 0.45f, bZ * signZ);
        ballRigidbody.velocity = new Vector3(0, 0, 0);

        acturatorA.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        acturatorB.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    private void Reward()
    {
        float c1 = -0.001f * 1000, c2 = -0.1f, c3 = -0.05f;
        //float alpha = 1f, beta = 100f, gamma = 1f;
        float alpha = 1f, beta = 1f, gamma = 1f;

        relativePos = Mathf.Pow(holeX - ballX, 2) + Mathf.Pow(holeZ - ballZ, 2);
        rewardPos = Mathf.Exp(c1 * relativePos);

        relativeTilt = Mathf.Pow(acturatorA.transform.localRotation.x, 2) + Mathf.Pow(acturatorB.transform.localRotation.z, 2);
        rewardTilt = c2 * relativeTilt;

        relativeAction = Mathf.Pow(accX, 2) + Mathf.Pow(accZ, 2);
        rewardAction = c3 * relativeAction;

        AddReward(alpha * rewardPos + beta * rewardTilt + gamma * rewardAction);
        // Debug.Log("Reward: " + (alpha * rewardPos + beta * rewardTilt + gamma * rewardAction));
    }

    public override void Initialize()
    {
        //Create_DumyEnv();
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
        // -------------------------------------------------------
        sensor.AddObservation(holeX - ballX);
        sensor.AddObservation(holeZ - ballZ);
        // -------------------------------------------------------
        sensor.AddObservation(ballRigidbody.velocity.x);
        sensor.AddObservation(ballRigidbody.velocity.z);
        // -------------------------------------------------------
        var yaw = acturatorA.transform.rotation.x;
        var roll = acturatorB.transform.rotation.z;
        sensor.AddObservation(yaw);
        sensor.AddObservation(roll);
        // -------------------------------------------------------
        sensor.AddObservation((yaw - tmpEuler.x) / Time.deltaTime);
        sensor.AddObservation((roll - tmpEuler.z) / Time.deltaTime);
        tmpEuler.x = yaw;
        tmpEuler.z = roll;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        # region Preprocessing

        var curvedDynamics = env.GetComponent<X_CurvedPlane_Dynamics>();
        ballX = curvedDynamics.getBallX();
        ballZ = curvedDynamics.getBallZ();

        dist = curvedDynamics.getDist();

        # endregion Preprocessing

        var action = actionBuffers.DiscreteActions[0];
        // Debug.Log(action);
        // action = 5;

        // Action 수행
        switch (action)
        {
            // Yaw: X 회전, Roll: Z 회전
            case 0:
                accX = 1f;
                accZ = 0f;
                break;

            case 1:
                accX = 1f;
                accZ = 1f;
                break;

            case 2:
                accX = 0f;
                accZ = 1f;
                break;

            case 3:
                accX = -1f;
                accZ = 1f;
                break;

            case 4:
                accX = -1f;
                accZ = 0f;
                break;

            case 5:
                accX = -1f;
                accZ = -1f;
                break;

            case 6:
                accX = 0f;
                accZ = -1f;
                break;

            case 7:
                accX = 1f;
                accZ = -1f;
                break;

            case 8:
                accX = 0f;
                accZ = 0f;
                break;

            default:
                throw new System.ArgumentException("Invalid action value");
        }
        acturatorA.transform.Rotate(new Vector3(1f, 0f, 0f), accX * angle * Time.deltaTime);
        acturatorB.transform.Rotate(new Vector3(0f, 0f, 1f), accZ * angle * Time.deltaTime);

        // 과꺾 방지 X축
        if (acturatorA.transform.rotation.x > 30f * Mathf.Deg2Rad)
        {
            acturatorA.transform.Rotate(new Vector3(1f, 0f, 0f), -accX * angle * Time.deltaTime);
        }
        else if (acturatorA.transform.rotation.x < -30f * Mathf.Deg2Rad)
        {
            acturatorA.transform.Rotate(new Vector3(1f, 0f, 0f), -accX * angle * Time.deltaTime);
        }

        // 과꺾 방지 Z축
        if (acturatorB.transform.rotation.z > 20f * Mathf.Deg2Rad)
        {
            acturatorB.transform.Rotate(new Vector3(0f, 0f, 1f), -accZ * angle * Time.deltaTime);
        }
        else if (acturatorB.transform.rotation.z < -20f * Mathf.Deg2Rad)
        {
            acturatorB.transform.Rotate(new Vector3(0f, 0f, 1f), -accZ * angle * Time.deltaTime);
        }

        // for Terminate Condition
        if ((dist < 0.1f) && ((float)Mathf.Round(ballRigidbody.velocity.magnitude * 100f) / 100f <= 0.5f)){
            Debug.Log("Goal");
            Reward();
            AddReward(30f);
            EndEpisode();
		}

        Reward();
        AddReward(-0.1f);
    }

}
