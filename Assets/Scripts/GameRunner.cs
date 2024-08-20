using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TMPro;
using System;
using UnityEngine.UI;

public class GameRunner : MonoBehaviour
{
    [SerializeField] protected int m_defaultDecks = 1;
    [SerializeField] protected int m_defaultPlayers = 2;

    [SerializeField] protected GameObject m_playerPrefab;

    [SerializeField] protected Transform m_drawDeckTrans = null;
    [SerializeField] protected Transform m_discardDeckTrans = null;
    [SerializeField] protected Transform m_playerContainer = null;

    [SerializeField] protected Button m_dealButton;
    [SerializeField] protected Button m_restartButton;

    [SerializeField] protected Button m_addDeckButton;
    [SerializeField] protected Button m_maxDeckButton;
    [SerializeField] protected Button m_removeDeckButton;

    [SerializeField] protected Button m_addPlayerButton;
    [SerializeField] protected Button m_maxPlayerButton;
    [SerializeField] protected Button m_removePlayerButton;

    [SerializeField] protected Toggle m_usePlayerCardToggle;

    protected CardManager m_cardManager;
    protected DeckBuilder m_deckBuilder;

    protected List<Player> m_playerList;

    [SerializeField] protected float m_dealAnimTime = 1f;

    [SerializeField] protected TextMeshProUGUI m_drawText;
    [SerializeField] protected TextMeshProUGUI m_discardText;

    protected AudioSource m_soundSource;
    [SerializeField] protected AudioClip m_flipClip;
    [SerializeField] protected AudioClip m_shuffleClip;

    protected float m_flipClipPitch = 1f;
    protected float m_shuffleClipPitch = 2f;

    public float flipTime { get { return (m_flipClip != null) ? m_flipClip.length / m_flipClipPitch : 0f; } }
    public float shuffleTime { get { return (m_shuffleClip != null) ? m_shuffleClip.length / m_shuffleClipPitch : 0f; } }

    protected Coroutine m_animationTimingRoutine = null;
    protected Coroutine m_shuffleAnimationRoutine = null;

    protected bool m_cardsDelt = false;
    protected bool m_showReshuffle = true;

    public void Awake()
    {
        m_cardManager = CardManager.instance;
        m_deckBuilder = DeckBuilder.instance;

        if (m_dealButton == null) m_dealButton = GameObject.FindGameObjectWithTag("DealButton").GetComponent<Button>();
        if (m_restartButton == null) m_restartButton = GameObject.FindGameObjectWithTag("RestartButton").GetComponent<Button>();

        if (m_addDeckButton == null) m_addDeckButton = GameObject.FindGameObjectWithTag("AddDeck").GetComponent<Button>();
        if (m_maxDeckButton == null) m_maxDeckButton = GameObject.FindGameObjectWithTag("MaxDecks").GetComponent<Button>();
        if (m_removeDeckButton == null) m_removeDeckButton = GameObject.FindGameObjectWithTag("RemoveDeck").GetComponent<Button>();

        if (m_addPlayerButton == null) m_addPlayerButton = GameObject.FindGameObjectWithTag("AddPlayer").GetComponent<Button>();
        if (m_maxPlayerButton == null) m_maxPlayerButton = GameObject.FindGameObjectWithTag("MaxPlayers").GetComponent<Button>();
        if (m_removePlayerButton == null) m_removePlayerButton = GameObject.FindGameObjectWithTag("RemovePlayer").GetComponent<Button>();

        if (m_drawText == null) m_drawText = m_drawDeckTrans.parent.GetComponentInChildren<TextMeshProUGUI>();
        if (m_discardText == null) m_discardText = m_discardDeckTrans.parent.GetComponentInChildren<TextMeshProUGUI>();

        if (m_usePlayerCardToggle == null) m_usePlayerCardToggle = GameObject.FindGameObjectWithTag("UsePlayerCards").GetComponent<Toggle>();

        if (m_soundSource == null) m_soundSource = GetComponent<AudioSource>();

        m_dealButton.onClick.AddListener(DealClicked);
        m_restartButton.onClick.AddListener(RestartClicked);

        m_addDeckButton.onClick.AddListener(AddDeck);
        m_maxDeckButton.onClick.AddListener(MaxDeck);
        m_removeDeckButton.onClick.AddListener(RemoveDeck);

        m_addPlayerButton.onClick.AddListener(AddPlayer);
        m_maxPlayerButton.onClick.AddListener(MaxPlayer);
        m_removePlayerButton.onClick.AddListener(RemovePlayer);

        ResetGame(m_defaultDecks, m_defaultPlayers);
    }

