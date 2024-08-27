using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class SpecialBlock : MonoBehaviour
{
#pragma warning disable CS1998
	protected BlockController controller => BlockController.instance;
	protected Block block => GetComponent<Block>();
	public virtual void Start() { }
	public virtual void OnBeforeMerge() { }
	public virtual void OnAfterMerge() { }
	public virtual void OnMergeNeighbor() { }
	public virtual async UniTask OnDropComplete() { }
#pragma warning restore CS1998
}
