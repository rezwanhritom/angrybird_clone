using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlingShotHandler : MonoBehaviour
{
    [Header("Line Renderers")]
    [SerializeField] private LineRenderer _leftLineRenderer;
    [SerializeField] private LineRenderer _rightLineRenderer;
    
    [Header("Transform References")]
    [SerializeField] private Transform _leftStartPosition;
    [SerializeField] private Transform _rightStartPosition;
    [SerializeField] private Transform _centerPosition;
    [SerializeField] private Transform _idlePosition;
    
    [Header("Slingshot Stats")]
    [SerializeField] private float _maxDistance = 3.5f;
    [SerializeField] private float _shotForce = 5f;
    [SerializeField] private float _timeBetweenBirdRespawns = 2f;
    
    [Header("Scripts")]
    [SerializeField] private SlingShotArea _slingshotArea;
    
    [Header("Bird")]
    [SerializeField] private AngryBird _angryBirdPrefab;
    [SerializeField] private float _angryBirdPositionOffset = 2f;
    

    private Vector2 _slingShotLinesPosition;
    
    private Vector2 _direction;
    private Vector2 _directionNormalized;
    
    private bool _clickedWithinArea;
    private bool _birdOnSlingshot;

    private AngryBird _spawnedAngryBird;

    private void Awake()
    {
        _leftLineRenderer.enabled = false;
        _rightLineRenderer.enabled = false;
        
        SpawnAngryBird();
    }
    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && _slingshotArea.IsWithSlingshotArea())
        {
            _clickedWithinArea = true;
        }
        
        if (Mouse.current.leftButton.isPressed && _clickedWithinArea && _birdOnSlingshot)
        {
            DrawSlingShot();
            PositionAndRotateAngryBird();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && _birdOnSlingshot)
        {
            if (GameManager.instance.HasEnoughShots())
            {
                _clickedWithinArea = false;

                _spawnedAngryBird.LaunchBird(_direction, _shotForce);
                
                GameManager.instance.UseShot();
                
                _birdOnSlingshot = false;
            
                SetLines(_centerPosition.position);

                if (GameManager.instance.HasEnoughShots())
                {
                    StartCoroutine(SpawnAngryBirdAfterTime());
                }
            }
        }
    }

    #region Slingshot Methods
    
    private void DrawSlingShot()
    {
        Vector3 touchPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        
        _slingShotLinesPosition = _centerPosition.position + Vector3.ClampMagnitude(touchPosition - _centerPosition.position, _maxDistance);
        
        SetLines(_slingShotLinesPosition);
        
        _direction = (Vector2)_centerPosition.position - _slingShotLinesPosition;
        _directionNormalized = _direction.normalized;
    }

    private void SetLines(Vector2 position)
    {
        if (!_leftLineRenderer.enabled && !_rightLineRenderer.enabled)
        {
            _leftLineRenderer.enabled = true;
            _rightLineRenderer.enabled = true;
        }
        
        _leftLineRenderer.SetPosition(0, position);
        _leftLineRenderer.SetPosition(1, _leftStartPosition.position);
        
        _rightLineRenderer.SetPosition(0, position);
        _rightLineRenderer.SetPosition(1, _rightStartPosition.position);
    }
    
    #endregion

    #region Angry Bird methods

    private void SpawnAngryBird()
    {
        SetLines(_idlePosition.position);
        
        Vector2 dir = (_centerPosition.position - _idlePosition.position).normalized;
        Vector2 spawnPosition = (Vector2)_idlePosition.position + dir * _angryBirdPositionOffset;
        
        _spawnedAngryBird = Instantiate(_angryBirdPrefab, spawnPosition, Quaternion.identity);
        _spawnedAngryBird.transform.right = dir;
        
        _birdOnSlingshot = true;
    }

    private void PositionAndRotateAngryBird()
    {
        _spawnedAngryBird.transform.position = _slingShotLinesPosition + _directionNormalized * _angryBirdPositionOffset;
        
        _spawnedAngryBird.transform.right = _directionNormalized;
    }

    private IEnumerator SpawnAngryBirdAfterTime()
    {
        yield return new WaitForSeconds(_timeBetweenBirdRespawns);
        
        SpawnAngryBird();
    }
    
    #endregion
}
