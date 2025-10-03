using UnityEngine;

public enum GameState
{
    Initializing,
    Ready,
    Playing,
    Processing,
    Paused,
    GameOver,
    Victory
}

public enum CardState
{
    FaceDown,
    FaceUp,
    Matched
}

public abstract class BaseGameState
{
    protected GameManager gameManager;

    public BaseGameState(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
    public virtual void OnCardFlipped(Card card) { }
}

public class PlayingState : BaseGameState
{
    public PlayingState(GameManager gameManager) : base(gameManager) { }

    public override void OnCardFlipped(Card card)
    {
        if (gameManager.CanFlipCard(card))
        {
            gameManager.AddFlippedCard(card);
        }
    }
}

public class ProcessingState : BaseGameState
{
    public ProcessingState(GameManager gameManager) : base(gameManager) { }

    public override void OnCardFlipped(Card card)
    {
        
    }
}
