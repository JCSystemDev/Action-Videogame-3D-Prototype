using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Character : MonoBehaviour
{
    public VisualEffect summonFX;
    public AudioSource summonSFX;
    public AudioSource attackSFX;
    public AudioSource hurtSFX;
    public AudioSource deathSFX;
    public AudioSource slideSFX;
    public AudioSource coinSFX;
    public AudioSource healSFX;
    private CharacterController _cc;
    public float moveSpeed = 5f;
    private Vector3 _movementVelocity;
    private PlayerInput _playerInput;
    private float _verticalVelocity;
    public float gravity = -9.8f;
    private Animator _animator;
    public int coinsAmount;
    public bool isInvincible;
    public float invincibleDuration = 2f;
    public float slideSpeed = 9f;
    private float _attackAnimationDuration;
    
    
    public bool isPlayer = true;
    private UnityEngine.AI.NavMeshAgent _navMeshAgent;
    private Transform _targetPlayer;

    private float _attackStartTime;
    public float attackSlideDuration = 0.4f;
    public float attackSlideSpeed = 0.06f;

    private Vector3 _impactOnCharacter;

    private Health _health;
    private DamageCaster _damageCaster;

    public enum CharacterState
    {
        Normal, Attacking, Dead, BeingHit, Slide, Spawn
    }

    public CharacterState currentState;

    public float spawnDuration;
    private float _currentSpawnTime;

    private MaterialPropertyBlock _materialPropertyBlock;
    private SkinnedMeshRenderer _skinnedMeshRenderer;

    public GameObject itemToDrop;

    private void Awake()
    {
        summonFX.Play();
        summonSFX.Play();
        _cc = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _health = GetComponent<Health>();
        _damageCaster = GetComponentInChildren<DamageCaster>();

        _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        _materialPropertyBlock = new MaterialPropertyBlock();
        _skinnedMeshRenderer.GetPropertyBlock(_materialPropertyBlock);
            

        if (!isPlayer)
        {
            _navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            _targetPlayer = GameObject.FindWithTag("Player").transform;
            _navMeshAgent.speed = moveSpeed;
            SwitchStateTo(CharacterState.Spawn);
        }
        else
        {
            _playerInput = GetComponent<PlayerInput>();
        }
    }

    private void CalculatePlayerMovement()
    {
        if (_playerInput.attackButton && _cc.isGrounded)
        {
            SwitchStateTo(CharacterState.Attacking);
            return;
        }
        else if (_playerInput.slideButton && _cc.isGrounded)
        {
            SwitchStateTo(CharacterState.Slide);
            return;
        }
        
        _movementVelocity.Set(_playerInput.horizontalInput, 0f, _playerInput.verticalInput );
        _movementVelocity.Normalize();
        _movementVelocity = Quaternion.Euler(0, -45f, 0) * _movementVelocity;
        
        _animator.SetFloat("Speed", _movementVelocity.magnitude);
        _movementVelocity *= moveSpeed * Time.deltaTime;

        if (_movementVelocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(_movementVelocity);
        }
        
        _animator.SetBool("AirBorne", !_cc.isGrounded);
        
    }

    private void CalculateEnemyMovement()
    {
        if (Vector3.Distance(_targetPlayer.position, transform.position) >= _navMeshAgent.stoppingDistance)
        {
            _navMeshAgent.SetDestination(_targetPlayer.position);
            _animator.SetFloat("Speed", 0.2f);
        }
        else
        {
            _navMeshAgent.SetDestination(transform.position);
            _animator.SetFloat("Speed", 0f);
            
            SwitchStateTo(CharacterState.Attacking);
        }
    }

    private void FixedUpdate()
    {
        switch (currentState)
        {
            case CharacterState.Normal:
                if (isPlayer)
                {
                    CalculatePlayerMovement();
                }
                else
                {
                    CalculateEnemyMovement();
                }

                break;

            case CharacterState.Attacking:
                if (isPlayer)
                {
                    if (Time.time < _attackStartTime + attackSlideDuration)
                    {
                        float timePassed = Time.time - _attackStartTime;
                        float lerpTIme = timePassed / attackSlideDuration;
                        _movementVelocity = Vector3.Lerp(transform.forward * attackSlideSpeed, Vector3.zero, lerpTIme);
                    }

                    if (_playerInput.attackButton && _cc.isGrounded)
                    {
                        
                        string currentClipName = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
                       
                        _attackAnimationDuration = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

                        if (currentClipName != "LittleAdventurerAndie_ATTACK_03" && _attackAnimationDuration > 0.5f && attackSlideDuration < 0.7f)
                        {
                            _playerInput.attackButton = false;
                            SwitchStateTo(CharacterState.Attacking);
                            
                            
                            CalculatePlayerMovement();
                        }
                    }
                }
                break;
            case CharacterState.Dead:
                return;
            
            case CharacterState.BeingHit:
                if (_impactOnCharacter.magnitude > 0.2f)
                {
                    _movementVelocity = _impactOnCharacter * Time.deltaTime;
                }

                _impactOnCharacter = Vector3.Lerp(_impactOnCharacter, Vector3.zero, Time.deltaTime * 5);
                
                break;
            
            case CharacterState.Slide:

                _movementVelocity = transform.forward * slideSpeed * Time.deltaTime;
                break;
            
            case CharacterState.Spawn:
                _currentSpawnTime -= Time.deltaTime;
                if (_currentSpawnTime <= 0)
                {
                    SwitchStateTo(CharacterState.Normal);
                }
                break;
        }
        
        if (isPlayer)
        {
            if (_cc.isGrounded == false)
            {
                _verticalVelocity = gravity;
            }
            else
            {
                _verticalVelocity = gravity * 0.3f;
            }

            _movementVelocity += Vector3.up * (_verticalVelocity * Time.deltaTime);
        
            _cc.Move(_movementVelocity);
            _movementVelocity = Vector3.zero;
        }
    }

    public void SwitchStateTo(CharacterState newState)
    {
        if (isPlayer)
        {
            _playerInput.ClearCache();
        }

        switch (currentState)
        {
            case CharacterState.Normal:
                break;
            case CharacterState.Attacking:

                if(_damageCaster != null)
                {
                    _damageCaster.DisableDamageCaster();
                }

                if (isPlayer)
                {
                    
                    GetComponent<PlayerVFXManager>().StopBlade();
                }
                
                break;
            case CharacterState.Dead:
                return;
            case CharacterState.BeingHit:
                break;
            case CharacterState.Slide:
                break;
            case CharacterState.Spawn:
                isInvincible = false;
                break;
                
        }

        switch (newState)
        {
            case CharacterState.Normal:
                break;
            case CharacterState.Attacking:

                if (!isPlayer)
                {
                    attackSFX.PlayDelayed(1.7f);
                    Quaternion newRotation = Quaternion.LookRotation(_targetPlayer.position - transform.position);
                    transform.rotation = newRotation;
                }
                
                _animator.SetTrigger("Attack");
                if (isPlayer)
                {
                    attackSFX.Play();
                    _attackStartTime = Time.time;
                }
                break;
            case CharacterState.Dead:
                deathSFX.Play();
                _cc.enabled = false;
                _animator.SetTrigger("Dead");
                StartCoroutine(MaterialDissolve());
                break;
            case CharacterState.BeingHit:
                _animator.SetTrigger("BeingHit");
                if (isPlayer)
                {
                    hurtSFX.Play();
                    isInvincible = true;
                    StartCoroutine(DelayCancelInvincible());
                }
                break;
            
            case CharacterState.Slide:
                slideSFX.Play();
                _animator.SetTrigger("Slide");
                break;
            
            case CharacterState.Spawn:
                isInvincible = true;
                _currentSpawnTime = spawnDuration;
                StartCoroutine(MaterialAppear());
                break;
        }

        currentState = newState;
        
        
    }

    public void SlideAnimationEnds()
    {
        SwitchStateTo(CharacterState.Normal);
    }

    public void AttackAnimationEnds()
    {
        SwitchStateTo(CharacterState.Normal);
    }

    public void BeingHitAnimationEnds()
    {
        SwitchStateTo(CharacterState.Normal);
    }
    
    public void ApplyDamage(int damage, Vector3 attackerPos = new Vector3())
    {
        if (isInvincible)
        {
            return;
        }
        
        if (_health != null)
        {
            _health.ApplyDamage(damage);
        }

        if (!isPlayer)
        {
            GetComponent<EnemyVFXManager>().PlayBeingHitVFX(attackerPos);
            hurtSFX.Play();
        }

        StartCoroutine(MaterialBlink());

        if (isPlayer)
        {
            SwitchStateTo(CharacterState.BeingHit);
            AddImpact(attackerPos, 10f);
        }
    }

    IEnumerator DelayCancelInvincible()
    {
        yield return new WaitForSeconds(invincibleDuration);
        isInvincible = false;
        BeingHitAnimationEnds();
    }

    private void AddImpact(Vector3 attackerPos, float force)
    {
        Vector3 impactDir = transform.position - attackerPos;
        impactDir.Normalize();
        impactDir.y = 0;
        _impactOnCharacter = impactDir * force;
    }
    

    public void EnableDamageCaster()
    {
        _damageCaster.EnableDamageCaster();
    }

    public void DisableDamageCaster()
    {
        _damageCaster.DisableDamageCaster();
    }

    IEnumerator MaterialBlink()
    {
        _materialPropertyBlock.SetFloat("_blink", 0.4f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);

        yield return new WaitForSeconds(0.2f);
        
        _materialPropertyBlock.SetFloat("_blink", 0f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);

    }

    IEnumerator MaterialDissolve()
    {
        yield return new WaitForSeconds(2f);

        float dissolveTimeDuration = 2f;
        float currentDissolveTime = 0;
        float dissolveHeight_start = 20f;
        float dissolveHeight_target = -10f;
        float dissolveHeight;
        
        _materialPropertyBlock.SetFloat("_enableDissolve", 1f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);

        while (currentDissolveTime < dissolveTimeDuration)
        {
            currentDissolveTime += Time.deltaTime;
            dissolveHeight = Mathf.Lerp(dissolveHeight_start, dissolveHeight_target,
                currentDissolveTime / dissolveTimeDuration);
            _materialPropertyBlock.SetFloat("_dissolve_height", dissolveHeight);
            _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
            yield return null;
        }
        
        DropItem();
        
    }

    public void DropItem()
    {
        if (itemToDrop != null)
        {
            Instantiate(itemToDrop, transform.position, Quaternion.identity);
        }
    }

    public void PickUpItem(PickUp item)
    {
        switch (item.type)
        {
            case PickUp.PickUpType.Heal:
                healSFX.Play();
                AddHealth(item.value);
                break;
            case PickUp.PickUpType.Coin:
                coinSFX.Play();
                AddCoin(item.value);
                break;
        }
        
    }

    private void AddHealth(int health)
    {
        _health.AddHealth(health);
        GetComponent<PlayerVFXManager>().PlayHealVFX();
    }

    private void AddCoin(int coin)
    {
        coinsAmount += coin;
    }

    public void RotateToTarget()
    {
        if (currentState != CharacterState.Dead)
        {
            transform.LookAt(_targetPlayer, Vector3.up);
        }
    }

    IEnumerator MaterialAppear()
    {
        float dissolveTimeDuration = spawnDuration;
        float currentDissolveTime = 0;
        float dissolveHeight_start = -10f;
        float dissolveHeight_target = 20f;
        float dissolveHeight;
        
        _materialPropertyBlock.SetFloat("_enableDissolve", 1f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);

        while (currentDissolveTime < dissolveTimeDuration)
        {
            currentDissolveTime += Time.deltaTime;
            dissolveHeight = Mathf.Lerp(dissolveHeight_start, dissolveHeight_target,
                currentDissolveTime / dissolveTimeDuration);
            _materialPropertyBlock.SetFloat("_dissolve_height", dissolveHeight);
            _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
            yield return null;

        }
        
        _materialPropertyBlock.SetFloat("_enableDissolve", 1f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
    }
    

}
