using UnityEngine;

public class PlayerIdleState : IPlayerState
{
    PlayerController playerController;
    public CurrentPlayerState playerState = CurrentPlayerState.IdleState;

    public PlayerIdleState(PlayerController player)
    {
        this.playerController = player;
    }

    public void Enter()
    {
        playerController.animator.SetTrigger("Idle");
        
        YHSMyPlayerController mpc = playerController as YHSMyPlayerController;
        if (mpc != null)
        {
            mpc.SendPlayerMovePacket();

            // 자동 회복 코루틴 시작
            mpc.playerInformation.StartAutoHeal();
        }
    }

    public void Execute()
    {
        
    }

    public void Exit()
    {
        YHSMyPlayerController mpc = playerController as YHSMyPlayerController;
        if (mpc != null)
        {
            // 자동 회복 코루틴 종료
            mpc.playerInformation.StopAutoHeal();
        }
    }

    public CurrentPlayerState ReturnNowState()
    {
        return playerState;
    }
}