    #region Button Clicks
    private void DealClicked()
    {
        m_cardsDelt = true;
        m_showReshuffle = true;
        if (CardManager.instance.EnoughCardsToDeal(m_playerList.Count))
        {
            //ClearText();
            m_cardManager.MoveCardsToDiscard();

            var cards = WeightedRandomDraw.DealCards(m_playerList.Count);
            m_cardManager.MoveCardsIntoPlay(cards);

            for (int index = 0; index < m_playerList.Count; index++)
            {
                m_playerList[index].DealCard(cards[index], m_discardDeckTrans, m_dealAnimTime, flipTime);
            }

            m_animationTimingRoutine = StartCoroutine(DealAnimationRoutine(m_dealAnimTime));
        }
        else
        {
            m_shuffleAnimationRoutine = StartCoroutine(ReshuffleAnimationRoutine(m_usePlayerCardToggle.isOn, shuffleTime));
        }
    }

    private void ResetGame(int decks, int players)
    {
        if (m_cardsDelt)
        {
            m_cardManager.Restart();
            m_deckBuilder.Restart();
        }

        m_cardsDelt = false;
        decks = Math.Min(decks, 10);
        int numberOfDecks = m_deckBuilder.numberOfDecks;

        if (numberOfDecks != decks)
        {
            m_cardManager.Restart();
            m_deckBuilder.Restart();
            m_deckBuilder.CreateDecks(decks, m_drawDeckTrans);
        }

        players = Math.Min(players, (m_cardManager.numberOfCards / 2));
        CreatePlayers(players);

        m_maxDeckButton.interactable = decks < 10;
        m_removeDeckButton.interactable = decks > 1;
        m_removePlayerButton.interactable = players > 2;
        m_addPlayerButton.interactable = players < (m_cardManager.numberOfCards / 2);
        m_maxPlayerButton.interactable = players < (m_cardManager.numberOfCards / 2);

        m_drawText.text = "Draw Pile\n(" + m_cardManager.cardsInDrawDeck.Count + " Cards)";
        m_discardText.text = "Discard\n(" + m_cardManager.cardsInDiscardPile.Count + " Cards)";

        if (numberOfDecks != decks && m_showReshuffle && m_shuffleAnimationRoutine == null)
        {
            PlayShuffleSFX();

            foreach (var card in m_cardManager.cardsInDrawDeck)
            {
                card.GetComponent<RectTransform>().anchoredPosition = new Vector2(UnityEngine.Random.Range(0f, 1200f), UnityEngine.Random.Range(-700f, 118f));
                card.MoveAndFlipCard(null, shuffleTime, false, 0f, false);
            }

            m_shuffleAnimationRoutine = StartCoroutine(ShuffleAnimationRoutine());
        }
    }

    private void RestartClicked()
    {
        m_soundSource.Stop();
        m_showReshuffle = true;

        StopAllCoroutines();
        m_animationTimingRoutine = null;
        m_shuffleAnimationRoutine = null;

        m_cardManager.Restart();
        m_deckBuilder.Restart();
        ResetGame(m_defaultDecks, m_defaultPlayers);
    }

    private void AddDeck()
    {
        m_showReshuffle = true;
        m_removeDeckButton.interactable = true;
        m_addDeckButton.interactable = (m_deckBuilder.numberOfDecks + 1) < 10;
        m_maxDeckButton.interactable = (m_deckBuilder.numberOfDecks + 1) < 10;

        ResetGame(m_deckBuilder.numberOfDecks + 1, m_playerList.Count);
    }

    private void MaxDeck()
    {
        m_showReshuffle = true;
        m_addDeckButton.interactable = false;
        m_maxDeckButton.interactable = false;
        m_removeDeckButton.interactable = true;

        ResetGame(10, m_playerList.Count);
    }

    private void RemoveDeck()
    {
        m_showReshuffle = true;
        m_maxDeckButton.interactable = true;
        m_addDeckButton.interactable = true;
        m_removeDeckButton.interactable = (m_deckBuilder.numberOfDecks - 1) > 1;
        ResetGame(m_deckBuilder.numberOfDecks - 1, m_playerList.Count);
    }

    private void AddPlayer()
    {
        if (m_playerList.Count < (m_cardManager.numberOfCards / 2))
        {
            m_removePlayerButton.interactable = true;
            ResetGame(m_deckBuilder.numberOfDecks, m_playerList.Count + 1);
        }

        m_addPlayerButton.interactable = m_playerList.Count < (m_cardManager.numberOfCards / 2);
    }

    private void MaxPlayer()
    {
        m_addPlayerButton.interactable = false;
        m_maxPlayerButton.interactable = false;
        m_removePlayerButton.interactable = true;

        ResetGame(m_deckBuilder.numberOfDecks, (m_cardManager.numberOfCards / 2));
    }

