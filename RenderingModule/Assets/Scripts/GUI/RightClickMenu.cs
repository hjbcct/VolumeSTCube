using System;
using UnityEngine;

public class RightClickMenu : MonoBehaviour
{
    public MenuButton buttonPrefab; // ���ư�ť��Ԥ����
    public int numberOfButtons = 4; // ��ť����
    public float radius = 4f; // ��ť���Ƶİ뾶
    public float longPressDuration = 1f; // �����Ҽ��ĳ���ʱ��
    public float resetDistanceThreshold = 100f; // �����Ҽ����ƶ�������ֵ
    public float intervalX = 120f; // ��ť֮��ļ��
    public float littleOffset = 60f; // ��ť��΢��
    public float animationDuration = 0.5f;  //  ���ֶ�������ʱ��

    private MenuButton[] buttons;
    private bool isRightClicking = false;
    private bool menuActive = false;
    private float rightClickStartTime = 0f;
    private Vector3 targetMousePosition;
    private Vector3 initialMousePosition;
    private Vector3[] initialButtonPositions;
    private Vector3[] targetButtonPositions;
    private float animationStartTime;

    public float tiltAmount = 30f; // ��б�����Ƕ�
    public float tiltSpeed = 5f; // ��б�ٶ�

    void Start()
    {
        buttons = new MenuButton[numberOfButtons];
        initialButtonPositions = new Vector3[numberOfButtons];
        targetButtonPositions = new Vector3[numberOfButtons];

        for (int i = 0; i < numberOfButtons; i++)
        {
            buttons[i] = Instantiate(buttonPrefab, transform);
            buttons[i].SetActive(false);
        }

        buttons[0].SetText("�� Setter");
        buttons[1].SetText("Opacity");
        buttons[2].SetText("TemporalSlice");
        buttons[3].SetText("SpatialSlice");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            initialMousePosition = Input.mousePosition;
            isRightClicking = true;
            rightClickStartTime = Time.time;
        }

        if (isRightClicking && !menuActive && Vector3.Distance(Input.mousePosition, initialMousePosition) > resetDistanceThreshold)
        {
            isRightClicking = false;
            foreach (MenuButton button in buttons)
            {
                button.SetActive(false);
            }
            menuActive = false;
        }

        if (Input.GetMouseButton(0))
        {
            isRightClicking = false;
            rightClickStartTime = Time.time;
            foreach (MenuButton button in buttons)
            {
                button.SetActive(false);
            }
            menuActive = false;
        }

        //if (Input.GetMouseButtonUp(1))
        //{
        //    isRightClicking = false;
        //    rightClickStartTime = Time.time;
        //    foreach (MenuButton button in buttons)
        //    {
        //        button.SetActive(false);
        //    }
        //    menuActive = false;
        //}

        if (isRightClicking && !menuActive && Time.time - rightClickStartTime >= longPressDuration)
        {
            menuActive = true;
            targetMousePosition = Input.mousePosition;
            targetMousePosition.z = Camera.main.nearClipPlane;

            // ���㰴ť��Ŀ��λ�ã�����ʼ����ťλ��
            for (int i = 0; i < numberOfButtons; i++)
            {
                Vector3 offset = new Vector3(intervalX, 20 * (i - 1.5f) * radius, 0);
                if (i == 0 || i == numberOfButtons - 1)
                {
                    offset.x = offset.x - littleOffset;
                }
                targetButtonPositions[i] = targetMousePosition + offset;
                buttons[i].SetPosition(Input.mousePosition);
                initialButtonPositions[i] = Input.mousePosition;
            }

            // ��������
            animationStartTime = Time.time;
        }

        if (isRightClicking && menuActive)
        {
            // ���°�ť��λ�ã�ͨ����ֵʵ�ֽ��䶯��
            float progress = (Time.time - animationStartTime) / animationDuration;
            for (int i = 0; i < numberOfButtons; i++)
            {
                buttons[i].SetActive(true);
                buttons[i].SetPosition(Vector3.Lerp(initialButtonPositions[i], targetButtonPositions[i], progress));

                // ��ȡ�������Ļ�ϵ�λ��
                Vector2 mousePosition = Input.mousePosition;
               
                // ����UIԪ�ص���б�Ƕ�
                float tiltX = Mathf.Clamp((mousePosition.y - buttons[i].GetPosition().y) / buttons[i].GetPosition().y, -1f, 1f) * tiltAmount;
                float tiltY = Mathf.Clamp((mousePosition.x - buttons[i].GetPosition().x) / buttons[i].GetPosition().x, -1f, 1f) * tiltAmount;

                // ������б�Ƕȵ���ת
                Quaternion tiltRotation = Quaternion.Euler(tiltX, tiltY, 0f);

                // Ӧ����б��ת��UIԪ��
                buttons[i].SetRotation(Quaternion.Slerp(buttons[i].GetRotation(), tiltRotation, Time.deltaTime * tiltSpeed));
            }


        }
    }
}
