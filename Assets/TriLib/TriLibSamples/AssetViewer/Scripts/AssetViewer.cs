#pragma warning disable 649
#pragma warning disable 108
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TriLibCore.SFB;
using TriLibCore.General;
using TriLibCore.Extensions;
#if TRILIB_SHOW_MEMORY_USAGE
using TriLibCore.Utils;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
namespace TriLibCore.Samples
{
    /// <summary>Represents a TriLib sample which allows the user to load models and HDR skyboxes from the local file-system.</summary>
    public class AssetViewer : AssetViewerBase
    {
        /// <summary>
        /// Maximum camera distance ratio based on model bounds.
        /// </summary>
        private const float MaxCameraDistanceRatio = 3f;

        /// <summary>
        /// Camera distance ratio based on model bounds.
        /// </summary>
        protected const float CameraDistanceRatio = 2f;

        /// <summary>
        /// minimum camera distance.
        /// </summary>
        protected const float MinCameraDistance = 0.01f;

        /// <summary>
        /// Skybox scale based on model bounds.
        /// </summary>
        protected const float SkyboxScale = 100f;

        /// <summary>
        /// Skybox game object.
        /// </summary>
        [SerializeField]
        protected GameObject Skybox;

        /// <summary>
        /// Scene CanvasScaler.
        /// </summary>
        [SerializeField]
        protected CanvasScaler CanvasScaler;

        /// <summary>
        /// Camera selection Dropdown.
        /// </summary>
        [SerializeField]
        private Dropdown _camerasDropdown;

        /// <summary>
        /// Camera loading Toggle.
        /// </summary>
        [SerializeField]
        private Toggle _loadCamerasToggle;

        /// <summary>
        /// Lights loading Toggle.
        /// </summary>
        [SerializeField]
        private Toggle _loadLightsToggle;

        /// <summary>
        /// Skybox game object renderer.
        /// </summary>
        [SerializeField]
        private Renderer _skyboxRenderer;

        /// <summary>
        /// Directional light.
        /// </summary>
        [SerializeField]
        private Light _light;

        /// <summary>
        /// Skybox material preset to create the final skybox material.
        /// </summary>
        [SerializeField]
        private Material _skyboxMaterialPreset;

        /// <summary>
        /// Main reflection probe.
        /// </summary>
        [SerializeField]
        private ReflectionProbe _reflectionProbe;

        /// <summary>
        /// Skybox exposure slider.
        /// </summary>
        [SerializeField]
        private Slider _skyboxExposureSlider;

        /// <summary>
        /// Loading time indicator.
        /// </summary>
        [SerializeField]
        private Text _loadingTimeText;

        /// <summary>
        /// Main scene Camera.
        /// </summary>
        [SerializeField]
        private Camera _mainCamera;

        /// <summary>
        /// Current camera distance.
        /// </summary>
        protected float CameraDistance = 1f;

        /// <summary>
        /// Current camera pivot position.
        /// </summary>
        protected Vector3 CameraPivot;
        
        /// <summary>
        /// Input multiplier based on loaded model bounds.
        /// </summary>
        protected float InputMultiplier = 1f;

        /// <summary>
        /// Skybox instantiated material.
        /// </summary>
        private Material _skyboxMaterial;

        /// <summary>
        /// Texture loaded for skybox.
        /// </summary>
        private Texture2D _skyboxTexture;

        /// <summary>
        /// List of loaded animations.
        /// </summary>
        private List<AnimationClip> _animations;

        /// <summary>
        /// Created animation component for the loaded model.
        /// </summary>
        private Animation _animation;

        /// <summary>
        /// Loaded model cameras.
        /// </summary>
        private IList<Camera> _cameras;

        /// <summary>
        /// Stop Watch used to track the model loading time.
        /// </summary>
        private Stopwatch _stopwatch;

        /// <summary>
        /// Represents the memory used by the Unity Player when the scene is loaded.
        /// </summary>
        private long _initialMemory;

