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

    // �S�[���pUI�e�L�X�g�I�u�W�F�N�g�ւ̎Q��
    [SerializeField]
    GameObject goalTextObject;

    // �p�[�e�B�N���I�u�W�F�N�g�ւ̎Q��   
    public GameObject particleObject;

    // ���˕���
    [SerializeField]
    LineRenderer line = null;

    [Range(0.1f, 2.0f)]
    public float speed = 2.0f;

    // �X�N���v�g�̎Q��
    public static SphereBooster instance;

    // ������͂̑傫��
    // �h���b�O�ő�t�^�͗�
    private const float MaxMagnitude = 5f;

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

    // �p�[�e�B�N���ւ̎Q��
    ParticleSystem particle;

    // ��s���t���O
    public bool isFlying = true;

    // �������蒆�t���O
    public bool isCheckingDistance = false;

    // �{�^�����������Ԃ��ǂ����̃t���O
    //bool canButtonPress = true;

    // �h���b�O�J�n�t���O
    public bool isDragStart = false;

    // �h���b�O�t���O
    bool isDragging = false;

    // �h���b�O�J�n�ł����Ԃ��ǂ����̃t���O
    public bool isDraggChecking = true;

    // �S�[���ڐG�t���O
    bool isTouchingGoal = false;

    // �S�[���ς݃t���O
    bool hasReachedGoal = false;

    // �{�[���̃I�u�W�F�N�g��~�ʒu�i�[�p�x�N�g��
    Vector3 stopPosition = Vector3.zero;

    // �p�x�̏���Ɖ����̒�`
    const float MaxAngle = 180f;
    const float MinAngle = 0f;

    // �͂����������
    Vector3 forceDirection = new Vector3(1.0f, 1.0f, 0f);

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

    // �S�[���̃^�O
    string goalTag = "Finish";

    // �n�ʂ̃^�O
    string Ground = "Ground";

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

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        Physics.autoSimulation = false;
    }

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        distanceText = distanceTextObject.GetComponent<Text>();
        highScoreText = highScoreTextObject.GetComponent<Text>();
        mainCamera = Camera.main;
        mainCameraPos = mainCamera.transform;
        currentPosition = rb.position;
        DeltaTime = Time.fixedDeltaTime;

        particle = particleObject.GetComponent<ParticleSystem>();

        // DistanceText��HighScoreText�̏����l���Z�b�g
        SetDistanceText(0f);
        SetHighScoreText(0f);
    }

    void Update()
    {
        // �}�E�X���쏈��
        MouseDraggs();

        // forceAngle�̕ύX�𔽉f����
        CalcForceDirection();
    }

    void FixedUpdate()
    {
        Physics.Simulate(Time.fixedDeltaTime * speed);

        // �����̑���
        CheckDistance();

        // �S�[������
        CheckSphereState();

        if (!isDragging)
        {
            // �L�[�܂��̓{�^����������Ă��Ȃ����
            // �����̐؂�ւ�������������
            return;
        }

        // �{�[���̈ʒu���L�^
        SphereStorage();

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

    void Stop_rb()
    {
        // �^���̒�~
        rb.velocity = Vector3.zero;

        //  �d�͂̒�~
        rb.isKinematic = true;
    }

    void SphereStorage()
    {
        // �{�[�������˂���鎞�̈ʒu���L�^
        prePosition = transform.position;

        // �������蒆��True�ɃZ�b�g
        isCheckingDistance = true;
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

        if (other.gameObject.tag == goalTag)
        {
            // ���肪�S�[�����������̏���
            isTouchingGoal = true;

            // �{�[���̉^���������I�ɒ�~
            Stop_rb();
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == Ground)
        {
            Instantiate(particle, this.transform.position, Quaternion.identity);
        }
    }

    void CheckSphereState()
    {

        if (!rb.IsSleeping() || hasReachedGoal)
        {
            return;
        }

        if (isTouchingGoal)
        {
            // �S�[���Ŏ~�܂������̏���
            StartCoroutine(GoalAnimation(0.3f));
            hasReachedGoal = true;
        }
        else
        {
            // �S�[���ȊO�Ŏ~�܂������̏���
            ReadyToBoost();
        }
    }

    IEnumerator GoalAnimation(float time)
    {
        // �S�[�����̃A�j���[�V��������
        RectTransform rt = goalTextObject.GetComponent<RectTransform>();

        // �S�[���e�L�X�g�I�u�W�F�N�g�̏����ʒu�ƈړ���ʒu
        Vector3 initTextPos = rt.localPosition;
        Vector3 targetPos = Vector3.zero;

        // �����œn���ꂽ�b���𑫂�������(���ݎ����ɑ���)
        float finishTime = Time.time + time;

        while (true)
        {
            // ���������̎����ɒB�������̊m�F
            float diff = finishTime - Time.time;
            if (diff <= 0)
            {
                break;
            }
            // Lerp���v�Z���邽�߂Ɏ��Ԑi�s�x���v�Z
            float rate = 1 - Mathf.Clamp01(diff / time);

            // �����ʒu����ړ���ʒu�܂ł̒�����ŁA�����ɉ������ʒu���Z�b�g
            rt.localPosition = Vector3.Lerp(initTextPos, targetPos, rate);

            // 1�t���[���ҋ@
            yield return null;
        }
        // �ړ���ʒu���Z�b�g
        rt.localPosition = targetPos;
    }

    void ReadyToBoost()
    {
        // ��s���t���O��False�ɃZ�b�g
        isFlying = false;

        // �������蒆�t���O��False�ɃZ�b�g
        isCheckingDistance = false;

        // ���݂̋��������Z�b�g
        SetDistanceText(0f);

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
        line.SetPosition(0, currentPosition);
        line.SetPosition(1, currentPosition);

    }

    void Drag()
    {
        currentPosition = rb.position;
        line.enabled = true;

        var position = GetMousePosition();

        currentForce = position - dragStart;
        if (currentForce.magnitude > MaxMagnitude)
        {
            currentForce *= MaxMagnitude / currentForce.magnitude;
        }

        line.SetPosition(0, currentPosition);
        line.SetPosition(1, currentPosition + currentForce);
    }

    void DragEnd()
    {
        line.enabled = false;
        isDraggChecking = false;
        isDragStart = false;
        Flip(currentForce * 3f);
    }

    void MouseDraggs()
    {
        // �}�E�X���h���b�O��
        if (Input.GetMouseButton(0) && isDraggChecking)
        {
            if (isDragStart == false)
            {
                isDragStart = true;
                DragStart();
            }
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