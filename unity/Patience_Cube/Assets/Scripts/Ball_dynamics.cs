using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball_dynamics : MonoBehaviour
{
    // Objects in UNITY
    public Transform plane;
    public Transform Cube_x; // EndPoint : X rotation acturatorA
    public Transform Cube_z; // Manipulator : Z rotation acturatorB
    public float g;
    public float mu;
    // Variables for xy-Plane 
    public static float x, y, z, theta_x, theta_y;
    public static float dx, dy;
    public static float ddx, ddy;

    // Variables for UNITY Global Frame
    float theta_X, theta_Z;
    float Xtmp, Ytmp, Ztmp;
    float X, Y, Z;

    // Constants and etc...
    bool first_enter = true;
    float epsilon = Mathf.Pow(10, -7);
    float L = 30f;          // Hand's Length Parameter
    float dt;
    public static float dist;
    static float mb = 1.057f * Mathf.Pow(10, -3);
    static float rb = 3.175f * Mathf.Pow(10, -3);
    static float Jb = 4.262f * Mathf.Pow(10, -9);
    static float mp = Mathf.Pow(10, -3);
    //static float mu = 0.1f; // Optimization
    //static float g = 9.81f;
    public static float F_mu_x, F_mu_y;
    public static float[] x_lim = { -2.946f, 2.946f };
    public static float[] y_lim = { -3.602f, 3.602f };
    public static float[] z_lim = { 0.4375001f, 0.4375001f };

    // Start is called before the first frame update
    void Start()
    {
        // Observation
        theta_X = Cube_x.localEulerAngles.x;
        theta_Z = Cube_z.eulerAngles.z;
        // 0 ~ 360 -> -PI ~ +PI
        theta_X = theta_X < 180 ? theta_X * Mathf.Deg2Rad : (theta_X - 360) * Mathf.Deg2Rad;
        theta_Z = theta_Z < 180 ? theta_Z * Mathf.Deg2Rad : (theta_Z - 360) * Mathf.Deg2Rad;

        // UNITY's theta to xy-Plane's theta
        theta_x = theta_X;
        theta_y = theta_Z;

        // Initialization
        dx = 0f;
        dy = 0f;
        F_mu_x = Mathf.Abs(dx) < epsilon ? 0 : -mu * mb * g * Mathf.Cos(theta_y) * Mathf.Sign(dx);
        F_mu_y = Mathf.Abs(dy) < epsilon ? 0 : -mu * mb * g * Mathf.Cos(theta_y) * Mathf.Sign(dy);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        dt = Time.deltaTime;
        /*###################### Observation ######################*/
        // Ball Position
        X = transform.position.x;
        Y = transform.position.y;
        Z = transform.position.z;

        // Cube Rotation (Degree)
        theta_X = Cube_x.localEulerAngles.x;
        theta_Z = Cube_z.eulerAngles.z;
        // 0 ~ 360 -> -PI ~ +PI
        theta_X = theta_X < 180 ? theta_X * Mathf.Deg2Rad : (theta_X - 360) * Mathf.Deg2Rad;
        theta_Z = theta_Z < 180 ? theta_Z * Mathf.Deg2Rad : (theta_Z - 360) * Mathf.Deg2Rad;

        // UNITY's theta to xy-Plane's theta (Radian)
        theta_x = theta_X;
        theta_y = -theta_Z;

        // UNITY's XYZ to xy-Plane's xyz
        Xtmp = Mathf.Cos(theta_Z) * X + Mathf.Sin(theta_Z) * Y;
        Ztmp = L - Z;
        Ytmp = -Mathf.Sin(theta_Z) * X + Mathf.Cos(theta_Z) * Y;
        X = Xtmp;
        Z = -(Mathf.Cos(theta_X) * Ztmp + Mathf.Sin(theta_X) * Ytmp - L);
        Y = -Mathf.Sin(theta_X) * Ztmp + Mathf.Cos(theta_X) * Ytmp;
        x = -X;
        y = -Z;
        z = Y;


        /*###################### Dynamics ######################*/
        dist = Mathf.Pow(x * x + y * y, 0.5f);
        //print("x : " + x + " / y : " + y + " / Dist : " + dist);
        // Around Hole
        if (dist < 0.193f)
        {
            if (first_enter)
            {
                this.GetComponent<Rigidbody>().velocity = new Vector3(-dx, 0f, -dy);
                first_enter = !first_enter;
            }
            dx = -this.GetComponent<Rigidbody>().velocity.x;
            dy = -this.GetComponent<Rigidbody>().velocity.z;
            this.GetComponent<Rigidbody>().isKinematic = false;
        }
        // Plane
        else
        {
            if (!first_enter)
            {
                dx = -this.GetComponent<Rigidbody>().velocity.x;
                dy = -this.GetComponent<Rigidbody>().velocity.z;
                first_enter = !first_enter;
            }

            this.GetComponent<Rigidbody>().isKinematic = true;
            x = x + dx * dt;
            y = y + dy * dt;

            F_mu_x = Mathf.Abs(dx) < epsilon ? 0f : -mu * mb * g * Mathf.Cos(theta_y) * Mathf.Sign(dx);
            F_mu_y = Mathf.Abs(dy) < epsilon ? 0f : -mu * mb * g * Mathf.Cos(theta_x) * Mathf.Sign(dy);

            dx = dx + ddx * dt;
            dy = dy + ddy * dt;
            ddx = -mb * g / (Jb / (rb * rb) + mb) * theta_y + F_mu_x / (Jb / (rb * rb) + mb);
            ddy = -mb * g / (Jb / (rb * rb) + mb) * theta_x + F_mu_y / (Jb / (rb * rb) + mb);

            /*###################### Kinematics ######################*/
            // Condition Check
            if (x < x_lim[0])
            { x = x_lim[0]; dx = 0f; }
            if (x > x_lim[1])
            { x = x_lim[1]; dx = 0f; }
            if (y < y_lim[0])
            { y = y_lim[0]; dy = 0f; }
            if (y > y_lim[1])
            { y = y_lim[1]; dy = 0f; }

            // Chage of Basis(To Global Frame)
            X = -x;
            Z = -y;
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
            transform.SetPositionAndRotation(new Vector3(X, Y, Z), transform.rotation);
        }
    }
    public float getBallX()
    {
        return -x;
    }

    public float getBallZ()
    {
        return -y;
    }

    public float getDist()
    {
        return dist;
    }
}


