using UnityEngine;

[CreateAssetMenu(fileName = "New Card Data", menuName = "Memory Game/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Card Properties")]
    public int cardID;
    public string cardName;
    public Sprite cardFaceSprite;
    public Sprite cardBackSprite;
    
    [Header("Audio")]
    public AudioClip flipSound;
    public AudioClip matchSound;
    public AudioClip wrongMatchSound;
    
    [Header("Effects")]
    public GameObject matchEffect;
    public Color cardColor = Color.white;
    
    private int _hashCode = -1;
    
    public override int GetHashCode()
    {
        if (_hashCode == -1)
            _hashCode = cardID.GetHashCode();
        return _hashCode;
    }
    
    public override bool Equals(object other)
    {
        if (other is CardData otherCard)
            return cardID == otherCard.cardID;
        return false;
    }
}
