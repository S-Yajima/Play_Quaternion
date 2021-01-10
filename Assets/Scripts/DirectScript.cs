using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DirectScript : MonoBehaviour
{
    // 四元数回転角度
    private float quot_angle = 0.0f;
    // 四元数回転軸
    private Vector3 quot_axis_v3 = new Vector3(0.0f, 1.0f, 0.0f);
    //private Vector3 quot_axis_v3 = new Vector3(0.0f, 0.0f, 1.0f);
    // 玉の初期位置
    private Vector3 sphere_1_position_v3 = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 sphere_2_position_v3 = new Vector3(0.0f, 0.0f, 0.0f);
    // Sphere
    private GameObject sphere_1;
    private GameObject sphere_2;
    // 矢印
    private GameObject arrow;

    // 四元数: 回転角度と軸
    private float[] q_angle_normal = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f };
    // 四元数: 元の位置
    private float[] q_position = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f };

    // 時間経過
    private float delta_time = 0.0f;
    private float interval_time = 5.0f;
    private bool is_rotate = false;


    // 四元数の逆数を算出する。
    // q : 四元数 [w, x, y, z]
    // 共役の四元数 / 元の四元数の大きさ を算出する。
    private float[] minor_quot(float[] q)
    {
        float[] result = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f };

        float q_length = (float)Math.Sqrt(Math.Pow(q[0], 2) + Math.Pow(q[1], 2) + Math.Pow(q[2], 2) + Math.Pow(q[3], 2));
        // 共役を元の四元数の大きさで割る
        result[0] = q[0] / q_length;
        result[1] = -1 * (q[1] / q_length);
        result[2] = -1 * (q[2] / q_length);
        result[3] = -1 * (q[3] / q_length);

        return result;
    }


    // 四元数の積算を行う。不可逆に注意。
    // q1 : 四元数 [w, x, y, z] ([w v])
    // q2 : 四元数 [w, x, y, z] ([w v])
    // 下記の計算で四元数の積算を実施する
    // [q1.w * q2.w - q1.v・q2.v  q1.w * q2.v + q2.w * q1.v + q1.v * q2.v]
    private float[] cross_quot(float[] q1, float[] q2)
    {
        float[] result = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f };

        // 2つの四元数のベクトル部をVector3変数に取得する
        Vector3 q1_v3 = new Vector3(q1[1], q1[2], q1[3]);
        Vector3 q2_v3 = new Vector3(q2[1], q2[2], q2[3]);

        // q1.w * q2.w - q1.v・q2.v
        result[0] = (q1[0] * q2[0]) - Vector3.Dot(q1_v3, q2_v3);
        // q1.v・q2.v  q1.w * q2.v + q2.w * q1.v + q1.v * q2.v
        Vector3 tmp_v3 = (q2_v3 * q1[0]) + (q1_v3 * q2[0]) + Vector3.Cross(q1_v3, q2_v3);
        result[1] = tmp_v3.x;
        result[2] = tmp_v3.y;
        result[3] = tmp_v3.z;

        return result;
    }


    // 回転軸と矢印を回転する
    void rotate_pole(int angle)
    {
        this.quot_axis_v3 = Quaternion.Euler(angle, 0, 0) * this.quot_axis_v3;
        this.arrow.transform.rotation = Quaternion.Euler(angle, 0, 0) * this.arrow.transform.rotation;

        //this.arrow.transform.Rotate(angle, 0, 0);
    }


    // Start is called before the first frame update
    void Start()
    {
        // SceneのGameObjectをメンバ変数に取得する
        this.sphere_1 = GameObject.Find("Sphere_1");
        this.sphere_1_position_v3 = this.sphere_1.transform.position;
        this.sphere_2 = GameObject.Find("Sphere_2");
        this.sphere_2_position_v3 = this.sphere_2.transform.position;
        this.arrow = GameObject.Find("Arrow");

    }

    // Update is called once per frame
    void Update()
    {
        // 2秒間回転軸を回す。5秒間回転軸を静止する。
        this.delta_time += Time.deltaTime;
        if(this.delta_time >= this.interval_time)
        {
            this.delta_time = 0.0f;
            this.is_rotate = !(this.is_rotate);
            if (this.is_rotate == true) this.interval_time = 0.5f;
            else this.interval_time = 5.0f;
        }
        if (this.is_rotate == true)
        {
            rotate_pole(1);
        }

        // 角度をインクリメント
        this.quot_angle += 1;
        if (this.quot_angle > 360) this.quot_angle = 0;

        // sin, cosを計算
        double half_radian = (this.quot_angle * 0.5) * Math.PI / 180;
        float cos_val = (float)Math.Cos(half_radian);
        float sin_val = (float)Math.Sin(half_radian);

        // 四元数をfloatの配列(4要素)で生成する
        // Sphereの座標を示す四元数
        float[] p1 = new float[4] { 0.0f, this.sphere_1_position_v3.x, this.sphere_1_position_v3.y, this.sphere_1_position_v3.z };
        float[] p2 = new float[4] { 0.0f, this.sphere_2_position_v3.x, this.sphere_2_position_v3.y, this.sphere_2_position_v3.z };
        // 角変位を示す四元数
        float[] q = new float[4] { cos_val, sin_val * this.quot_axis_v3.x, sin_val * this.quot_axis_v3.y, sin_val * this.quot_axis_v3.z };
        // 角変位を示す四元数の逆数
        float[] q_minor = minor_quot(q);

        // 四元数の積算で回転後の座標を算出しオブジェクトに設定する: 角変位の四元数 * 座標の四元数 * 逆数の四元数
        float[] qp1 = cross_quot(q, p1);
        float[] qp1q_minor = cross_quot(qp1, q_minor);
        this.sphere_1.transform.position = new Vector3(qp1q_minor[1], qp1q_minor[2], qp1q_minor[3]);

        float[] qp2 = cross_quot(q, p2);
        float[] qp2q_minor = cross_quot(qp2, q_minor);
        this.sphere_2.transform.position = new Vector3(qp2q_minor[1], qp2q_minor[2], qp2q_minor[3]);
    }
}