        /// <summary>
        /// Current directional light angle.
        /// </summary>
        private Vector2 _lightAngle = new Vector2(0f, -45f);

        /// <summary>Gets the playing Animation State.</summary>
        private AnimationState CurrentAnimationState
        {
            get
            {
                if (_animation != null)
                {
                    return _animation[PlaybackAnimation.options[PlaybackAnimation.value].text];
                }
                return null;
            }
        }

        /// <summary>Is there any animation playing?</summary>
        private bool AnimationIsPlaying => _animation != null && _animation.isPlaying;

        /// <summary>
        /// Shows the file picker for loading a model from the local file-system.
        /// </summary>
        public void LoadModelFromFile()
        {
            AssetLoaderOptions.ImportCameras = _loadCamerasToggle.isOn;
            AssetLoaderOptions.ImportLights = _loadLightsToggle.isOn;
            base.LoadModelFromFile();
        }

        /// <summary>
        /// Shows the URL selector for loading a model from network.
        /// </summary>
        public void LoadModelFromURLWithDialogValues()
        {
            AssetLoaderOptions.ImportCameras = _loadCamerasToggle.isOn;
            AssetLoaderOptions.ImportLights = _loadLightsToggle.isOn;
            base.LoadModelFromURLWithDialogValues();
        }

        /// <summary>Shows the file picker for loading a skybox from the local file-system.</summary>
        public void LoadSkyboxFromFile()
        {
            SetLoading(false);
            var title = "Select a skybox image";
            var extensions = new ExtensionFilter[]
            {
                new ExtensionFilter("Radiance HDR Image (hdr)", "hdr")
            };
            StandaloneFileBrowser.OpenFilePanelAsync(title, null, extensions, true, OnSkyboxStreamSelected);
        }

        /// <summary>
        /// Removes the skybox texture.
        /// </summary>
        public void ClearSkybox()
        {
            if (_skyboxMaterial == null)
            {
                _skyboxMaterial = Instantiate(_skyboxMaterialPreset);
            }
            _skyboxMaterial.mainTexture = null;
            _skyboxExposureSlider.value = 1f;
            OnSkyboxExposureChanged(1f);
        }

