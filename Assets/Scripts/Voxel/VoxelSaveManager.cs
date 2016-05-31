#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Vox {
	public class VoxelSaveManager : UnityEditor.AssetModificationProcessor {

		public static string[] OnWillSaveAssets(string[] paths) {
			if (!isSavingScene(paths))
				return paths;


			MonoBehaviour.print("Saving scene.");
			return paths;
		}

		protected static bool isSavingScene(string[] paths) {
			foreach (string path in paths) {
				if (path.EndsWith(".unity"))
					return true;
			}
			return false;
		}
	}
}
#endif