using Google.Protobuf.Protocol;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttackState : IPlayerState
{
    PlayerController playerController;
    public CurrentPlayerState playerState = CurrentPlayerState.AttackState;

    public PlayerAttackState(PlayerController player)
    {
        this.playerController = player;
    }

    public void Enter()
    {
        playerController.isDamaged = false;
        string attack = "Attack";
        if (playerController.GetComponentInChildren<Warrior>())
        {
            int a = Random.Range(1, 4);
            attack += (a > 1 ? a : "");
        }

        playerController.animator.SetTrigger(attack);

        YHSMyPlayerController mpc = playerController as YHSMyPlayerController;
        if (mpc != null)
            mpc.SendPlayerMovePacket();
    }

    public void Execute()
    {
        // 점프 공격 시 위치 동기화 불가 문제 해결을 위해 추가
        // 근데 이게 맞나 싶음
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
