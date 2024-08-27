using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Stage : ScriptableObject
{
	public int stageNumber;
	public Vector2Int boardSize;
	public float blockSpeed;
	public List<CellData> stageCells;
	public List<BlockData> stageBlocks;
	public List<ListWrapper<int>> stageBlockIdGroups;
	public List<BlockData> startBlocks;
	public List<BlockData> spawnBlocks;
	public List<BlockData> blockBoardLimits;
	public List<BlockData> blockSpawnCounts;
	public List<Mission> missions;
	public int nContinue;

#if DEV_TEST
	[HideInInspector] public int width, height, time;
#endif
}

/// <summary>
/// List<List<int>> 타입을 직렬화하기 위한 래퍼 클래스
/// </summary>
/// <typeparam name="T">리스트 원소 타입</typeparam>
[Serializable]
public class ListWrapper<T> : IEnumerable<T>, IEnumerator<T>
{
	[SerializeField]
    private List<T> list = new List<T>();
	private int iCurrent = -1;

    public T this[int i]
    {
        get { return list[i]; }
        set { list[i] = value; }
    }

    public T Current
	{
		get
		{
			try
			{
				return list[iCurrent];
			}
			catch(IndexOutOfRangeException)
			{
				throw new InvalidOperationException();
			}
		}
	}

    object IEnumerator.Current { get { return Current; } }

	public int Count => list.Count;

    public void Dispose()
    {
		// empty
    }

    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)list).GetEnumerator();
    }

    public bool MoveNext()
    {
		return (++iCurrent < list.Count);
    }

    public void Reset()
    {
        iCurrent = -1;
    }

	public void Add(T element)
	{
		list.Add(element);
	}

	public void AddRange(ListWrapper<T> range)
	{
		list.AddRange(range.list);
	}

	public void Remove(T item)
	{
		list.Remove(item);
	}

    public void Clear()
    {
		list.Clear();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)list).GetEnumerator();
    }

	public (T x1, T x2) ToTuple() => (list[0], list[1]);
}