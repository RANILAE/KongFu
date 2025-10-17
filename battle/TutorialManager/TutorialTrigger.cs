using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial Manager")]
    public TutorialManager tutorialManager;

    [Header("Trigger Settings")]
    public KeyCode triggerKey = KeyCode.T; // �����̵̳İ���
    public bool useUIButton = false; // �Ƿ�ʹ��UI��ť����
    public string triggerButtonName = "TutorialButton"; // UI��ť���ƣ����ʹ��UI������

    void Start()
    {
        // ���û��ָ��tutorialManager�������Զ�����
        if (tutorialManager == null)
        {
            tutorialManager = FindObjectOfType<TutorialManager>();
        }
    }

    void Update()
    {
        // ���������̳�
        if (Input.GetKeyDown(triggerKey))
        {
            TriggerTutorial();
        }
    }

    // �������������Ա�UI��ť����
    public void TriggerTutorial()
    {
        if (tutorialManager != null)
        {
            // ���¿�ʼ�̳�
            tutorialManager.SetCurrentStep(0);

            // ȷ���̳���弤��
            if (tutorialManager.tutorialPanel != null)
            {
                tutorialManager.tutorialPanel.SetActive(true);
            }

            Debug.Log("�̳������´�����");
        }
        else
        {
            Debug.LogError("δ�ҵ�TutorialManager��");
        }
    }

    // ���ò���ʼ�̳̣���ѡ�ĸ����׵����÷�����
    public void ResetAndStartTutorial()
    {
        if (tutorialManager != null)
        {
            // ��������״̬
            tutorialManager.SetCurrentStep(0);

            // ���¿�ʼ�̳�
            tutorialManager.StartTutorial();

            Debug.Log("�̳������ò����¿�ʼ��");
        }
    }

    // ���ʹ����ײ��������ѡ��
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerTutorial();
        }
    }

    // ���ʹ��3D��ײ��������ѡ��
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerTutorial();
        }
    }
}