using System.Collections.Generic;
using UnityEngine;


namespace BoraBattle.Game.WorldBasketballKing
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

		public static T PickOne<T>(this List<T> list)
		{
			int RandomIndex = Random.Range(0, list.Count);
			return list[RandomIndex];
		}

		public static Transform FindRecursive(this Transform parent, string n)
		{
			Transform result = null;
			for (int i = 0; i < parent.childCount; i++)
			{
				Transform child = parent.GetChild(i);
				if (child.name == n) return child;
				result = FindRecursive(child, n);
				if (result) break;
			}

			return result;
		}
	}
}
