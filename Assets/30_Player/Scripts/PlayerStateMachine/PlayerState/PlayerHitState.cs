using System.Collections;
using UnityEngine;

public class PlayerHitState : IPlayerState
{
    PlayerController playerController;
    public CurrentPlayerState playerState = CurrentPlayerState.HitState;

    float duration = 2f;    // 흐트러진 상태 지속시간
    bool isPlaying = false;    // 1회성 코루틴용 bool

    public PlayerHitState(PlayerController pc)
    {
        playerController = pc;
    }

    public void Enter()
    {
        playerController.animator.SetTrigger("Hit");

        YHSMyPlayerController mpc = playerController as YHSMyPlayerController;
        if (mpc != null)
        {
            mpc.SendPlayerMovePacket();
        }
    }

    public void Execute()
    {
        if (playerController.isDead)
        {
            playerController.OnDead();
            isPlaying = false;
            playerController.StopCoroutine(DurationDamaged());
        }
        if (!isPlaying)
        {
            isPlaying = true;
            playerController.StartCoroutine(DurationDamaged());
        }
    }

    public void Exit()
    {
        playerController.StopCoroutine(DurationDamaged());
    }

    /// <summary>
    /// 데미지를 입었을 때 지속시간동안 애니메이션이 지속되도록 하는 코루틴
    /// 지속시간이 끝나면 데미지를 받지 않은 애니메이션이 되도록 한다.
    /// </summary>
    IEnumerator DurationDamaged()
    {
        yield return new WaitForSeconds(duration);
        playerController.isDamaged = false;
        isPlaying = false;
        yield break;
    }

    public CurrentPlayerState ReturnNowState()
    {
        return playerState;
    }
}
