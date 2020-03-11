using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class MyCameraControl : MonoBehaviour
{
    public Camera mainCam;
    //旋转变量;
    private float m_deltX = 0f;
    private float m_deltY = 0f;
    //缩放变量;
    private float m_distance = 10f;
    private float m_mSpeed = 5f;
    //移动变量;
    private Vector3 m_mouseMovePos = Vector3.zero;
    //平移速度
    float Speed = 20f;
 
    void Start()
    {
        //  transform.localPosition = new Vector3(0, m_distance, 0);
    }
 
    void Update()
    {
 
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * Time.deltaTime * Speed);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * Time.deltaTime * Speed);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.forward * Time.deltaTime * -Speed);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.left * Time.deltaTime * -Speed);
        }
        //鼠标左键控制旋转
        if (Input.GetMouseButton(0))
        {
            m_deltX += Input.GetAxis("Mouse X") * m_mSpeed;
            m_deltY -= Input.GetAxis("Mouse Y") * m_mSpeed;
            m_deltX = ClampAngle(m_deltX, -360, 360);//旋转幅度 左右
            m_deltY = ClampAngle(m_deltY, -70, 70);//旋转幅度 上下
            transform.rotation = Quaternion.Euler(m_deltY, m_deltX, 0);
        }
        //鼠标滑轮缩放
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            //自由缩放方式;
            m_distance = Input.GetAxis("Mouse ScrollWheel") * 10f;
            transform.localPosition = transform.position + transform.forward * m_distance;
        }
 
        //相机位置跳到点击处;
        if (Input.GetMouseButtonDown(1)) //0-左键 1-右键 2-滑轮
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);//从摄像机发出到点击坐标的射线
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                m_mouseMovePos = hitInfo.point;
                transform.localPosition = m_mouseMovePos;
            }
        }
 
    }
 
    float ClampAngle(float angle, float minAngle, float maxAgnle)
    {
        if (angle <= -360)
            angle += 360;
        if (angle >= 360)
            angle -= 360;
 
        return Mathf.Clamp(angle, minAngle, maxAgnle);
    }
 
 
}