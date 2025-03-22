using UnityEngine;

public class PlayerMoveState : IPlayerState
{
    PlayerController playerController;
    public CurrentPlayerState playerState = CurrentPlayerState.MoveState;

    public PlayerMoveState(PlayerController player)
    {
        this.playerController = player;
    }

    public void Enter()
    {
        playerController.animator.SetTrigger("Move");
    }

    public void Execute()
    {
        YHSMyPlayerController mpc = playerController as YHSMyPlayerController;
        if (mpc != null)
            mpc.SendPlayerMovePacket();
    }

    public void Exit()
    {

    }

    public CurrentPlayerState ReturnNowState()
    {
        return playerState;
    }
}
