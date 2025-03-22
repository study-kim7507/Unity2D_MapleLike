using System;
using System.Collections;
using UnityEngine;

public class PlayerDeadState : IPlayerState
{
    PlayerController playerController;
    public CurrentPlayerState playerState = CurrentPlayerState.DeadState;
    
    GameObject character;       // 플레이어가 사망했을 때 스프라이트를 끄기 위한 게임오브젝트

    public PlayerDeadState(PlayerController pc)
    {
        playerController = pc;

        character = pc.transform.GetChild(0).gameObject;
    }

    public void Enter()
    {
        character.GetComponent<SpriteRenderer>().enabled = false;
        
        SpawnManager.Instance.SpawnAsset(ConstList.RIP, playerController.transform);   // 비석 소환
        SoundManager.Instance.PlaySoundOneShot(ConstList.Death);

        YHSMyPlayerController mpc = playerController as YHSMyPlayerController;
        if (mpc != null)
        {
            mpc.SendPlayerMovePacket();
            mpc.SendPlayerDiePacket();
            DeathManager.Instance.ActiveDeathPopup();
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
            // 사망 후 부활 시의 후처리
            playerController.gameObject.GetComponent<BoxCollider2D>().enabled = true;
            character.GetComponent<SpriteRenderer>().enabled = true;

            DeathManager.Instance.StopCount();
        }
    }

    public CurrentPlayerState ReturnNowState()
    {
        return playerState;
    }
}
