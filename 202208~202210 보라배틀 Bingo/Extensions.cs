using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections.Generic;
using UnityEngine;


namespace BoraBattle.Game.BingoMasterKing
{
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

		public static T PickOne<T>(this List<T> list, Well512Random randomProvider)
		{
			int RandomIndex = (int)randomProvider.Next(0, list.Count);
			return list[RandomIndex];
		}

		public static void ShuffleList<T>(this List<T> list, Well512Random randomProvider)
		{
			for (int i = 0; i < list.Count; i++)
			{
				//int RandomIndex = Random.Range(0, list.Count);
				int RandomIndex = (int)randomProvider.Next(0, list.Count);
				T temp = list[i];
				list[i] = list[RandomIndex];
				list[RandomIndex] = temp;
			}
		}

		public static List<T> SampleList<T>(this List<T> list, int SampleCount, Well512Random randomProvider)
		{
			List<T> DuplicateList = new List<T>(list);
			ShuffleList(DuplicateList, randomProvider);
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

        public static void Play(this Animator anim, string stateName, bool play)
        {
            anim.Play(stateName, 0, play ? 0f : 1f);
            anim.speed = play ? 1f : 0f;
        }

		public static bool Done(this Animator anim) => anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99f;
    }
}
