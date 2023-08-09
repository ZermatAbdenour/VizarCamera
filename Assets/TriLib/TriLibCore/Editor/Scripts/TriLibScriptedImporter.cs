#pragma warning disable CS0105
using UnityEngine;
using TriLibCore.Interfaces;
using UnityEditor;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
namespace TriLibCore.Editor
{
    public class TriLibScriptedImporter : ScriptedImporter
    {
        public AssetLoaderOptions AssetLoaderOptions
        {
            get
            {
                var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions(true);
                if (userData != null && userData != "null")
                {
                    EditorJsonUtility.FromJsonOverwrite(userData, assetLoaderOptions);
                }
                return assetLoaderOptions;
            }
            set => userData = EditorJsonUtility.ToJson(value);
        }

        public override void OnImportAsset(AssetImportContext assetImportContext)
        {
            var assetLoaderOptions = AssetLoaderOptions;
            var assetLoaderContext = AssetLoader.LoadModelFromFileNoThread(assetImportContext.assetPath, OnError, null, assetLoaderOptions, assetImportContext);
            if (assetLoaderContext.RootGameObject != null)
            {
                assetImportContext.AddObjectToAsset("Main", assetLoaderContext.RootGameObject);
                assetImportContext.SetMainObject(assetLoaderContext.RootGameObject);
                for (var i = 0; i < assetLoaderContext.Allocations.Count; i++)
                {
                    var allocation = assetLoaderContext.Allocations[i];
                    if (string.IsNullOrWhiteSpace(allocation.name))
                    {
                        allocation.name = allocation.GetType().Name;
                    }

                    assetImportContext.AddObjectToAsset(allocation.name, allocation);
                }
            }
        }

        private static void OnError(IContextualizedError contextualizedError)
        {
            var exception = contextualizedError.GetInnerException();
            if (contextualizedError.GetContext() is IAssetLoaderContext assetLoaderContext)
            {
                var assetImportContext = (AssetImportContext)assetLoaderContext.Context.CustomData;
                if (assetLoaderContext.Context.Options.ShowLoadingWarnings)
                {
                    assetImportContext.LogImportError(exception.ToString());
                    return;
                }
            }
            Debug.LogError(exception.ToString());
        }
    }
}