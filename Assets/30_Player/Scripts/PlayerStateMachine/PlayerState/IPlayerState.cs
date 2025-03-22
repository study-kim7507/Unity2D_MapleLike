using UnityEngine;

public interface IPlayerState
{
    public void Enter();
    public void Execute();
    public void Exit();
    public CurrentPlayerState ReturnNowState();
}
