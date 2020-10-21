using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPoints : MonoBehaviour
{
    public static RandomPoints instance;

    public Transform[] spawnPoints;
    public Transform[] resultOfChooseSet;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        Shuffle(spawnPoints);
        resultOfChooseSet = ChooseSet(MultiplayerSettings.instance.maxPlayers);
    }


    void Shuffle<T>(T[] deck)
    {
        for (int i = 0; i < deck.Length; i++)
        {
            T aux = deck[i];
            int randomIndex = Random.Range(0, deck.Length);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = aux;
        }
    }

    private Transform[] ChooseSet(int numRequired)
    {
        Transform[] result = new Transform[numRequired];

        int numToChoose = numRequired;

        for (int numAvailable = spawnPoints.Length; numAvailable > 0; numAvailable--)
        {
            float prob = numToChoose / numAvailable;
            if (Random.value <= prob)
            {
                numToChoose--;
                result[numToChoose] = spawnPoints[numAvailable - 1];
            }

            if (numToChoose == 0) break;
        }

        return result;
    }
}
