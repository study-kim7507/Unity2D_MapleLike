using Google.Protobuf.Protocol;
using UnityEngine;

public class PlayerJumpState : IPlayerState
{
    PlayerController playerController;
    public CurrentPlayerState playerState = CurrentPlayerState.JumpState;

    public PlayerJumpState(PlayerController player)
    {
        this.playerController = player;
    }

    public void Enter()
    {
        playerController.animator.SetTrigger("Jump");
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
