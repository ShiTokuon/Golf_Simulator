using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class SphereBooster : MonoBehaviour
{
    // DistanceText�I�u�W�F�N�g�ւ̎Q��
    [SerializeField]
    GameObject distanceTextObject;

    // HighScoreText�I�u�W�F�N�g�ւ̎Q��
    [SerializeField]
    GameObject highScoreTextObject;

    // ���˃{�^���I�u�W�F�N�g�ւ̎Q��
    [SerializeField]
    GameObject boostButtonObject;

    // ���˕���
    [SerializeField]
    LineRenderer line = null;

    // ���ˋO��
    [SerializeField]
    LineRenderer GuideLine = null;

    // ������͂̑傫��
    // �h���b�O�ő�t�^�͗�
    private const float MaxMagnitude = 2f;

    // X������̊p�x(90�܂Őݒ�)
    [SerializeField, Range(0f, 90f)]
    float forceAngle = 45.0f;

    // Rigidbody�R���|�[�l���g�ւ̎Q�Ƃ��L���b�V��
    Rigidbody rb;

    // DistanceText�I�u�W�F�N�g��Text�R���|�[�l���g�ւ̎Q�Ƃ��L���b�V��
    Text distanceText;

    // HighScoereText�I�u�W�F�N�g��Text�R���|�[�l���g�ւ̎Q�Ƃ��L���b�V��
    Text highScoreText;

    // ���˃{�^���I�u�W�F�N�g�ւ̎Q��
    Button boostButton;

    // ��s���t���O
    bool isFlying = false;

    // �{�^�������t���O
    //bool isBoostPressed = false;

    // �������蒆�t���O
    bool isCheckingDistance = false;

    // �{�^�����������Ԃ��ǂ����̃t���O
    //bool canButtonPress = true;

    // �h���b�O�J�n�t���O
    bool isDragging = false;

    // �h���b�O�J�n�ł����Ԃ��ǂ����̃t���O
    bool isDraggChecking = true;

    // �{�[���̃I�u�W�F�N�g��~�ʒu�i�[�p�x�N�g��
    Vector3 stopPosition = Vector3.zero;

    // �p�x�̏���Ɖ����̒�`
    const float MaxAngle = 180f;
    const float MinAngle = 0f;

    // �͂����������
    Vector3 forceDirection = new Vector3(1.0f, 1.0f, 0f);

    // ������͂̑傫��
    //public float forceToAdd = 10f;

    // �{�[���𔭎˂��钼�O�̈ʒu
    Vector3 prePosition = Vector3.zero;

    // ���˕����̗�
    Vector3 currentForce = Vector3.zero;

    // �{�[���ʒu
    Vector3 currentPosition = Vector3.zero;

    // ���C���J����
    private Camera mainCamera = null;

    // ���C���J�������W
    private Transform mainCameraPos = null;

    // �h���b�N�J�n�_
    private Vector3 dragStart = Vector3.zero;

    // �Œ�t���[���E�F�C�g
    private static float DeltaTime;

    // �Œ�t���[���҂�����
    private static readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    // UI�e�L�X�g�̃v���t�B�b�N�X
    string distancePrefix = "�򋗗�: ";

    // UI�e�L�X�g�̃T�t�B�b�N�X
    string distanceSuffix = " m";

    // �n�C�X�R�A�\���̃v���t�B�b�N�X
    string highScorePrefix = "�n�C�X�R�A: ";

    // �n�C�X�R�A�\���̃T�t�B�b�N�X
    string highScoreSuffix = " m";

    // �n�C�X�R�A�̋���
    float highScoreDistance = 0f;

    // ��������p�̃^�O
    string fallCheckerTag = "FallChecker";


    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        distanceText = distanceTextObject.GetComponent<Text>();
        highScoreText = highScoreTextObject.GetComponent<Text>();
        mainCamera = Camera.main;
        mainCameraPos = mainCamera.transform;
        currentPosition = rb.position;
        DeltaTime = Time.fixedDeltaTime;

        // boost�{�^���̃R���|�[�l���g�ւ̎Q�Ƃ�ǉ�
        boostButton = boostButtonObject.GetComponent<Button>();

        // DistanceText��HighScoreText�̏����l���Z�b�g
        SetDistanceText(0f);
        SetHighScoreText(0f);
    }

    void Update()
    {
        // �L�[�{�[�h����̓��͂��Ď�
        CheckInput();

        // �}�E�X���쏈��
        MouseDragges();

        // forceAngle�̕ύX�𔽉f����
        CalcForceDirection();

        Debug.Log("idDragging�t���O��" + isDragging);
        //Debug.Log("isBoostPressed�t���O��"+isBoostPressed);
        Debug.Log("isDraggingChecking�t���O��" + isDraggChecking);
    }

    void FixedUpdate()
    {
        // �����̑���
        CheckDistance();

        // �S�[������
        CheckSphereState();

        //if (!isBoostPressed)
        //{
        //    // �L�[�܂��̓{�^����������Ă��Ȃ����
        //    // �����̐؂�ւ�������������
        //    return;
        //}

        if (!isDragging)
        {
            // �L�[�܂��̓{�^����������Ă��Ȃ����
            // �����̐؂�ւ�������������
            return;
        }

        // �{�[���̔��ˏ���
        BoostSphere();

        // ��s���t���O�̐؂�ւ�
        isFlying = true;

        // �ǂ���̏��������Ă��{�^�������t���O��false��
        // �}�E�X�p
        isDragging = false;

    }

    void StopFlying()
    {
        // �^���̒�~
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // �{�[�����ˑO�̈ʒu�Ɉړ�������
        gameObject.transform.position = prePosition;

        // �{�[����~��̏������Ă�
        ReadyToBoost();
    }

    void BoostSphere()
    {
        // �{�[�������˂���鎞�̈ʒu���L�^
        prePosition = transform.position;

        // �������蒆��True�ɃZ�b�g
        isCheckingDistance = true;

        // �p�x�����\���ɂ���
        //angleArrowObject.SetActive(false);

        // �{�^���������Ȃ��悤�ɂ���
        //SetAngleButtonState(false);

        // �K�C�h���\���ɂ���
        //guideManager.SetGuidesState(false);
    }

    void CalcForceDirection()
    {
        // ���͂��ꂽ�p�x�����W�A���ɕϊ�
        float rad = forceAngle * Mathf.Deg2Rad;

        // ���ꂼ��̎��̐������v�Z
        float x = Mathf.Cos(rad);
        float y = Mathf.Sin(rad);
        float z = 0f;

        // Vector3�^�Ɋi�[
        forceDirection = new Vector3(x, y, z);
    }

    void CheckDistance()
    {
        if (!isCheckingDistance)
        {
            // �������蒆�Ȃ牽�����Ȃ�
            return;
        }

        // ���݋����܂ł̋������v�Z����
        Vector3 currentPosition = gameObject.transform.position;
        float distance = GetDistanceInXZ(prePosition, currentPosition);

        // UI�ɕ\��
        SetDistanceText(distance);

        if (rb.IsSleeping())
        {
            // �n�C�X�R�A�̃`�F�b�N
            stopPosition = currentPosition;
            float currentDistance = GetDistanceInXZ(prePosition, stopPosition);

            if (currentDistance > highScoreDistance)
            {
                highScoreDistance = currentDistance;
            }
            SetHighScoreText(highScoreDistance);

            // �������蒆���I�t�ɂ���
            isCheckingDistance = false;
        }
    }
    float GetDistanceInXZ(Vector3 startPos, Vector3 stopPos)
    {
        // �J�n�ʒu�A��~�ʒu���ꂼ��AY����������Vector3�𐧍�
        Vector3 startPosCalc = new Vector3(startPos.x, 0f, startPos.z);
        Vector3 stopPosCalc = new Vector3(stopPos.x, 0f, stopPos.z);

        // 2��Vector3���狗�����Z�o
        float distance = Vector3.Distance(startPosCalc, stopPosCalc);
        return distance;
    }

    void SetDistanceText(float distance)
    {
        // �󂯎���������̒l���g���ĉ�ʂɕ\������e�L�X�g���Z�b�g
        distanceText.text = distancePrefix + distance.ToString("F2") + distanceSuffix;
    }

    void SetHighScoreText(float distance)
    {
        // �󂯎�����n�C�X�R�A�̒l���g���ĉ�ʂɕ\������e�L�X�g���Z�b�g
        highScoreText.text = highScorePrefix + distance.ToString("F2") + highScoreSuffix;
    }
    void OnTriggerEnter(Collider other)
    {
        // �Փ˂�������̃^�O���m�F����
        if (other.gameObject.tag == fallCheckerTag)
        {
            // ���肪��������p�I�u�W�F�N�g���������̏���
            StopFlying();
        }
    }

    void CheckInput()
    {
        // �L�[�������ꂽ���̓������`

        // Input.GetKeyUp�̓L�[����x�����ꂽ��A���ꂪ�����ꂽ����True��Ԃ�
        //if (Input.GetKeyUp(KeyCode.B) && canButtonPress)
        //{
        //    //isBoostPressed = true;
        //}

        if (Input.GetKeyUp(KeyCode.B) && isDragging)
        {
            isDragging = true;
        }

        // ��L�[�������ꂽ���́A�p�x�ύX�{�^��(��)�������ꂽ���̏������Ă�
        //if (Input.GetKeyDown(KeyCode.UpArrow))
        //{
        //    OnPressedAngleUpButton();
        //}
        //else if (Input.GetKeyUp(KeyCode.UpArrow))
        //{
        //    OnReleasedAngleButton();
        //}

        //// ���L�[�������ꂽ���́A�p�x�ύX�{�^��(��)�������ꂽ���̏������Ă�
        //if (Input.GetKeyDown(KeyCode.DownArrow))
        //{
        //    OnPressedAngleDownButton();
        //}
        //else if (Input.GetKeyUp(KeyCode.DownArrow))
        //{
        //    OnReleasedAngleButton();
        //}
    }


    //void SetAngleButtonState(bool isInteractable)
    //{
    //    // �{�^���������邩�ǂ����̏�Ԃ��Z�b�g����
    //    angleUpButton.interactable = isInteractable;
    //    angleDownButton.interactable = isInteractable;
    //}

    void CheckSphereState()
    {
        // �S�[��������������R�����g����

        //if (!rb.IsSleeping() || hasReachedGoal)
        //{
        //    return;
        //}
        if (!rb.IsSleeping())
        {
            return;
        }
        //if (isTouchingGoal)
        //{
        //    // �S�[���Ŏ~�܂������̏���
        //    StartCoroutine(GoalAnimation(0.8f));
        //    hasReachedGoal = true;
        //}
        else
        {
            // �S�[���ȊO�Ŏ~�܂������̏���
            ReadyToBoost();
        }
    }

    void ReadyToBoost()
    {
        // ��s���t���O��False�ɃZ�b�g
        isFlying = false;

        // �������蒆�t���O��False�ɃZ�b�g
        isCheckingDistance = false;

        // ���݂̋��������Z�b�g
        SetDistanceText(0f);

        // �p�x����\������
        //angleArrowObject.SetActive(true);

        // �{�^����������悤�ɂ���
        //SetAngleButtonState(true);

        // �K�C�h��\������
        //guideManager.SetGuidesState(true);

        // �{�[���̉^����Ԃ��m�F
        //canButtonPress = true;
        //boostButton.interactable = canButtonPress;

        isDragging = true;
        isDraggChecking = true;
    }

    Vector3 GetMousePosition()
    {
        // �}�E�X���W���擾
        var position = Input.mousePosition;
        position.z = mainCameraPos.position.z;
        position = mainCamera.ScreenToWorldPoint(position);
        position.z = 0;

        return position;
    }

    void DragStart()
    {
        dragStart = GetMousePosition();
        currentPosition = rb.position;

        line.enabled = true;
        GuideLine.enabled = true;
        line.SetPosition(0, currentPosition);
        line.SetPosition(1, currentPosition);

    }

    void Drag()
    {
        var position = GetMousePosition();

        currentForce = position - dragStart;
        if (currentForce.magnitude > MaxMagnitude * MaxMagnitude)
        {
            currentForce *= MaxMagnitude / currentForce.magnitude;
        }

        line.SetPosition(0, currentPosition);
        line.SetPosition(1, currentPosition + currentForce);

        this.StartCoroutine(this.Guide());
    }
    IEnumerator Guide()
    {
        line.enabled = false;
        
        Physics.autoSimulation = false;

        var points = new List<Vector3> { currentPosition };
        Flip(currentForce * 6);

        // �O�Ղ��V�~���[�V�������ċL�^����
        for (var i = 0; i < 20; i++)
        {
            Physics.Simulate(DeltaTime * 2.5f);
            points.Add(rb.position);
        }

        // ���̈ʒu�ɖ߂�
        rb.velocity = Vector3.zero;
        this.transform.position = currentPosition;

        // �\���n�_���q���ŋO�Ղ�`��
        GuideLine.positionCount = points.Count;
        GuideLine.SetPositions(points.ToArray());

        Physics.autoSimulation = true;
        line.enabled = true;

        yield return waitForFixedUpdate;
    }

    void DragEnd()
    {
        line.enabled = false;
        GuideLine.enabled = false;
        isDraggChecking = false;
        Flip(currentForce * 6f);
    }

    void MouseDragges()
    {
        // �}�E�X���N���b�N������
        if (Input.GetMouseButtonDown(0) && isDraggChecking)
        {
            DragStart();
        }

        // �}�E�X���h���b�O��
        if (Input.GetMouseButton(0) && isDraggChecking)
        {
            Drag();
        }

        // �}�E�X�N���b�N������������
        if (Input.GetMouseButtonUp(0) && isDraggChecking)
        {
            DragEnd();
        }
    }

    public void Flip(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
    }

}