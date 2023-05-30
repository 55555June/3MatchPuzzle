using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Wait,
    Continue
}

//게임상태조작 클래스
public class GameStateController                                     
{
    private GameState gameState = GameState.Wait;

    public GameState GetGameState()
    {
        return gameState;
    }

    public void ChangeGameState(GameState _state)
    {
        gameState = _state;
    }

    public bool CompareGameState(GameState _state)
    {
        if(gameState == _state)
        {
            return true;
        }
        return false;
    }
}
