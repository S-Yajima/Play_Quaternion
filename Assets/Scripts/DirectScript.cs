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

    // 時間経過
    private float delta_time = 0.0f;
    private float interval_time = 5.0f;
    private bool is_rotate = false;


    // 四元数の逆数を算出する。
    // q : 四元数 [w, x, y, z]
    // 共役の四元数 / 元の四元数の大きさ を算出する。(大きさが0の場合の0割り算の発生リスクは今回は無視する)
    private Vector4 minor_quot(Vector4 q)
    {
        Vector4 result = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

        // 共役を元の四元数の大きさで割る. 0割り算発生のリスクは今回は無視する.
        result.w = q.w / q.magnitude;
        result.x = -1 * (q.x / q.magnitude);
        result.y = -1 * (q.y / q.magnitude);
        result.z = -1 * (q.z / q.magnitude);

        return result;
    }


    // 四元数の積算を行う。不可逆に注意。
    // q1 : 四元数 [w, x, y, z] ([w v])
    // q2 : 四元数 [w, x, y, z] ([w v])
    // 下記の計算で四元数の積算を実施する
    // [q1.w * q2.w - q1.v・q2.v  q1.w * q2.v + q2.w * q1.v + q1.v * q2.v]
    private Vector4 cross_quot(Vector4 q1, Vector4 q2)
    {
        Vector4 result = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

        // 2つの四元数のベクトル部をVector3変数に取得する
        Vector3 q1_v3 = new Vector3(q1.x, q1.y, q1.z);
        Vector3 q2_v3 = new Vector3(q2.x, q2.y, q2.z);

        // q1.w * q2.w - q1.v・q2.v
        result.w = (q1.w * q2.w) - Vector3.Dot(q1_v3, q2_v3);
        // q1.w * q2.v + q2.w * q1.v + q1.v * q2.v
        Vector3 tmp_v3 = (q2_v3 * q1.w) + (q1_v3 * q2.w) + Vector3.Cross(q1_v3, q2_v3);
        result.x = tmp_v3.x;
        result.y = tmp_v3.y;
        result.z = tmp_v3.z;

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
        Vector4 p1 = new Vector4(this.sphere_1_position_v3.x, this.sphere_1_position_v3.y, this.sphere_1_position_v3.z);
        Vector4 p2 = new Vector4(this.sphere_2_position_v3.x, this.sphere_2_position_v3.y, this.sphere_2_position_v3.z);
        // 角変位を示す四元数. Vector4() コンストラクタの引数順序に注意。wは最後。
        Vector4 q = new Vector4(sin_val * this.quot_axis_v3.x, sin_val * this.quot_axis_v3.y, sin_val * this.quot_axis_v3.z, cos_val);
        // 角変位を示す四元数の逆数
        Vector4 q_minor = minor_quot(q);


        // 四元数の積算で回転後の座標を算出しオブジェクトに設定する: 角変位の四元数 * 座標の四元数 * 逆数の四元数
        Vector4 qp1 = cross_quot(q, p1);
        Vector4 qp1q_minor = cross_quot(qp1, q_minor);
        this.sphere_1.transform.position = new Vector3(qp1q_minor.x, qp1q_minor.y, qp1q_minor.z);

        Vector4 qp2 = cross_quot(q, p2);
        Vector4 qp2q_minor = cross_quot(qp2, q_minor);
        this.sphere_2.transform.position = new Vector3(qp2q_minor.x, qp2q_minor.y, qp2q_minor.z);
    }
}
