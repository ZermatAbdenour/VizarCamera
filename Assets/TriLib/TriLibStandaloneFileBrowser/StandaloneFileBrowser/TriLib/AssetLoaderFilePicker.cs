#pragma warning disable 618

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TriLibCore.SFB;
using TriLibCore.General;
using TriLibCore.Mappers;
using TriLibCore.Utils;
using UnityEngine;

namespace TriLibCore
{
    /// <summary>Represents an Asset Loader which loads files using a platform-specific file picker.</summary>
    public class AssetLoaderFilePicker : MonoBehaviour
    {
        private IList<ItemWithStream> _items;
        private string _modelExtension;
        private Action<AssetLoaderContext> _onLoad;
        private Action<AssetLoaderContext> _onMaterialsLoad;
        private Action<AssetLoaderContext, float> _onProgress;
        private Action<IContextualizedError> _onError;
        private Action<bool> _onBeginLoad;
        private GameObject _wrapperGameObject;
        private AssetLoaderOptions _assetLoaderOptions;
        private bool _haltTask;

        /// <summary>Creates the Asset Loader File Picker Singleton instance.</summary>
        /// <returns>The created AssetLoaderFilePicker.</returns>
        public static AssetLoaderFilePicker Create()
        {
            var gameObject = new GameObject("AssetLoaderFilePicker");
            var assetLoaderFilePicker = gameObject.AddComponent<AssetLoaderFilePicker>();
            return assetLoaderFilePicker;
        }

        /// <summary>Loads a Model from the OS file picker asynchronously, or synchronously when the OS doesn't support Threads.</summary>
        /// <param name="title">The dialog title.</param>
        /// <param name="onLoad">The Method to call on the Main Thread when the Model is loaded but resources may still pending.</param>
        /// <param name="onMaterialsLoad">The Method to call on the Main Thread when the Model and resources are loaded.</param>
        /// <param name="onProgress">The Method to call when the Model loading progress changes.</param>
        /// <param name="onBeginLoad">The Method to call when the model begins to load.</param>
        /// <param name="onError">The Method to call on the Main Thread when any error occurs.</param>
        /// <param name="wrapperGameObject">The Game Object that will be the parent of the loaded Game Object. Can be null.</param>
        /// <param name="assetLoaderOptions">The options to use when loading the Model.</param>
        /// <param name="haltTask">Turn on this field to avoid loading the model immediately and chain the Tasks.</param>
        public void LoadModelFromFilePickerAsync(string title, Action<AssetLoaderContext> onLoad, Action<AssetLoaderContext> onMaterialsLoad, Action<AssetLoaderContext, float> onProgress, Action<bool> onBeginLoad, Action<IContextualizedError> onError, GameObject wrapperGameObject, AssetLoaderOptions assetLoaderOptions, bool haltTask = false)
        {
            _onLoad = onLoad;
            _onMaterialsLoad = onMaterialsLoad;
            _onProgress = onProgress;
            _onError = onError;
            _onBeginLoad = onBeginLoad;
            _wrapperGameObject = wrapperGameObject;
            _assetLoaderOptions = assetLoaderOptions;
            _haltTask = haltTask;
            try
            {
				StandaloneFileBrowser.OpenFilePanelAsync(title, null, GetExtensions(), true, OnItemsWithStreamSelected);
            }
            catch (Exception)
            {
                Dispatcher.InvokeAsync(DestroyMe);
                throw;
            }
        }

        private void DestroyMe()
        {
            Destroy(gameObject);
        }

        private void HandleFileLoading()
        {
            StartCoroutine(DoHandleFileLoading());
        }

        private IEnumerator DoHandleFileLoading()
        {
            var hasFiles = _items != null && _items.Count > 0 && _items[0].HasData;
            _onBeginLoad?.Invoke(hasFiles);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            if (!hasFiles)
            {
                DestroyMe();
                yield break;
            }
            var modelFileWithStream = FindModelFile();
            var modelFilename = modelFileWithStream.Name;
            var modelStream = modelFileWithStream.OpenStream();
            if (_assetLoaderOptions == null)
            {
                _assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
            }
            _assetLoaderOptions.TextureMapper = ScriptableObject.CreateInstance<FilePickerTextureMapper>();
            _assetLoaderOptions.ExternalDataMapper = ScriptableObject.CreateInstance<FilePickerExternalDataMapper>();
            _modelExtension = modelFilename != null ? FileUtils.GetFileExtension(modelFilename, false) : null;
            if (_modelExtension == "zip")
            {
                if (modelStream != null)
                {
                    AssetLoaderZip.LoadModelFromZipStream(modelStream, _onLoad, _onMaterialsLoad, _onProgress, _onError, _wrapperGameObject, _assetLoaderOptions, _items, null);
                }
                else
                {
                    AssetLoaderZip.LoadModelFromZipFile(modelFilename, _onLoad, _onMaterialsLoad, _onProgress, _onError, _wrapperGameObject, _assetLoaderOptions, _items, null);
                }
            }
            else
            {
                if (modelStream != null)
                {
                    AssetLoader.LoadModelFromStream(modelStream, modelFilename, _modelExtension, _onLoad, _onMaterialsLoad, _onProgress, _onError, _wrapperGameObject, _assetLoaderOptions, _items, _haltTask);
                }
                else
                {
                    AssetLoader.LoadModelFromFile(modelFilename, _onLoad, _onMaterialsLoad, _onProgress, _onError, _wrapperGameObject, _assetLoaderOptions, _items, _haltTask);
                }
            }
            DestroyMe();
        }

        private static ExtensionFilter[] GetExtensions()
        {
            var extensions = Readers.Extensions;
            var extensionFilters = new List<ExtensionFilter>();
            var subExtensions = new List<string>();
            for (var i = 0; i < extensions.Count; i++)
            {
                var extension = extensions[i];
                extensionFilters.Add(new ExtensionFilter(null, extension));
                subExtensions.Add(extension);
            }

            subExtensions.Add("zip");
            extensionFilters.Add(new ExtensionFilter(null, new[] { "zip" }));
            extensionFilters.Add(new ExtensionFilter("All Files", new[] { "*" }));
            extensionFilters.Insert(0, new ExtensionFilter("Accepted Files", subExtensions.ToArray()));
            return extensionFilters.ToArray();
        }
        
        private ItemWithStream FindModelFile()
        {
            if (_items.Count == 1)
            {
                return _items.First();
            }
            var extensions = Readers.Extensions;
            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item.Name == null)
                {
                    continue;
                }

                var extension = FileUtils.GetFileExtension(item.Name, false);
                if (extensions.Contains(extension))
                {
                    return item;
                }
            }

            return null;
        }
		
        private void OnItemsWithStreamSelected(IList<ItemWithStream> itemsWithStream)
        {
			if (itemsWithStream != null)
            {
                _items = itemsWithStream;
                Dispatcher.InvokeAsync(HandleFileLoading);
            } else {
                DestroyMe();
            }    
        }
    }
}
