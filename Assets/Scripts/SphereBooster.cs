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

    // ������͂̑傫��
    [SerializeField]
    float forceMagnitude = 10.0f;

    // X������̊p�x(90�܂Őݒ�)
    [SerializeField, Range(0f, 90f)]
    float forceAngle = 45.0f;

    // �͂����������
    Vector3 forceDirection = new Vector3(1.0f, 1.0f, 0f);

    // ��s���t���O
    bool isFlying = false;

    // �{�^�������t���O
    bool isBoostPressed = false;

    // �������蒆�t���O
    bool isCheckingDistance = false;

    // �{�[���̃I�u�W�F�N�g��~�ʒu�i�[�p�x�N�g��
    Vector3 stopPosition = Vector3.zero;

    // Sphere�I�u�W�F�N�g�̏����ʒu�i�[�p�x�N�g��
    Vector3 initPosition = Vector3.zero;

    // Rigidbody�R���|�[�l���g�ւ̎Q�Ƃ��L���b�V��
    Rigidbody rb;

    // DistanceText�I�u�W�F�N�g��Text�R���|�[�l���g�ւ̎Q�Ƃ��L���b�V��
    Text distanceText;

    // UI�e�L�X�g�̃v���t�B�b�N�X
    string distancePrefix = "�򋗗�: ";

    // UI�e�L�X�g�̃T�t�B�b�N�X
    string distanceSuffix = " m";

    // HighScoereText�I�u�W�F�N�g��Text�R���|�[�l���g�ւ̎Q�Ƃ��L���b�V��
    Text highScoreText;

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
        initPosition = gameObject.transform.position;
        rb = gameObject.GetComponent<Rigidbody>();
        distanceText = distanceTextObject.GetComponent<Text>();
        highScoreText = highScoreTextObject.GetComponent<Text>();

        // DistanceText��HighScoreText�̏����l���Z�b�g
        SetDistanceText(0f);
        SetHighScoreText(0f);
    }

    void Update()
    {
        // Input.GetKeyUp�̓L�[����x�����ꂽ��A���ꂪ�����ꂽ����True��Ԃ�
        if (Input.GetKeyUp(KeyCode.B))
        {
            isBoostPressed = true;
        }

        // forceAngle�̕ύX�𔽉f����
        CalcForceDirection();
    }

    void FixedUpdate()
    {
        CheckDistance();

        if (!isBoostPressed)
        {
            // �L�[�܂��̓{�^����������Ă��Ȃ����
            // �����̐؂�ւ�������������
            return;
        }
        if (isFlying)
        {
            // ��s���̏���
            StopFlying();
        }
        else
        {
            // �{�[�����΂�����
            BoostSphere();
        }

        // ��s���t���O�̐؂�ւ�
        isFlying = !isFlying;

        // �ǂ���̏��������Ă��{�^�������t���O��false��
        isBoostPressed = false;
    }

    void StopFlying()
    {
        // �^���̒�~
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // �����ʒu�Ɉړ�������
        gameObject.transform.position = initPosition;

        // �������蒆��False�ɃZ�b�g
        isCheckingDistance = false;

        // ���݂̋��������Z�b�g
        SetDistanceText(0f);
    }

    void BoostSphere()
    {
        // �����Ɨ͂̌v�Z
        Vector3 force = forceMagnitude * forceDirection;

        // �͂������郁�\�b�h
        rb.AddForce(force, ForceMode.Impulse);

        // �������蒆��True�ɃZ�b�g
        isCheckingDistance = true;
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
        float distance = GetDistanceInXZ(initPosition, currentPosition);

        // UI�ɕ\��
        SetDistanceText(distance);

        if (rb.IsSleeping())
        {
            // �n�C�X�R�A�̃`�F�b�N
            stopPosition = currentPosition;
            float currentDistance = GetDistanceInXZ(initPosition, stopPosition);

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
            // �{�[������������p�̃I�u�W�F�N�g���������̏���
            isBoostPressed = true;
        }
    }
}
