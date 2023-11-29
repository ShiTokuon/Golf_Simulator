using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    LineRenderer line;

    // ������͂̑傫��
    [SerializeField]
    float forceMagnitude = 10f;

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
    bool isBoostPressed = false;

    // �������蒆�t���O
    bool isCheckingDistance = false;

    // �{�^�����������Ԃ��ǂ����̃t���O
    bool canButtonPress = true;

    // �h���b�O�J�n�t���O
    bool isDragging = false;

    // �{�[���̃I�u�W�F�N�g��~�ʒu�i�[�p�x�N�g��
    Vector3 stopPosition = Vector3.zero;

    // �p�x�̏���Ɖ����̒�`
    const float MaxAngle = 180f;
    const float MinAngle = 0f;

    // �͂����������
    Vector3 forceDirection = new Vector3(1.0f, 1.0f, 0f);

    // �h���b�O�ő�t�^�͗�
    [SerializeField]
    float MaxMagnitude = 2f;

    // �^�͗ʏ���l
    [SerializeField]
    const float FixForce = 10f;

    // ������͂̑傫��
    //public float forceToAdd = 10f;

    // �{�[���𔭎˂��钼�O�̈ʒu
    Vector3 prePosition = Vector3.zero;

    // ���˕����̗�
    Vector3 currentForce = Vector3.zero;

    // ���C���J����
    private Camera mainCamera = null;

    // ���C���J�������W
    private Transform mainCameraPos = null;

    // �h���b�N�J�n�_
    private Vector3 dragStart = Vector3.zero;

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

        // �}�E�X���N���b�N������
        if (Input.GetMouseButtonDown(0))
        {
            DragStart();
        }

        // �}�E�X���h���b�O��
        if (Input.GetMouseButton(0))
        {
            Drag();
        }

        // �}�E�X�N���b�N������������
        if (Input.GetMouseButtonUp(0))
        {
            DragEnd();
        }

        // forceAngle�̕ύX�𔽉f����
        CalcForceDirection();
    }

    void FixedUpdate()
    {
        // �����̑���
        CheckDistance();

        // �S�[������
        CheckSphereState();

        if (!isBoostPressed)
        {
            // �L�[�܂��̓{�^����������Ă��Ȃ����
            // �����̐؂�ւ�������������
            return;
        }

        // Boost�{�^���������ꂽ�ꍇ�Ɉȉ��̏������s��
        canButtonPress = false;
        boostButton.interactable = canButtonPress;

        // �{�[���̔��ˏ���
        BoostSphere();

        // ��s���t���O�̐؂�ւ�
        isFlying = true;

        // �ǂ���̏��������Ă��{�^�������t���O��false��
        isBoostPressed = false;
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

        //// �����Ɨ͂̌v�Z
        Vector3 force = forceMagnitude * forceDirection;

        //// �͂������郁�\�b�h
        rb.AddForce(force, ForceMode.Impulse);

        // �������蒆��True�ɃZ�b�g
        isCheckingDistance = true;

        // �p�x�����\���ɂ���
        //angleArrowObject.SetActive(false);

        // �{�^���������Ȃ��悤�ɂ���
        //SetAngleButtonState(false);

        // �K�C�h���\���ɂ���
        //guideManager.SetGuidesState(false);
    }

    public void OnPressedBoostButton()
    {
        isBoostPressed = true;
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
        if (Input.GetKeyUp(KeyCode.B) && canButtonPress)
        {
            isBoostPressed = true;
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
        canButtonPress = true;
        boostButton.interactable = canButtonPress;
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

    private void OnMouseDown()
    {
        dragStart = GetMousePosition();

        line.enabled = true;
        isDragging = true;
        line.SetPosition(0, rb.position);
        line.SetPosition(1, rb.position);

    }

    private void OnMouseDrag()
    {
            var position = GetMousePosition();

            currentForce = position - dragStart;
            if (currentForce.magnitude > MaxMagnitude * MaxMagnitude)
            {
                currentForce *= MaxMagnitude / currentForce.magnitude;
            }

            line.SetPosition(0, rb.position);
            line.SetPosition(1, rb.position + currentForce);
    }

    public void OnMouseUp()
    {
        if (isDragging)
        {
            line.enabled = false;
            isDragging = false;
            Flip(currentForce * 2);
        }
    }



    public void Flip(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
    }

    void DragStart()
    {

    }

    void Drag()
    {

    }

    void DragEnd()
    {

    }

}
