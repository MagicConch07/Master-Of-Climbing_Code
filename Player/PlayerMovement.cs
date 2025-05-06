using UnityEngine;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using System.Collections;

public enum HandType
{
    Left, Right
};

public class PlayerMovement : MonoBehaviour, IPlayerComponent
{
    private Player _player;
    private InputReaderSO _inputReader;

    [Header("Hand")]
    [SerializeField] private Transform _leftHandTarget;
    [SerializeField] private Transform _rightHandTarget;
    [SerializeField] private Transform _leftArm;
    [SerializeField] private Transform _rightArm;
    private bool _isCanLeftHandMove;
    private bool _isCanRightHandMove;

    [Header("Foot")]
    [SerializeField] private Transform _leftFootTarget;
    [SerializeField] private Transform _rightFootTarget;
    private bool _isCanLeftFootMove;
    private bool _isCanRightFootMove;

    [Header("Hip")]
    [SerializeField] private Transform _hipLeft;
    [SerializeField] private Transform _hipRight;

    private bool _isMoveLeftHand;
    private bool _isMoveRightHand;
    private bool _isMoveLeftFoot;
    private bool _isMoveRightFoot;
    private bool _isBodyMove;

    [Header("Rigid")]
    private Rigidbody _myRigid;

    public bool IsDie => _isDie;
    private bool _isDie = false;

    [Header("Speeds")]
    [SerializeField] private float _handSpeed = 7f;
    [SerializeField] private float _footSpeed = 5f;
    [SerializeField] private float _footXSpeed = 1f;
    [SerializeField] private float _footYSpeed = 0.3f;
    [SerializeField] private float _hipSpeed = 0.7f;

    [field: SerializeField] public float SpeedMultiplier { get; set; } = 1f;
    [field: SerializeField] public float DurationMultiplier { get; set; } = 1f;

    private Vector3 targetPos;
    private Sequence _mySequence;
    private bool _isCanMoveBody = false;
    private bool _isCanBodyMove = false;

    private bool _isLeftHandNotHold;
    private bool _isRightHandNotHold;
    private bool _isLeftFootNotHold;
    private bool _isRightFootNotHold;

    public bool IsPercentLoosening { get; set; } = false;

    [Header("Percent")]
    [field: SerializeField] public float PercentLoosening { get; set; } = 0.1f;

    [Header("Cam")]
    [SerializeField] private CinemachineCamera _followCam;
    [SerializeField] private CinemachineCamera _lookCam;
    [SerializeField] private CinemachineImpulseSource _impulseSource;

    [SerializeField] private Volume _volume;
    private Vignette _vignette;

    [SerializeField] private float _vignetteDur = 2.0f;
    private float _time = 0;

    public void Initialize(Player player)
    {
        _myRigid = GetComponent<Rigidbody>();
        _myRigid.useGravity = false;
        _isDie = false;
        _player = player;
        _inputReader = _player.GetCompo<InputReaderSO>();

        _volume.profile.TryGet(out _vignette);

        _inputReader.OnLeftHandEvent += HandleLeftHand;
        _inputReader.OnRightHandEvent += HandleRightHand;
        _inputReader.OnLeftFootEvent += HandleLeftFoot;
        _inputReader.OnRightFootEvent += HandleRightFoot;
        _inputReader.OnBodyUpEvent += HandleBodyUp;
    }

    private void OnDestroy()
    {
        _inputReader.OnLeftHandEvent -= HandleLeftHand;
        _inputReader.OnRightHandEvent -= HandleRightHand;
        _inputReader.OnLeftFootEvent -= HandleLeftFoot;
        _inputReader.OnRightFootEvent -= HandleRightFoot;
        _inputReader.OnBodyUpEvent -= HandleBodyUp;
    }

    private void HandleLeftHand(bool isPress)
    {
        _isCanLeftHandMove = isPress;
        _isMoveLeftHand = !isPress;

        if (_isLeftHandNotHold) return;
        _isLeftHandNotHold = isPress;
    }

    private void HandleRightHand(bool isPress)
    {
        _isCanRightHandMove = isPress;
        _isMoveRightHand = !isPress;

        if (_isRightHandNotHold == true) return;
        _isRightHandNotHold = isPress;
    }

    private void HandleLeftFoot(bool isPress)
    {
        _isCanLeftFootMove = isPress;
        _isMoveLeftFoot = !isPress;

        if (_isLeftFootNotHold == true) return;
        _isLeftFootNotHold = isPress;
    }

    private void HandleRightFoot(bool isPress)
    {
        _isCanRightFootMove = isPress;
        _isMoveRightFoot = !isPress;

        if (_isRightFootNotHold == true) return;
        _isRightFootNotHold = isPress;
    }

    private void HandleBodyUp(bool isPress)
    {
        _isCanMoveBody = isPress;
    }