        public void ResetModelScale()
        {
            if (RootGameObject != null)
            {
                RootGameObject.transform.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// Plays the selected animation.
        /// </summary>
        public override void PlayAnimation()
        {
            if (_animation == null)
            {
                return;
            }
            _animation.Play(PlaybackAnimation.options[PlaybackAnimation.value].text);
        }

        /// <summary>
        /// Stop playing the selected animation.
        /// </summary>
        public override void StopAnimation()
        {
            if (_animation == null)
            {
                return;
            }
            PlaybackSlider.value = 0f;
            _animation.Stop();
            SampleAnimationAt(0f);
        }

        /// <summary>Switches to the animation selected on the Dropdown.</summary>
        /// <param name="index">The selected Animation index.</param>
        public override void PlaybackAnimationChanged(int index)
        {
            StopAnimation();
        }

        /// <summary>Switches to the camera selected on the Dropdown.</summary>
        /// <param name="index">The selected Camera index.</param>
        public void CameraChanged(int index)
        {
            for (var i = 0; i < _cameras.Count; i++)
            {
                var camera = _cameras[i];
                camera.enabled = false;
            }
            if (index == 0)
            {
                _mainCamera.enabled = true;
            }
            else
            {
                _cameras[index - 1].enabled = true;
            }
        }

        /// <summary>Event triggered when the Animation slider value has been changed by the user.</summary>
        /// <param name="value">The Animation playback normalized position.</param>
        public override void PlaybackSliderChanged(float value)
        {
            if (!AnimationIsPlaying)
            {
                var animationState = CurrentAnimationState;
                if (animationState != null)
                {
                    SampleAnimationAt(value);
                }
            }
        }

        /// <summary>Samples the Animation at the given normalized time.</summary>
        /// <param name="value">The Animation normalized time.</param>
        private void SampleAnimationAt(float value)
        {
            if (_animation == null || RootGameObject == null)
            {
                return;
            }
            var animationClip = _animation.GetClip(PlaybackAnimation.options[PlaybackAnimation.value].text);
            animationClip.SampleAnimation(RootGameObject, animationClip.length * value);
        }

        /// <summary>
        /// Event triggered when the user selects the skybox on the selection dialog.
        /// </summary>
        /// <param name="files">Selected files.</param>
        private void OnSkyboxStreamSelected(IList<ItemWithStream> files)
        {
            if (files != null && files.Count > 0 && files[0].HasData)
            {
                Utils.Dispatcher.InvokeAsyncUnchecked(LoadSkybox, files[0].OpenStream());
            }
            else
            {
                Utils.Dispatcher.InvokeAsync(ClearSkybox);
            }
        }

        /// <summary>Loads the skybox from the given Stream.</summary>
        /// <param name="stream">The Stream containing the HDR Image data.</param>
        /// <returns>Coroutine IEnumerator.</returns>
        private IEnumerator DoLoadSkybox(Stream stream)
        {
            //Double frame waiting hack
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            if (_skyboxTexture != null)
            {
                Destroy(_skyboxTexture);
            }
            ClearSkybox();
            _skyboxTexture = HDRLoader.HDRLoader.Load(stream, out var gamma, out var exposure);
            _skyboxMaterial.mainTexture = _skyboxTexture;
            _skyboxExposureSlider.value = 1f;
            OnSkyboxExposureChanged(exposure);
            stream.Close();
            SetLoading(false);
        }

        /// <summary>Starts the Coroutine to load the skybox from the given Sstream.</summary>
        /// <param name="stream">The Stream containing the HDR Image data.</param>
        private void LoadSkybox(Stream stream)
        {
            SetLoading(true);
            StartCoroutine(DoLoadSkybox(stream));
        }

        /// <summary>Event triggered when the skybox exposure Slider has changed.</summary>
        /// <param name="exposure">The new exposure value.</param>
        public void OnSkyboxExposureChanged(float exposure)
        {
            _skyboxMaterial.SetFloat("_Exposure", exposure);
            _skyboxRenderer.material = _skyboxMaterial;
            RenderSettings.skybox = _skyboxMaterial;
            DynamicGI.UpdateEnvironment();
            _reflectionProbe.RenderProbe();
        }

        /// <summary>Initializes the base-class and clears the skybox Texture.</summary>
        protected override void Start()
        {
            base.Start();
            if (SystemInfo.deviceType == DeviceType.Handheld)
            {
                CanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            }
            AssetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
            AssetLoaderOptions.Timeout = 180;
            AssetLoaderOptions.ShowLoadingWarnings = true;
#if TRILIB_SHOW_MEMORY_USAGE
            _initialMemory = RuntimeProcessUtils.GetProcessMemory();
#endif
            ClearSkybox();
        }

        /// <summary>Handles the input.</summary>
        private void Update()
        {
            ProcessInput();
            UpdateHUD();
        }

        /// <summary>Handles the input and moves the Camera accordingly.</summary>
        protected virtual void ProcessInput()
        {
            if (!_mainCamera.enabled)
            {
                return;
            }
            ProcessInputInternal(_mainCamera.transform);
        }

        /// <summary>
        /// Handles the input using the given Camera.
        /// </summary>
        /// <param name="cameraTransform">The Camera to process input movements.</param>
        private void ProcessInputInternal(Transform cameraTransform)
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (GetMouseButton(0))
                {
                    if (GetKey(KeyCode.LeftAlt) || GetKey(KeyCode.RightAlt))
                    {
                        _lightAngle.x = Mathf.Repeat(_lightAngle.x + GetAxis("Mouse X"), 360f);
                        _lightAngle.y = Mathf.Clamp(_lightAngle.y + GetAxis("Mouse Y"), -MaxPitch, MaxPitch);
                    }
                    else
                    {
                        UpdateCamera();
                    }
                }
                if (GetMouseButton(2))
                {
                    CameraPivot -= cameraTransform.up * GetAxis("Mouse Y") * InputMultiplier + cameraTransform.right * GetAxis("Mouse X") * InputMultiplier;
                }
                CameraDistance = Mathf.Min(CameraDistance - GetMouseScrollDelta().y * InputMultiplier, InputMultiplier * (1f / InputMultiplierRatio) * MaxCameraDistanceRatio);
                if (CameraDistance < 0f)
                {
                    CameraPivot += cameraTransform.forward * -CameraDistance;
                    CameraDistance = 0f;
                }
                Skybox.transform.position = CameraPivot;
                cameraTransform.position = CameraPivot + Quaternion.AngleAxis(CameraAngle.x, Vector3.up) * Quaternion.AngleAxis(CameraAngle.y, Vector3.right) * new Vector3(0f, 0f, Mathf.Max(MinCameraDistance, CameraDistance));
                cameraTransform.LookAt(CameraPivot);
                _light.transform.position = CameraPivot + Quaternion.AngleAxis(_lightAngle.x, Vector3.up) * Quaternion.AngleAxis(_lightAngle.y, Vector3.right) * Vector3.forward;
                _light.transform.LookAt(CameraPivot);
            }
        }

