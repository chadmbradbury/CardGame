using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Card : MonoBehaviour
{
    public enum Suit
    {
        Club = 0,
        Diamond,
        Heart,
        Spade
    };

    public enum CardName
    {
        Ace = 0,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King    
    };

    [Serializable]
    public class CardValue
    {
        public Suit m_suit;
        //public Suit suit { get { return m_suit; } }

        public CardName m_cardName;
        //public CardName cardName { get { return m_cardName; } }

        public static bool operator <(CardValue lhs, CardValue rhs)
        {
            if (lhs.m_cardName < rhs.m_cardName)
                return true;

            if (lhs.m_cardName == rhs.m_cardName &&
                lhs.m_suit < rhs.m_suit)
            {
                return true;
            }

            return false;
        }

        public static bool operator >(CardValue lhs, CardValue rhs)
        {
            if (lhs.m_cardName > rhs.m_cardName)
                return true;

            if (lhs.m_cardName == rhs.m_cardName &&
                lhs.m_suit > rhs.m_suit)
            {
                return true;
            }

            return false;
        }

        public override bool Equals(object Obj)
        {
            CardValue other = (CardValue)Obj;
        
            if (other is null)
                return false;
        
            return (this == other);
        }
        
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(CardValue lhs, CardValue rhs)
        {
            if (lhs is null || rhs is null)
            {
                return (lhs is null && rhs is null);
            }

            return (lhs.m_cardName == rhs.m_cardName && lhs.m_suit == rhs.m_suit);
        }

        public static bool operator !=(CardValue lhs, CardValue rhs)
        {
            return !(lhs == rhs);
        }

    }

    public CardValue m_cardValue;
    public float m_cardWeight;

    public Image m_cardFace;
    public Image m_cardBack;

    protected Coroutine m_cardMoveRoutine;
    protected Coroutine m_flipCardRoutine;
    protected Coroutine m_moveToDiscardRoutine;
    protected Coroutine m_moveCardToDrawRoutine;

    protected bool m_faceUp = false;
    protected RectTransform m_myRectTrans;

    #region Operator Overloads
    public static bool operator <(Card lhs, Card rhs)
    {
        return (lhs.m_cardValue < rhs.m_cardValue);
    }

    public static bool operator >(Card lhs, Card rhs)
    {
        return (lhs.m_cardValue > rhs.m_cardValue);
    }

    public override bool Equals(object Obj)
    {
        Card other = (Card)Obj;

        if (other is null)
            return false;

        return (gameObject.name == other.gameObject.name);
    }

    public static bool operator ==(Card lhs, Card rhs)
    {
        if (lhs is null || rhs is null)
        {
            return (lhs is null && rhs is null);
        }

        return (lhs.m_cardValue == rhs.m_cardValue);
    }

    public static bool operator !=(Card lhs, Card rhs)
    {
        return !(lhs == rhs);
    }

    public override int GetHashCode()
    {
        return gameObject.name.GetHashCode();
    }
    #endregion

    public void InitCard(Suit suit, CardName cardName, float cardWeight = 1f)
    {
        m_cardValue.m_suit = suit;
        m_cardValue.m_cardName = cardName;

        m_cardWeight = cardWeight;

        SetCardFace();

        m_myRectTrans = GetComponent<RectTransform>();
    }

    public void MoveAndFlipCard(Transform newParent, float moveTime, bool performFlipAnim, float flipTime, bool randomMid)
    {
        if (newParent != null)
            transform.SetParent(newParent, true);

        if (m_cardMoveRoutine == null)
        {
            if (randomMid)
                m_cardMoveRoutine = StartCoroutine(MoveCardWithRandomMidPointRoutine(moveTime));
            else
                m_cardMoveRoutine = StartCoroutine(MoveCardRoutine(moveTime));
        }

        if (performFlipAnim && m_flipCardRoutine == null)
            m_flipCardRoutine = StartCoroutine(FlipCardRoutine(flipTime));
    }

    public void FlipCard()
    {
        m_faceUp = !m_faceUp;
        m_cardBack.enabled = !m_cardBack.enabled;
        m_cardFace.enabled = !m_cardFace.enabled;
    }

    protected void SetCardFace()
    {
        m_cardFace.sprite = Resources.Load<Sprite>(SuitFolders[((int)m_cardValue.m_suit)] + "\\" + 
            CardFileNames[((int)m_cardValue.m_cardName)] + SuitFolders[((int)m_cardValue.m_suit)][0]);

        m_faceUp = false;
        
        m_cardBack.enabled = true;
        m_cardFace.enabled = false;
    }

    IEnumerator FlipCardRoutine(float totalTime)
    {
        yield return new WaitWhile(() => m_cardMoveRoutine != null);

        float timeRunning = 0f;
        totalTime = totalTime / 2f;

        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 90, 0));

        while ((timeRunning / totalTime) <= 1f)
        {
            transform.rotation = Quaternion.Lerp(Quaternion.identity, targetRotation, (timeRunning / totalTime));
            timeRunning += Time.deltaTime;
            yield return null;
        }

        timeRunning = 0f;
        m_cardBack.enabled = m_faceUp;
        m_cardFace.enabled = !m_faceUp;
        
        while ((timeRunning / totalTime) <= 1f)
        {
            transform.rotation = Quaternion.Lerp(targetRotation, Quaternion.identity, (timeRunning / totalTime));
            timeRunning += Time.deltaTime;
            yield return null;
        }

        m_faceUp = !m_faceUp;
        transform.rotation = Quaternion.identity;

        m_flipCardRoutine = null;
    }

    IEnumerator MoveCardRoutine(float totalTime)
    {
        float timeRunning = 0f;

        var startScale = m_myRectTrans.sizeDelta;
        var startPos = m_myRectTrans.anchoredPosition;

        while (timeRunning < totalTime)
        {
            m_myRectTrans.sizeDelta = Vector2.Lerp(startScale, Vector2.zero, (timeRunning / totalTime));
            m_myRectTrans.anchoredPosition = Vector2.Lerp(startPos, Vector2.zero, (timeRunning / totalTime));

            timeRunning += Time.deltaTime;
            yield return null;
        }

        m_myRectTrans.sizeDelta = Vector2.zero;
        m_myRectTrans.anchoredPosition = Vector2.zero;
        m_cardMoveRoutine = null;
    }

    IEnumerator MoveCardWithRandomMidPointRoutine(float totalTime)
    {
        float timeRunning = 0f;
        float timeToRun = totalTime / 3f;

        var startPos = m_myRectTrans.anchoredPosition;

        Vector2 target = new Vector2(UnityEngine.Random.Range(200, startPos.x), UnityEngine.Random.Range(startPos.y, 118f));

        while (timeRunning < timeToRun)
        {
            m_myRectTrans.anchoredPosition = Vector2.Lerp(startPos, target, (timeRunning / timeToRun));

            timeRunning += Time.deltaTime;

            yield return null;
        }

        m_cardMoveRoutine = StartCoroutine(MoveCardRoutine(totalTime - timeToRun));
    }

    private readonly string[] SuitFolders = new string[]
    {
        "Clubs",
        "Diamonds",
        "Hearts",
        "Spades"
    };

    private readonly string[] CardFileNames = new string[]
    {
        "01_A_",
        "02_2_",
        "03_3_",
        "04_4_",
        "05_5_",
        "06_6_",
        "07_7_",
        "08_8_",
        "09_9_",
        "10_10_",
        "11_J_",
        "12_Q_",
        "13_K_",        
    };

    protected void OnDestroy()
    {
        StopAllCoroutines();
    }
}
