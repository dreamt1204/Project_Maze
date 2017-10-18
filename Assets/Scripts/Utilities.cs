using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    // Return random X unique integers from total number
    public static int[] GetRandomUniqueNumbers(int num, int totalNum)
    {
        if (num > totalNum)
            num = totalNum;

        List<int> numbers = new List<int>(totalNum);
        for (int i = 0; i < totalNum; i++)
        {
            numbers.Add(i);
        }
        int[] randomNumbers = new int[num];
        for (int i = 0; i < randomNumbers.Length; i++)
        {
            int thisNumber = Random.Range(0, numbers.Count);
            randomNumbers[i] = numbers[thisNumber];
            numbers.RemoveAt(thisNumber);
        }

        return randomNumbers;
    }

	public static void TryCatchError(bool statement, string message)
	{
		if( statement )
			Debug.LogError("Error: " + message);
	}
}
