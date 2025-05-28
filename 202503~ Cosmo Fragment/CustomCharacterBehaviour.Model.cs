using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static SingletonManager;
using static DataManager;

namespace Naninovel
{
	public partial class CustomCharacterBehaviour
    {
        public CharacterName characterName;

        public class ModelInfo
		{
			public string _ClothName;
			public GameObject _ModelInst;
			public AsyncOperationHandle<GameObject> _LoadHandle;
		}
		List<ModelInfo> _ModelInfos;

		void OnDestroy()
		{
			foreach (ModelInfo loadInfo in _ModelInfos)
			{
				Addressables.Release(loadInfo._LoadHandle);
			}
		}

#if SPECIAL_PROJECT
        void ExternalOnEnable()
		{
			ModelInit();
        }
#endif

        void ModelInit()
        {
			// 캐릭터 복장 로드
			_ModelInfos ??= new();
			CharacterCloth clothInfo = Data.GetCharacterCloth(characterName);
			ModelInfo modelInfo = _ModelInfos.Find(x => x._ClothName == clothInfo._Name);
			if (modelInfo == null)
			{
				AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(clothInfo._Model);
				ModelInfo loadInfo = new()
				{
					_ClothName = clothInfo._Name,
					_LoadHandle = handle,
				};
				GameObject asset = handle.WaitForCompletion();
				loadInfo._ModelInst = Instantiate(asset, transform);
				_ModelInfos.Add(loadInfo);
			}

			// 활성화
			foreach (ModelInfo loadInfo in _ModelInfos)
			{
				loadInfo._ModelInst.SetActive(loadInfo._ClothName == clothInfo._Name);
				Util.SetActiveSchoolUniform(loadInfo._ModelInst, clothInfo);
				SkinnedMeshRenderer[] renderers = loadInfo._ModelInst.GetComponentsInChildren<SkinnedMeshRenderer>();
				foreach (SkinnedMeshRenderer renderer in renderers)
				{
					renderer.enabled = false;
				}
			}
		}
    }
}