    private void RemovePlayer()
    {
        ResetGame(m_deckBuilder.numberOfDecks, m_playerList.Count - 1);

        m_addPlayerButton.interactable = true;
        m_removeDeckButton.interactable = m_playerList.Count > 2;
    }
    #endregion

    #region Animation Coroutines
    IEnumerator ShuffleAnimationRoutine()
    {
        yield return new WaitForSeconds(shuffleTime);

        m_showReshuffle = true;
        m_shuffleAnimationRoutine = null;
    }

    IEnumerator DealAnimationRoutine(float animTime)
    {
        m_dealButton.interactable = false;

        yield return new WaitWhile(() => m_shuffleAnimationRoutine != null);

        m_drawText.text = "Draw Pile\n(" + m_cardManager.cardsInDrawDeck.Count + " Cards)";
        m_discardText.text = "Discard\n(" + m_cardManager.cardsInDiscardPile.Count + " Cards)";

        yield return new WaitForSeconds(animTime);

        PlayFlipSFX();

        yield return new WaitForSeconds(flipTime);

        FindWinner();
        m_dealButton.interactable = true;

        m_animationTimingRoutine = null;
    }

    IEnumerator ReshuffleAnimationRoutine(bool addCardsInPlay, float animTime)
    {
        m_dealButton.interactable = false;
        Transform location = addCardsInPlay ? m_drawDeckTrans : m_discardDeckTrans;

        foreach (var player in m_playerList)
        {
            player.Reshuffle(addCardsInPlay);
        }

        foreach (var card in m_cardManager.cardsInPlay)
        {
            if (addCardsInPlay) card.FlipCard();
            card.MoveAndFlipCard(location, (addCardsInPlay) ? animTime : animTime / 3f, false, 0f, false);
        }

        foreach (var card in m_cardManager.cardsInDiscardPile)
        {
            card.FlipCard();
            card.MoveAndFlipCard(m_drawDeckTrans, animTime, false, 0f, true);
        }

        m_cardManager.ReshuffleDiscard(addCardsInPlay);

        m_drawText.text = "Draw Pile\n(" + m_cardManager.cardsInDrawDeck.Count + " Cards)";
        m_discardText.text = "Discard\n(" + m_cardManager.cardsInDiscardPile.Count + " Cards)";

        PlayShuffleSFX();

        yield return new WaitForSeconds(animTime + 0.1f);

        DealClicked();
        m_shuffleAnimationRoutine = null;
    }
    #endregion

    private void FindWinner()
    {
        List<int> highPlayers = new List<int>();

        highPlayers.Add(0);
        m_playerList[0].Lose();

        for (int index = 1; index < m_playerList.Count; ++index)
        {
            if (m_playerList[index].playerCard > m_playerList[highPlayers[0]].playerCard)
            {
                highPlayers.Clear();
                highPlayers.Add(index);
            }
            else if (m_playerList[index].playerCard == m_playerList[highPlayers[0]].playerCard)
            {
                highPlayers.Add(index);
            }

            m_playerList[index].Lose();
        }

        foreach (int index in highPlayers)
        {
            m_playerList[index].Win();
        }
    }

    private void CreatePlayers(int numberOfPlayers)
    {
        if (m_playerList == null)
            m_playerList = new List<Player>();

        // Destroy old players
        for (int index = numberOfPlayers; index < m_playerList.Count;)
        {
            m_restartButton.onClick.RemoveListener(m_playerList[index].Restart);
            Destroy(m_playerList[index].gameObject);
            m_playerList.RemoveAt(index);
        }

        foreach (var player in m_playerList)
        {
            player.Restart();
        }

        for (int index = m_playerList.Count; index < numberOfPlayers; index++)
        {
            var player = Instantiate(m_playerPrefab, m_playerContainer).GetComponent<Player>();

            player.AddPlayer(index);
            m_restartButton.onClick.AddListener(player.Restart);

            m_playerList.Add(player);
        }
    }

    protected void PlayShuffleSFX()
    {
        m_soundSource.Stop();

        m_soundSource.clip = m_shuffleClip;
        m_soundSource.pitch = m_shuffleClipPitch;
        m_soundSource.Play();
    }

    protected void PlayFlipSFX()
    {
        m_soundSource.Stop();

        m_soundSource.clip = m_flipClip;
        m_soundSource.pitch = m_flipClipPitch;
        m_soundSource.Play();
    }

    public void OnDestroy()
    {
        m_dealButton.onClick.RemoveAllListeners();
        m_restartButton.onClick.RemoveAllListeners();

        m_addDeckButton.onClick.RemoveAllListeners();
        m_removeDeckButton.onClick.RemoveAllListeners();

        m_addPlayerButton.onClick.RemoveAllListeners();
        m_removePlayerButton.onClick.RemoveAllListeners();
    }
}
