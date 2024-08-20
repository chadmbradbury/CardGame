using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Linq;

public static class WeightedRandomDraw
{
    private static List<int> cardList = new List<int>();

    public static Card[] DealCards(int numPlayers)
    {
        CreateWeights();

        HashSet<int> usedCards = new HashSet<int>();
        int random = UnityEngine.Random.Range(0, cardList.Count);

        Card[] returnCards = new Card[numPlayers];

        for (int cardIndex = 0; cardIndex < numPlayers; ++cardIndex)
        {
            while (usedCards.Contains(cardList[random]))
            {
                random = UnityEngine.Random.Range(0, cardList.Count);
            }

            usedCards.Add(cardList[random]);
            returnCards[cardIndex] = CardManager.instance.cardsInDrawDeck[cardList[random]];
        }

        return returnCards;
    }

    private static void CreateWeights()
    {
        cardList = new List<int>();
        var drawDeck = CardManager.instance.cardsInDrawDeck;

        for (int card = 0; card < drawDeck.Count; card++)
        {
            for (int weight = 0; weight < drawDeck[card].m_cardWeight * 10; ++weight)
            {
                cardList.Add(card);
            }
        }

        // Just for fun, randomize the numbers even more
        System.Random rnd = new System.Random();
        cardList.Sort((x, y) => rnd.Next(-1, 1));
    }
}
