using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using AYellowpaper.SerializedCollections;
using System.Data;

public class DeckBuilder : MonoBehaviour
{
    #region Singleton
    public static DeckBuilder instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        if (gameObject.scene.buildIndex != -1)
            DontDestroyOnLoad(this);
    }
    #endregion Singleton

    // Could be made dynamic with a count on the Card enums,
    // if we wanted to add custom suits or cards
    private readonly int numSuits = 4;
    private readonly int numCards = 13;

    [SerializeField] private GameObject cardPrefab = null;

    [SerializeField] protected SerializedDictionary<Card.Suit, float> m_suitWeights;
    [SerializeField] protected SerializedDictionary<Card.CardValue, float> m_singleCardWeights;

    private int m_numberOfDecks = 0;
    public int numberOfDecks { get { return m_numberOfDecks; } }

    public void CreateDecks(int numberOfDecks, Transform dealDeck)
    {
        float weight = 1.0f;
        float oldWeight = 1.0f;

        Card newCard = null;
        GameObject newGO = null;

        m_numberOfDecks = numberOfDecks;

        for (int deck = 0; deck < numberOfDecks; deck++)
        {
            string deckName = new string("D" + (deck + 1) + "_");
        
            for (int suit = 0; suit < numSuits; suit++)
            {
                weight = 1.0f;
        
                foreach (var suitWeight in m_suitWeights)
                {
                    if (suit == (int)(suitWeight.Key))
                    {
                        weight = suitWeight.Value;
                    }
                }
        
                string suitName = new string(((Card.Suit)suit).ToString() + "s");
        
                for (int cardVal = 0; cardVal < numCards; cardVal++)
                {
                    newGO = (Instantiate(cardPrefab, dealDeck));
                    newCard = newGO.GetComponent<Card>();
        
                    newGO.name = deckName + ((Card.CardName)cardVal).ToString() + "_Of_" + suitName;
        
                    if (newCard != null)
                    {
                        oldWeight = weight;
        
                        foreach (var singleCardWeight in m_singleCardWeights)
                        {
                            if ((Card.Suit)suit == singleCardWeight.Key.m_suit &&
                                (Card.CardName)cardVal == singleCardWeight.Key.m_cardName)
                            {
                                weight = singleCardWeight.Value;
                            }
                        }
        
                        newCard.InitCard((Card.Suit)suit, (Card.CardName)cardVal, weight);
                        CardManager.instance.AddCardToDeck(newCard);
                        weight = oldWeight;
                    }
                }
            }
        }
    }

    public void Restart()
    {
        m_numberOfDecks = 0;
    }
}
