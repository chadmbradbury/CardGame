using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using TMPro;

public class Player : MonoBehaviour
{
    [SerializeField] protected Transform m_cardHolder;

    [SerializeField] protected TextMeshProUGUI m_nameText;
    [SerializeField] protected TextMeshProUGUI m_scoreText;

    [SerializeField] protected TextMeshProUGUI m_winText;
    [SerializeField] protected TextMeshProUGUI m_loseText;

    protected int m_score = 0;

    protected Card m_playerCard;
    public Card playerCard { get { return m_playerCard; } }

    [SerializeField] protected Image m_cardBGImage;
    protected Coroutine m_ResultBlinkRoutine = null;
    protected bool m_hasWon = false;

    public void AddPlayer(int playerNumber)
    {
        gameObject.name = "Player_" + ((playerNumber < 9) ? "0" : "") + (playerNumber + 1);
        m_nameText.text = "Player " + (playerNumber + 1);
    }

    public void DealCard(Card newCard, Transform discardTrans, float moveTime, float flipTime)
    {
        if (m_ResultBlinkRoutine != null)
        {
            StopCoroutine(m_ResultBlinkRoutine);

            m_hasWon = false;
            m_winText.enabled = false;
            m_loseText.enabled = false;
            m_ResultBlinkRoutine = null;
            m_cardBGImage.enabled = false;
        }

        if (m_playerCard != null)
        {
            m_playerCard.MoveAndFlipCard(discardTrans, moveTime, false, 0f, false);
        }

        if (newCard == null)
        {
            Debug.LogWarning("Card was delt as null");
        }

        m_playerCard = newCard;
        m_playerCard.MoveAndFlipCard(m_cardHolder, moveTime, true, flipTime, false);
    }

    public void Lose()
    {
        m_hasWon = false;

        if (m_ResultBlinkRoutine == null)
            m_ResultBlinkRoutine = StartCoroutine(BlinkRoutine());
    }

    public void Win()
    {
        m_hasWon = true;
        m_scoreText.text = "Score: " + ++m_score;

        if (m_ResultBlinkRoutine == null)
            m_ResultBlinkRoutine = StartCoroutine(BlinkRoutine());
    }

    public void Restart()
    {
        m_score = 0;
        m_playerCard = null;
        m_winText.enabled = false;
        m_loseText.enabled = false;

        m_scoreText.text = "Score: 0";

        if (m_ResultBlinkRoutine != null)
        {
            StopCoroutine(m_ResultBlinkRoutine);

            m_hasWon = false;
            m_winText.enabled = false;
            m_loseText.enabled = false;
            m_ResultBlinkRoutine = null;
            m_cardBGImage.enabled = false;
        }
    }

    public void Reshuffle(bool resetCard)
    {
        if (m_ResultBlinkRoutine != null)
        {
            StopCoroutine(m_ResultBlinkRoutine);

            m_hasWon = false;
            m_winText.enabled = false;
            m_loseText.enabled = false;
            m_ResultBlinkRoutine = null;
            m_cardBGImage.enabled = false;
        }

        //if (resetCard)
            m_playerCard = null;
    }

    protected readonly Color m_winColor = new Color(0, 1, 0, 0.5f);
    protected readonly Color m_loseColor = new Color(1, 0, 0, 0.5f);

    protected IEnumerator BlinkRoutine()
    {
        yield return new WaitForSeconds(0.25f);

        m_winText.enabled = m_hasWon;
        m_loseText.enabled = !m_hasWon;
        m_cardBGImage.color = (m_hasWon) ? m_winColor : m_loseColor;

        m_cardBGImage.enabled = true;

        yield return new WaitForSeconds(0.25f);

        while (true)
        {
            m_winText.enabled = m_hasWon && !m_winText.enabled;
            m_loseText.enabled = !m_hasWon && !m_loseText.enabled;

            m_cardBGImage.enabled = !m_cardBGImage.enabled;
            m_cardBGImage.color = (m_hasWon) ? m_winColor : m_loseColor;
            yield return new WaitForSeconds(0.25f);
        }
    }
}