        /// <summary>Updates the HUD information.</summary>
        private void UpdateHUD()
        {
            var animationState = CurrentAnimationState;
            var time = animationState == null ? 0f : PlaybackSlider.value * animationState.length % animationState.length;
            var seconds = time % 60f;
            var milliseconds = time * 100f % 100f;
            PlaybackTime.text = $"{seconds:00}:{milliseconds:00}";
            var normalizedTime = animationState == null ? 0f : animationState.normalizedTime % 1f;
            if (AnimationIsPlaying)
            {
                PlaybackSlider.value = float.IsNaN(normalizedTime) ? 0f : normalizedTime;
            }
            var animationIsPlaying = AnimationIsPlaying;
            if (_animation != null)
            {
                Play.gameObject.SetActive(!animationIsPlaying);
                Stop.gameObject.SetActive(animationIsPlaying);
            }
            else
            {
                Play.gameObject.SetActive(true);
                Stop.gameObject.SetActive(false);
                PlaybackSlider.value = 0f;
            }
        }

        /// <summary>Event triggered when the user selects a file or cancels the Model selection dialog.</summary>
        /// <param name="hasFiles">If any file has been selected, this value is <c>true</c>, otherwise it is <c>false</c>.</param>
        protected override void OnBeginLoadModel(bool hasFiles)
        {
            base.OnBeginLoadModel(hasFiles);
            if (hasFiles)
            {
                _animations = null;
                _loadingTimeText.text = null;
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
            }
        }