    private void Update()
    {
        if (IsPercentLoosening)
        {
            float rand = Random.Range(0f, 100f);

            if (rand < PercentLoosening)
            {
                int randType = Random.Range(0, 2);

                if (randType == 0)
                {
                    HandLoosening(HandType.Left);
                }
                else if (randType == 1)
                {
                    HandLoosening(HandType.Right);
                }
            }
        }

        if ((_isCanLeftHandMove && _isCanRightHandMove) ||
            (transform.localEulerAngles.z > 60f && transform.localEulerAngles.z < 300f))
        {
            Die();
        }
        if (_isDie) return;
        //* Hand
        Vector3 mousePos = _inputReader.GetMouseToWorldPosition();

        if (_isCanLeftHandMove)
        {
            Vector3 dir = mousePos - _leftArm.position;
            dir.Normalize();

            targetPos = _leftArm.position + dir * 1f;
            _leftHandTarget.position = Vector3.Lerp(_leftHandTarget.position, targetPos, Time.deltaTime * _handSpeed * SpeedMultiplier);
        }
        if (_isCanRightHandMove)
        {
            Vector3 dir = mousePos - _rightArm.position;
            dir.Normalize();

            targetPos = _rightArm.position + dir * 1f;
            _rightHandTarget.position = Vector3
            .Lerp(_rightHandTarget.position, targetPos, Time.deltaTime * _handSpeed * SpeedMultiplier);
        }

        //* Foot
        if (_isCanLeftFootMove)
        {
            Vector3 dir = mousePos - _hipLeft.position;
            dir.Normalize();

            targetPos = new Vector3(
                    _hipLeft.position.x + dir.x * _footXSpeed,
                    _hipLeft.position.y + dir.y * _footYSpeed,
                    _leftFootTarget.position.z);

            _leftFootTarget.position = Vector3.Lerp(_leftFootTarget.position, targetPos, Time.deltaTime * _footSpeed * SpeedMultiplier);
        }

        if (_isCanRightFootMove)
        {
            Vector3 dir = mousePos - _hipRight.position;
            dir.Normalize();

            targetPos = new Vector3(
                    _hipRight.position.x + dir.x * _footXSpeed,
                    _hipRight.position.y + dir.y * _footYSpeed,
                    _rightFootTarget.position.z);

            _rightFootTarget.position = Vector3.Lerp(_rightFootTarget.position, targetPos, Time.deltaTime * _footSpeed * SpeedMultiplier);
        }

        if (_isLeftHandNotHold && _isRightHandNotHold &&
            _isLeftFootNotHold && _isRightFootNotHold)
        {
            _isCanBodyMove = true;
        }

        //* Up Body
        if (_isMoveRightFoot && _isMoveLeftFoot &&
            _isMoveLeftHand && _isMoveRightHand &&
            _isCanMoveBody && _isCanBodyMove &&
            _isBodyMove == false)
        {
            _isBodyMove = true;
            //* Hand Dir

            _isLeftHandNotHold = false;
            _isRightHandNotHold = false;
            _isLeftFootNotHold = false;
            _isRightFootNotHold = false;
            _isCanBodyMove = false;

            Vector3 footDir = Vector3.Lerp(_leftFootTarget.position, _rightFootTarget.position, 0.5f);
            Vector3 targetRotDir = ((_leftHandTarget.position - transform.position) + (_rightHandTarget.position - transform.position)).normalized;
            Quaternion targetRot = Quaternion.FromToRotation(targetRotDir, Vector3.up);
            targetRot.x = 0;
            targetRot.y = 0;

            _mySequence = DOTween.Sequence()
                .Append(transform.DORotateQuaternion(Quaternion.Euler(0, 0, -targetRot.eulerAngles.z), _hipSpeed * DurationMultiplier))
                .Join(transform.DOMove(new Vector3(footDir.x, footDir.y, transform.position.z), _hipSpeed * DurationMultiplier))
                .OnComplete(() =>
                {
                    _isBodyMove = false;
                });
        }
    }

    public void MoveStop(bool enable)
    {
        _inputReader.KeyStop(enable);
    }

    private void Die()
    {
        if (_isDie) return;
        _isDie = true;
        _myRigid.useGravity = true;

        _followCam.Priority = 0;
        _lookCam.transform.position = _followCam.transform.position;
        _lookCam.Priority = 100;

        _isCanLeftHandMove = true;
        _isCanRightHandMove = true;
        _isCanLeftFootMove = true;
        _isCanRightFootMove = true;

        StartCoroutine(DieVi());
    }

    private IEnumerator DieVi()
    {
        float start = _vignette.intensity.value;
        while (_time < _vignetteDur)
        {
            _vignette.intensity.value = Mathf.Lerp(start, 1f, _time / _vignetteDur);

            _time += Time.deltaTime;
            yield return null;
        }
        SceneManager.LoadScene(0);
    }

    public void CreateImpulse()
    {
        _impulseSource.GenerateImpulse();
    }

    public void HandLoosening(HandType type)
    {
        if (type == HandType.Left)
        {
            _isCanLeftHandMove = true;
        }
        else if (type == HandType.Right)
        {
            _isCanRightHandMove = true;
        }
    }
}
