using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Z_CurvedPlane_Dynamics : MonoBehaviour
{
    // Objects in UNITY
    public Transform acturatorA; // EndPoint : X rotation
    public Transform acturatorB; // Manipulator : Z rotation
   
    // Variables for xy-Plane 
    float x, y, z, theta_x, theta_y;
    static public float dx, dy, dtheta_x, dtheta_y;
    static public float ddx, ddy, ddtheta_x, ddtheta_y;

    // Variables for UNITY Global Frame
    float theta_X, theta_Z;
    float Xtmp, Ytmp, Ztmp;
    float X, Y, Z;

    // Constants and etc...
    bool first_enter = true;
    float epsilon = Mathf.Pow(10, -3);
    float L = 30f;          // Hand's Length Parameter
    float delta;
    float dist;
    static float mb = 1.057f * Mathf.Pow(10, -3);
    static float rb = 3.175f * Mathf.Pow(10, -3);
    static float Jb = 4.262f * Mathf.Pow(10, -9);
    static float mp = 10f * Mathf.Pow(10, -3);
    static float mu = 0.02604f; // Optimization 0.03604f
    static float g = 9.81f;
    float F_mu_x, F_mu_y;

    float[] x_lim = { -2.946f, 2.946f };
    float[] y_lim = { -3.602f, 3.602f };
    float[] z_lim = { 0.4375001f, 0.4375001f };

    //float[] x_lim = { -1.78f, 1.8f };
    //float[] y_lim = { -2.22f, 2.18f };
    //float[] z_lim = { 0.001968113f, 0.4185f };   // 0.4185f

    // Curved Plane
    private float coefficient_curve;
    float bias = -0.2f;
    Vector2 adjacent_side = new Vector2();
    Vector2 hypotenuse = new Vector2();
    Vector2 ro_adjacent_side = new Vector2();
    Vector2 ro_hypotenuse = new Vector2();
    float curve_theta;

    // Curve Render
    public LineRenderer lr;
    List<Vector3> init_planePos = new List<Vector3>();
    List<Vector3> rotated_planePos = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        initState();

        coefficient_curve = 0f;

        // for Line Render 
        float start = -2.2f;
        float end = 2.2f;
        for(int i=0; i< 20; i++){
            init_planePos.Add(new Vector3(0, start + i * (end - start) / 19, coefficient_curve * (start + i * (end - start) / 19) * (start + i * (end - start) / 19) + bias));
            rotated_planePos.Add(new Vector3());
        }

        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        dist = 10f;
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        delta = Time.deltaTime;
        LeftHanded_To_RightHanded();
        UpdateFlatDynamics();
        UpdateCurvedDynamics();
        dist = Mathf.Pow(x * x + y * y, 0.5f);
        DrawLine();
    }

    void DrawLine(){
        for (int i = 0; i < init_planePos.Count; i++)
        {
            Vector3 init = init_planePos[i];
            Vector3 tmp = new Vector3();
            Vector3 rotated = new Vector3();
            tmp[0] = init[0];
            tmp[2] = Mathf.Cos(theta_X) * (L - init[1]) - Mathf.Sin(theta_X) * init[2];
            tmp[1] = Mathf.Sin(theta_X) * (L - init[1]) + Mathf.Cos(theta_X) * init[2];

            rotated[0] = init[0];
            rotated[2] = L - tmp[2];
            rotated[1] = Mathf.Sin(theta_Z) * tmp[0] + Mathf.Cos(theta_Z) * tmp[1];

            rotated_planePos[i] = rotated;
        }


        for (int i = 0; i < rotated_planePos.Count; i++)
        {
            lr.SetPosition(i, rotated_planePos[i]);
        }
    }

    void initState(){
        // Observation
        theta_X = acturatorA.localEulerAngles.x;
        theta_Z = acturatorB.eulerAngles.z;
        // 0 ~ 360 -> -PI ~ +PI
        theta_X = theta_X < 180 ? theta_X * Mathf.Deg2Rad : (theta_X - 360) * Mathf.Deg2Rad;
        theta_Z = theta_Z < 180 ? theta_Z * Mathf.Deg2Rad : (theta_Z - 360) * Mathf.Deg2Rad;

        // UNITY's theta to xy-Plane's theta
        theta_x = theta_X;
        theta_y = theta_Z;

        // Initialization
        x = Random.Range(x_lim[0], x_lim[1]);
        y = Random.Range(y_lim[0], y_lim[1]);
        z = z_lim[1];
        dx = 0f;
        dy = 0f;
        dtheta_x = 0f;
        dtheta_y = 0f;
        F_mu_x = Mathf.Abs(dx) < epsilon ? 0 : -mu * mb * g * Mathf.Cos(theta_y) * Mathf.Sign(dx);
        F_mu_y = Mathf.Abs(dy) < epsilon ? 0 : -mu * mb * g * Mathf.Cos(theta_y) * Mathf.Sign(dy);
    }

    void LeftHanded_To_RightHanded(){
        // Observation
        X = transform.position.x;
        Y = transform.position.y;
        Z = transform.position.z;
        theta_X = acturatorA.localEulerAngles.x;
        theta_Z = acturatorB.eulerAngles.z;

        // 0 ~ 360 -> -PI ~ +PI
        theta_X = theta_X < 180 ? theta_X * Mathf.Deg2Rad : (theta_X - 360) * Mathf.Deg2Rad;
        theta_Z = theta_Z < 180 ? theta_Z * Mathf.Deg2Rad : (theta_Z - 360) * Mathf.Deg2Rad;

        // UNITY's theta to xy-Plane's theta
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
    }

	void UpdateFlatDynamics(){
        /*###################### Dynamics ######################*/
        dist = Mathf.Pow(x * x + y * y, 0.5f);
        //print("x : " + x + " / y : " + y + " / Dist : " + dist);
        // Around Hole
        if (dist < 0.5)
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
            x = x + dx * delta;
            y = y + dy * delta;

            F_mu_x = Mathf.Abs(dx) < epsilon ? 0f : -mu * mb * g * Mathf.Cos(theta_y) * Mathf.Sign(dx);
            F_mu_y = Mathf.Abs(dy) < epsilon ? 0f : -mu * mb * g * Mathf.Cos(theta_x) * Mathf.Sign(dy);

            dx = dx + ddx * delta;
            dy = dy + ddy * delta;
            ddx = -mb * g / (Jb / (rb * rb) + mb) * theta_y + F_mu_x / (Jb / (rb * rb) + mb);
            ddy = -mb * g / (Jb / (rb * rb) + mb) * theta_x + F_mu_y / (Jb / (rb * rb) + mb);

            // Condition Check
            if (x < x_lim[0])
            {
                x = x_lim[0];
                dx = 0f;
            }
            if (x > x_lim[1])
            {
                x = x_lim[1];
                dx = 0f;
            }
            if (y < y_lim[0])
            {
                y = y_lim[0];
                dy = 0f;
            }
            if (y > y_lim[1])
            {
                y = y_lim[1];
                dy = 0f;
            }

        }
    }

    void UpdateCurvedDynamics()
    {
        // For Curved dynamics
        adjacent_side = new Vector2(y, 0);
        hypotenuse = new Vector2(x, coefficient_curve * x* x); // y, z

        ro_adjacent_side[0] = adjacent_side[0] * Mathf.Cos(theta_y) - adjacent_side[1] * Mathf.Sin(theta_y);
        ro_adjacent_side[1] = adjacent_side[0] * Mathf.Sin(theta_y) + adjacent_side[1] * Mathf.Cos(theta_y);
        ro_hypotenuse[0] = hypotenuse[0] * Mathf.Cos(theta_y) - hypotenuse[1] * Mathf.Sin(theta_y);
        ro_hypotenuse[1] = hypotenuse[0] * Mathf.Sin(theta_y) + hypotenuse[1] * Mathf.Cos(theta_y);

        curve_theta = Mathf.Atan2(hypotenuse[0] - rb * rb / x, hypotenuse[1]);
        if (Mathf.Pow(x * x, 0.5f) < 0.1f) curve_theta = 0f;

        if (curve_theta > 0) curve_theta = Mathf.Deg2Rad * 90 - curve_theta;
        else if (curve_theta < 0) curve_theta = -Mathf.Abs(Mathf.Deg2Rad * 90 + curve_theta);

        //print(theta_x*Mathf.Rad2Deg + ", " + curve_theta*Mathf.Rad2Deg + ", " + (theta_x + curve_theta) * Mathf.Rad2Deg);

        theta_y = theta_y + curve_theta;
        x = x;
        y = y;
        z = hypotenuse[1];

        /*###################### Dynamics ######################*/
        // Around Hole
        if (dist < 0.05f)
        {
            if (first_enter)
            {
                this.GetComponent<Rigidbody>().velocity = new Vector3(-dx, 0f, -dy);
                first_enter = !first_enter;
            }

            this.GetComponent<Rigidbody>().isKinematic = false;
        }
        // Plane
        else
        {
            this.GetComponent<Rigidbody>().isKinematic = true;
            x = x + dx * delta;
            y = y + dy * delta;

            F_mu_x = Mathf.Abs(dx) < epsilon ? 0f : -mu * mb * g * Mathf.Cos(theta_y) * Mathf.Sign(dx);
            F_mu_y = Mathf.Abs(dy) < epsilon ? 0f : -mu * mb * g * Mathf.Cos(theta_x) * Mathf.Sign(dy);

            dx = dx + ddx * delta;
            dy = dy + ddy * delta;
            ddx = -mb * g / (Jb / (rb * rb) + mb) * theta_y + F_mu_x / (Jb / (rb * rb) + mb);
            ddy = -mb * g / (Jb / (rb * rb) + mb) * theta_x + F_mu_y / (Jb / (rb * rb) + mb);

            // Condition Check
            if (x < x_lim[0])
            {
                x = x_lim[0];
                dx = 0f;
            }
            if (x > x_lim[1])
            {
                x = x_lim[1];
                dx = 0f;
            }
            if (y < y_lim[0])
            {
                y = y_lim[0];
                dy = 0f;
            }
            if (y > y_lim[1])
            {
                y = y_lim[1];
                dy = 0f;
            }

            // Chage of Basis(To Global Frame)      (X, Y, Z)는 왼손 좌표계. (x, y, z)는 오른손 좌표계
            X = -x;
            Z = -y;
            Y = z;
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

    public void SetCurveCoefficient(float n){
        coefficient_curve = n;
	}

    public float getBallX(){
        return -x;
    }

    public float getBallZ()
    {
        return -y;
    }

    public float getDist(){
        return dist;
    }
}