        /// <summary>Event triggered when the Model Meshes and hierarchy are loaded.</summary>
        /// <param name="assetLoaderContext">The Asset Loader Context reference. Asset Loader Context contains the Model loading data.</param>
        protected override void OnLoad(AssetLoaderContext assetLoaderContext)
        {
            base.OnLoad(assetLoaderContext);
            ResetModelScale();
            _camerasDropdown.options.Clear();
            PlaybackAnimation.options.Clear();
            _cameras = null;
            _animation = null;
            _mainCamera.enabled = true;
            if (assetLoaderContext.RootGameObject != null)
            {
                if (assetLoaderContext.Options.ImportCameras)
                {
                    _cameras = assetLoaderContext.RootGameObject.GetComponentsInChildren<Camera>();
                    if (_cameras.Count > 0)
                    {
                        _camerasDropdown.gameObject.SetActive(true);
                        _camerasDropdown.options.Add(new Dropdown.OptionData("User Camera"));
                        for (var i = 0; i < _cameras.Count; i++)
                        {
                            var camera = _cameras[i];
                            camera.enabled = false;
                            _camerasDropdown.options.Add(new Dropdown.OptionData(camera.name));
                        }
                        _camerasDropdown.captionText.text = _cameras[0].name;
                    }
                    else
                    {
                        _cameras = null;
                    }
                }
                _animation = assetLoaderContext.RootGameObject.GetComponent<Animation>();
                if (_animation != null)
                {
                    _animations = _animation.GetAllAnimationClips();
                    if (_animations.Count > 0)
                    {
                        PlaybackAnimation.interactable = true;
                        for (var i = 0; i < _animations.Count; i++)
                        {
                            var animationClip = _animations[i];
                            PlaybackAnimation.options.Add(new Dropdown.OptionData(animationClip.name));
                        }
                        PlaybackAnimation.captionText.text = _animations[0].name;
                    }
                    else
                    {
                        _animation = null;
                    }
                }
                _camerasDropdown.value = 0;
                PlaybackAnimation.value = 0;
                StopAnimation();
                RootGameObject = assetLoaderContext.RootGameObject;
            }
            if (_cameras == null)
            {
                _camerasDropdown.gameObject.SetActive(false);
            }
            if (_animation == null)
            {
                PlaybackAnimation.interactable = false;
                PlaybackAnimation.captionText.text = "No Animations";
            }
            ModelTransformChanged();
        }

        /// <summary>
        /// Changes the camera placement when the Model has changed.
        /// </summary>
        protected virtual void ModelTransformChanged()
        {
            if (RootGameObject != null && _mainCamera.enabled)
            {
                var bounds = RootGameObject.CalculateBounds();
                _mainCamera.FitToBounds(bounds, CameraDistanceRatio);
                // Uncomment this code to scale up small objects
                //if (bounds.size.magnitude < 1f)
                //{
                //    var increase = 1f / bounds.size.magnitude;
                //    RootGameObject.transform.localScale *= increase;
                //    bounds = RootGameObject.CalculateBounds();
                //}
                CameraDistance = _mainCamera.transform.position.magnitude;
                CameraPivot = bounds.center;
                Skybox.transform.localScale = bounds.size.magnitude * SkyboxScale * Vector3.one;
                InputMultiplier = bounds.size.magnitude * InputMultiplierRatio;
                CameraAngle = Vector2.zero;
            }
        }

        /// <summary>
        /// Event is triggered when any error occurs.
        /// </summary>
        /// <param name="contextualizedError">The Contextualized Error that has occurred.</param>
        protected override void OnError(IContextualizedError contextualizedError)
        {
            base.OnError(contextualizedError);
            StopAnimation();
            _stopwatch?.Stop();
        }

        /// <summary>Event is triggered when the Model (including Textures and Materials) has been fully loaded.</summary>
        /// <param name="assetLoaderContext">The Asset Loader Context reference. Asset Loader Context contains the Model loading data.</param>
        protected override void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
        {
            base.OnMaterialsLoad(assetLoaderContext);
            _stopwatch.Stop();
#if TRILIB_SHOW_MEMORY_USAGE
            var loadedText = $"Loaded in: {_stopwatch.Elapsed.Minutes:00}:{_stopwatch.Elapsed.Seconds:00} Peak Memory Usage: {ProcessUtils.SizeSuffix(RuntimeProcessUtils.GetProcessMemory() - _initialMemory)}";
#else
            var loadedText = $"Loaded in: {_stopwatch.Elapsed.Minutes:00}:{_stopwatch.Elapsed.Seconds:00}";
#endif
            _loadingTimeText.text = loadedText;
            Debug.Log(loadedText);
            ModelTransformChanged();
        }
    }
}