using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    #region Singleton
    public static CardManager instance;
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

    [SerializeField] protected List<Card> m_drawDeck = new List<Card>();
    public List<Card> cardsInDrawDeck
    {
        get { return m_drawDeck; }
    }

    [SerializeField] protected List<Card> m_cardsInPlay = new List<Card>();
    public List<Card> cardsInPlay
    {
        get { return m_cardsInPlay; }
    }

    [SerializeField] protected List<Card> m_discardPile = new List<Card>();
    public List<Card> cardsInDiscardPile
    {
        get { return m_discardPile; }
    }

    public int numberOfCards { get { return m_drawDeck.Count + m_cardsInPlay.Count + m_discardPile.Count; } }

    public void AddCardToDeck(Card newCard)
    {
        if (m_drawDeck == null)
            m_drawDeck = new List<Card>();

        m_drawDeck.Add(newCard);
    }

    public void MoveCardsIntoPlay(Card[] cards)
    {
        if (m_drawDeck == null || m_drawDeck.Count == 0)
            return;

        foreach (Card card in cards)
        {
            m_drawDeck.Remove(card);
            m_cardsInPlay.Add(card);
        }
    }

    public void MoveCardsToDiscard()
    {
        m_discardPile.AddRange(m_cardsInPlay);
        m_cardsInPlay.Clear();
    }

    public bool EnoughCardsToDeal(int numPlayers)
    {
        return m_drawDeck.Count >= numPlayers;
    }

    public void ReshuffleDiscard(bool useCardsInPlay = true)
    {
        if (useCardsInPlay)
        {
            MoveCardsToDiscard();   
        }

        m_drawDeck.AddRange(m_discardPile);
        m_discardPile.Clear();

        if (!useCardsInPlay)
        {
            m_discardPile.AddRange(cardsInPlay);
        }
        
        m_cardsInPlay.Clear();
    }

    public void Restart()
    {
        foreach (Card card in m_cardsInPlay)
        {
            Destroy(card.gameObject);
        }

        foreach (Card card in m_drawDeck)
        {
            Destroy(card.gameObject);
        }

        foreach (Card card in m_discardPile)
        {
            Destroy(card.gameObject);
        }

        m_drawDeck.Clear();
        m_cardsInPlay.Clear();
        m_discardPile.Clear();
    }
}
