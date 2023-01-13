using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [SerializeField] private List<DungeonLevelSO> dungeonLevelList;

    [SerializeField] private int currentDungeonLevelListIndex = 0;

    [SerializeField] public GameState gameState;
    public int dungeonLevel = 0;

    void Start()
    {
        gameState = GameState.gameStarted;
    }

    void Update()
    {
        HandleGameState();

        if (Input.GetKeyDown(KeyCode.R))
        {
            gameState = GameState.gameStarted;
        }
    }

    private void HandleGameState()
    {
        switch (gameState)
        {
            case GameState.gameStarted:
                //Play first level
                PlayDungeonLevel(currentDungeonLevelListIndex);
                gameState = GameState.playingLevel;
                break;

            default:
                break;
        }
    }

    private void PlayDungeonLevel(int index)
    {
        bool dungeonBuiltSuccessfully = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[currentDungeonLevelListIndex]);
    }

    #region Validation
#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(dungeonLevelList), dungeonLevelList);
    }
#endif
    #endregion Validation
}
