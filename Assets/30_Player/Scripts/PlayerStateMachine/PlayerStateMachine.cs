using Google.Protobuf.Protocol;
using System;
using UnityEngine;

public enum CurrentPlayerState
{
    IdleState = 0,
    MoveState = 1,
    JumpState = 2,
    HitState = 3,
    AttackState = 4,
    // BuffSkill = 5,
    // DebuffSkill = 6,
    // Hanging = 7,
    DeadState = 8,
}

public class PlayerStateMachine 
{
    public IPlayerState CurrentState { get; private set; }
    public CurrentPlayerState NowState { get; private set; }

    PlayerController player;

    public PlayerIdleState idleState;
    public PlayerMoveState moveState;
    public PlayerJumpState jumpState;
    public PlayerHitState hitState;
    public PlayerAttackState attackState;
    public PlayerDeadState deadState;

    public event Action<IPlayerState> stateChanged;

    public PlayerStateMachine(PlayerController player)
    {
        this.player = player;
        idleState = new PlayerIdleState(player);
        moveState = new PlayerMoveState(player);
        jumpState = new PlayerJumpState(player);
        hitState = new PlayerHitState(player);
        attackState = new PlayerAttackState(player);
        deadState = new PlayerDeadState(player);
    }

    public void Initialize(IPlayerState state)
    {
        CurrentState = state;
    }

    public void TransitionTo(IPlayerState nextState)
    {
        if (CurrentState != nextState)
        {
            CurrentState?.Exit();
            CurrentState = nextState;
            NowState = CurrentState.ReturnNowState();
            CurrentState?.Enter();

            stateChanged?.Invoke(CurrentState);
        }
    }

    public void TransitionByEnum(PlayerState nextState)
    {
        switch (nextState)
        {
            case PlayerState.PIdle:
                TransitionTo(idleState);
                break;
            case PlayerState.PMoving:
                TransitionTo(moveState);
                break;
            case PlayerState.PJump:
                TransitionTo(jumpState);
                break;
            case PlayerState.PStun:
                TransitionTo(hitState);
                break;
            case PlayerState.PAttackskill:
                TransitionTo(attackState);
                break;
            case PlayerState.PDead:
                TransitionTo(deadState);
                break;
            default:
                TransitionTo(idleState);
                break;
        };
    }

    public void Execute()
    {
        CurrentState?.Execute();
    }
}
