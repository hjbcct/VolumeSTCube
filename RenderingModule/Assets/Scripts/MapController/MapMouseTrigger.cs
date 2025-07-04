using AxisController;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityVolumeRendering;

public class MapMouseTrigger : MonoBehaviour
{
    UnityEngine.RaycastHit hit;
    public LayerMask mapHighlightClickableLayer;
    private VolumeControllerObject volumeControllerObj;
    public bool isSetHighlightPosition = false;
    public bool dragable = true;
    public Toggle toggle;
    public ShiningText shiningText;

    private AxisContainer axisContainer;

    // ��������Բ
    public GameObject hollowCirclePrefab; // ����ԲԤ����
    private GameObject hollowCircleInstance; // ����Բʵ��
    private LineRenderer hollowCircleLineRenderer;
    private int circleSegments = 500; // Բ�ķֶ���

    public void changeHighlightState()
    {
        setIsSetHighlightPosition(!isSetHighlightPosition);
        //toggle.isOn = isSetHighlightPosition;
    }

    public void setIsSetHighlightPosition(bool state)
    {
        isSetHighlightPosition = state;
        //FindObjectOfType<UIBehaviour>().setSpatialClippingStateText(isSetHighlightPosition);
        toggle.SetIsOnWithoutNotify(state);
        shiningText.isActive = state;
    }

    // Start is called before the first frame update
    void Start()
    {
        axisContainer = FindObjectOfType<AxisContainer>();
        volumeControllerObj = FindObjectOfType<VolumeControllerObject>();

        // ʵ��������Բ
        if (hollowCirclePrefab != null)
        {
            hollowCircleInstance = Instantiate(hollowCirclePrefab, Vector3.zero, Quaternion.identity, transform);  //transform ��Ϊ������ʹ����Բ���ͼͼ��һ���ƶ�����ת
            hollowCircleLineRenderer = hollowCircleInstance.GetComponent<LineRenderer>();
            if (hollowCircleLineRenderer != null)
            {
                // ������ɫΪ��ɫ              
                hollowCircleLineRenderer.startColor = Color.black;
                hollowCircleLineRenderer.endColor = Color.black;


                // ����������ȼ�С
                hollowCircleLineRenderer.startWidth = 0.02f;
                hollowCircleLineRenderer.endWidth = 0.02f;
            }

            hollowCircleInstance.SetActive(false); // ��ʼʱ����
        }
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (isSetHighlightPosition)
        {
            //  ����Space���˳����ø���λ��
            if(Input.GetKeyDown(KeyCode.Space))
            {
                setIsSetHighlightPosition(!isSetHighlightPosition);   
            }

            if(Physics.Raycast(ray, out hit, 1000, mapHighlightClickableLayer.value))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    Vector3 hitPoint = hit.point;
                    float positionX = (hitPoint.x + 5) / 10;
                    float positionY = (hitPoint.z + 5) / 10;

                    volumeControllerObj.SetHighlightPosition(new Vector2(positionX, positionY));
                    axisContainer.setPosition(new Vector2(hitPoint.x, hitPoint.z));  //�������

                    // ���¿���Բ��λ�ú�����
                    if (hollowCircleInstance != null)
                    {
                        hollowCircleInstance.SetActive(true);

                        Vector3 hollowCirclePosition = new Vector3(hitPoint.x, transform.position.y, hitPoint.z);
                        hollowCircleInstance.transform.position = hollowCirclePosition;
                        UpdateHollowCircle(hitPoint, volumeControllerObj.GetHighlightRadius());
                    }
                }
            }
            else
            {
                // �������δ���пɵ��ͼ�㣬���ؿ���Բ
                if (hollowCircleInstance != null)
                {
                    hollowCircleInstance.SetActive(false);
                }
            }


            //  ���ֵ��������뾶
            float radius = Mathf.Clamp(volumeControllerObj.GetHighlightRadius() + Input.mouseScrollDelta.y * 0.015f, 0.0f, 0.707f);
            volumeControllerObj.SetHighlightRadius(radius);
            float axisScaleX = Mathf.Clamp((radius + 0.05f) * 2 * 10, 2.0f, 10.0f);
            float axisScaleZ = Mathf.Clamp((radius + 0.05f) * 2 * 10, 2.0f, 10.0f);
            axisContainer.setScale(new Vector3(axisScaleX, -1, axisScaleZ));

            // ���¿���Բ������
            if (hollowCircleInstance != null && hollowCircleLineRenderer != null)
            {
                if(radius >= 0.69)
                {
                    hollowCircleInstance.SetActive(false);
                }
                else
                {
                    UpdateHollowCircle(hit.point, (radius) * 10);
                }
            }
        }
        else
        {
            // ���������ø���λ��ʱ�����ؿ���Բ
            if (hollowCircleInstance != null)
            {
                print(transform.position.y);
                Vector3 hollowCirclePosition = new Vector3(volumeControllerObj.GetHighlightPosition().x * 10 - 5, transform.position.y, volumeControllerObj.GetHighlightPosition().y * 10 - 5); // Slight offset to prevent z-fighting
                hollowCircleInstance.transform.position = hollowCirclePosition;
                UpdateHollowCircle(hollowCirclePosition, volumeControllerObj.GetHighlightRadius() * 10);
            }
        }

    }

    //��Բ
    void UpdateHollowCircle(Vector3 center, float radius)
    {
        if (hollowCircleLineRenderer == null)
            return;

        // ����Բ�Ķ���
        Vector3[] points = new Vector3[circleSegments + 1];
        float angle = 0f;
        float step = 360f / circleSegments;

        for (int i = 0; i <= circleSegments; i++)
        {
            float rad = Mathf.Deg2Rad * angle;
            float x = Mathf.Cos(rad) * radius;
            float z = Mathf.Sin(rad) * radius;
            points[i] = new Vector3(x, 0.02f, z) + center;
            angle += step;
        }

        // ���� LineRenderer ��λ��
        hollowCircleLineRenderer.positionCount = points.Length;
        hollowCircleLineRenderer.SetPositions(points);
    }
}
