using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public static class Extensions
{
	public static Vector3 WithX(this Vector3 v, float x)
	{
		v.x = x;
		return v;
	}

	public static Vector3 WithY(this Vector3 v, float y)
	{
		v.y = y;
		return v;
	}

	public static Vector3 WithZ(this Vector3 v, float z)
	{
		v.z = z;
		return v;
	}

	public static Color WithAlpha(this Color color, float alpha)
	{
		color.a = alpha;
		return color;
	}

	public static Vector2 WithX(this Vector2 v, float x)
	{
		v.x = x;
		return v;
	}

	public static Vector2 WithY(this Vector2 v, float y)
	{
		v.y = y;
		return v;
	}

	public static T PickOne<T>(this List<T> list)
	{
		if (list.Count == 0) return default;
		int RandomIndex = Random.Range(0, list.Count);
		return list[RandomIndex];
	}

	public static void ShuffleList<T>(this List<T> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			int RandomIndex = Random.Range(0, list.Count);
			T temp = list[i];
			list[i] = list[RandomIndex];
			list[RandomIndex] = temp;
		}
	}

	public static List<T> SampleList<T>(this List<T> list, int SampleCount)
	{
		List<T> DuplicateList = new List<T>(list);
		ShuffleList(DuplicateList);
		if (DuplicateList.Count <= SampleCount)
			return DuplicateList;
		DuplicateList.RemoveRange(SampleCount, DuplicateList.Count - SampleCount);
		return DuplicateList;
	}

	public static void DestroyChildren(this Transform tr)
	{
		foreach (Transform child in tr)
		{
			Object.Destroy(child.gameObject);
		}
	}

	public static string GetSignedText(this int i)
	{
		if (i > 0) return string.Format("+ {0}", Mathf.Abs(i).ToString());
		else if (i < 0) return string.Format("- {0}", Mathf.Abs(i).ToString());
		else return i.ToString();
	}

	public static float ForceParseToFloat(this string str)
	{
		if (float.TryParse(str, out float resultFloat)) return resultFloat;
		else if (int.TryParse(str, out int resultInt)) return resultInt;
		else
		{
			Debug.LogWarning($"{str} 오타");
			return 0f;
		}
	}

	public static string ToPercentString(this float f)
	{
		int Percent = (int)(f * 100f);
		return string.Format("{0} %", Percent);
	}

	public static void TryAndAddDictionaryList<TKey, T>(this Dictionary<TKey, List<T>> dict, TKey key, T item)
	{
		if (!dict.ContainsKey(key)) dict.Add(key, new List<T>());
		dict[key].Add(item);
	}

	public static int ToMilliseconds(this float f) => (int)(f * 1000f);

	public static UniTask<bool> BP(this UniTask t, CancellationTokenSource tokenSource) => t.AttachExternalCancellation(tokenSource.Token).SuppressCancellationThrow();

    public static int GetStringByte(string str)
    {
        int count = 0;

        var charArr = new NativeArray<int>(str.Length, Allocator.TempJob);
        for (int i = 0; i < charArr.Length; i++) charArr[i] = str[i];
        var bytes = new NativeArray<int>(str.Length, Allocator.TempJob);

        var jobGetStringByte = new PJ_GetStringByte();
        jobGetStringByte.charArr = charArr;
        jobGetStringByte.bytes = bytes;

        var handle = jobGetStringByte.Schedule(bytes.Length, 1);
        handle.Complete();

        foreach (int b in bytes) count += b;

        charArr.Dispose();
        bytes.Dispose();

        return count;
    }

    private struct PJ_GetStringByte : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<int> charArr;

        public NativeArray<int> bytes;

        public void Execute(int index)
        {
            if ((charArr[index] >> 7) > 0) bytes[index] = 2;
            else bytes[index] = 1;
        }
    }

	public static bool IsNicknameValid(string nickname)
    {
		foreach(int c in nickname)
        {
			// 한글 자모음이 들어갈시 비유효 판정 결과를 반환한다.
			if(
				c >= 0x1100 && c <= 0x11FF ||
				c >= 0x3130 && c <= 0x318F ||
				c >= 0xA960 && c <= 0xA97F)
            {
				return false;
            }

			// 문자를 표시하지 못해 대리자가 들어갔을시 비유효 판정 결과를 반환한다.
			var category = char.GetUnicodeCategory((char)c);
			switch(category)
            {
				case System.Globalization.UnicodeCategory.OtherSymbol:
				case System.Globalization.UnicodeCategory.Surrogate:
				case System.Globalization.UnicodeCategory.ModifierSymbol:
				case System.Globalization.UnicodeCategory.NonSpacingMark:
					return false;
			}
        }
		return true;
    }
}
