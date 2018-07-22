using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using AvatarScriptPack;

namespace CustomAvatar
{
    public class AvatarScript : MonoBehaviour
    {
        private AssetBundle _customAvatar;
        private GameObject _avatarInstance;
        private GameObject _fpsAvatarInstance;
        private AvatarBodyManager _bodyManager;
        private bool _manualControl;
        private AssetBundleCreateRequest _bundleRequest;

        public delegate void AvatarLoadAction();
        public event AvatarLoadAction OnAvatarLoaded;

        public static AvatarScript SpawnAvatar(string path, bool manual)
        {
            var avatarObject = new GameObject("Avatar Root");
            var script = avatarObject.AddComponent<AvatarScript>();
            script.LoadAvatar(path, manual);
            return script;
        }

        public void LoadAvatar(string path, bool manual)
        {
            _manualControl = manual;
            _bundleRequest = AssetBundle.LoadFromFileAsync(path);
            _bundleRequest.completed += AvatarLoaded;
        }

        public void LoadAvatar(byte[] file, bool manual, bool multiPrefab)
        {
            _manualControl = manual;
            if (multiPrefab)
            {
                LoadMultiPrefab();
            }
            else
            {
                _bundleRequest = AssetBundle.LoadFromMemoryAsync(file);
                _bundleRequest.completed += AvatarLoaded;
            }
        }

        private void AvatarLoaded(AsyncOperation op)
        {
            _customAvatar = _bundleRequest.assetBundle;

            if (_customAvatar == null)
            {
                Plugin.Log("The bundle file is wrong");
                UnloadAvatar();
                Destroy(gameObject);
                return;
            }

            GameObject avatarObject = _customAvatar.LoadAsset<GameObject>("_customavatar");

            if (avatarObject == null)
            {
                Plugin.Log("The bundle gameobject was not found");
                UnloadAvatar();
                Destroy(gameObject);
                return;
            }
            
            _avatarInstance = Instantiate(avatarObject, gameObject.transform);

            if (Plugin.fpsAvatar) _fpsAvatarInstance = Instantiate(avatarObject, gameObject.transform);

            SetupAvatar();

            UnloadAvatar();

            _bundleRequest = null;

            if (OnAvatarLoaded != null) OnAvatarLoaded.Invoke();
        }

        public void UnloadAvatar()
        {
            if (_customAvatar != null)
            {
                _customAvatar.Unload(false);
            }
        }

        private void SetupAvatar()
        {
            var curScene = SceneManager.GetActiveScene();
            if (curScene.buildIndex != 1)
            {
                _avatarInstance.AddComponent<AvatarEventsPlayer>();
                if (_fpsAvatarInstance!=null) _fpsAvatarInstance.AddComponent<AvatarEventsPlayer>();
            }
            _bodyManager = _avatarInstance.AddComponent<AvatarBodyManager>();
            _bodyManager.manual = _manualControl;

            if (_fpsAvatarInstance != null)
            {
                var tempBody = _fpsAvatarInstance.AddComponent<AvatarBodyManager>();
                tempBody.manual = _manualControl;
                SetLayerToChilds(_fpsAvatarInstance.transform, 27);
                
                var comp = _fpsAvatarInstance.GetComponentInChildren<VRIK>();
                if (comp != null)
                {
                    comp.AutoDetectReferences();
                    Transform neckTf = comp.references.GetTransforms()[4];
                    
                    if (neckTf != null)
                    {
                        neckTf.localScale = Vector3.zero;
                    }
                }

                var renderers = tempBody.GetHeadTransform().gameObject.GetComponentsInChildren<Renderer>();
                foreach(Renderer renderer in renderers)
                {
                    renderer.enabled = false;
                }
            }

            SetupCams();
        }

        public GameObject getInstance()
        {
            return _avatarInstance;
        }

        public AvatarBodyManager GetBodyManager()
        {
            return _bodyManager;
        }

        public AssetBundleCreateRequest GetBundleRequest()
        {
            return _bundleRequest;
        }

        public void HideFromView(bool hide)
        {
            if (_avatarInstance != null)
            {
                SetLayerToChilds(_avatarInstance.transform, hide ? 24 : 0);

                if (_fpsAvatarInstance != null) SetLayerToChilds(_fpsAvatarInstance.transform, 27);
            }
        }

        private static void SetLayerToChilds(Transform origin, int layer)
        {
            foreach (Transform child in origin)
            {
                child.gameObject.layer = layer;
                SetLayerToChilds(child, layer);
            }
        }

        private static void SetupCams()
        {
            var cameras = FindObjectsOfType<Camera>();
            foreach(var cam in cameras)
            {
                cam.cullingMask &= ~(1 << 27);
                // layer 26 for culling the exclusions
                cam.cullingMask &= ~(1 << 26);
            }

            var mainCamera = FindObjectsOfType<Camera>().FirstOrDefault(x => x.CompareTag("MainCamera"));

            mainCamera.cullingMask &= ~(1 << 24);
            // layer 26 for culling the exclusions
            mainCamera.cullingMask &= ~(1 << 26);
            mainCamera.cullingMask |= 1 << 27;
        }

        private void OnDestroy()
        {
            UnloadAvatar();
        }

        #region Multiplayer
        public void LoadDefaultMultiPrefab(byte[] file)
        {
            StartCoroutine(InitializeMultiPrefab(file));
        }
        IEnumerator InitializeMultiPrefab(byte[] file)
        {
            if (Plugin.MultiGameObject == null)
            {
                var bundleLoadRequest = AssetBundle.LoadFromMemoryAsync(file);
                yield return bundleLoadRequest;
                var bundle = bundleLoadRequest.assetBundle;

                if (bundleLoadRequest.isDone && bundle != null)
                {
                    var assetLoadRequest = bundle.LoadAssetAsync<GameObject>("_customavatar");
                    yield return assetLoadRequest;
                    Plugin.MultiGameObject = assetLoadRequest.asset as GameObject;
                    if (Plugin.MultiGameObject != null)
                    {
                        Plugin.Log("Finished loading multi game object, it isn't null");
                    }
                }

                if (bundleLoadRequest.isDone && bundle == null)
                {
                    Plugin.Log("Asset bundle null");
                }
            }
        }
        private void LoadMultiPrefab()
        {
            if (Plugin.MultiGameObject == null)
            {
                Plugin.Log("Multi Game object is null");
                return;
            }
            _avatarInstance = Instantiate(Plugin.MultiGameObject, gameObject.transform);

            if (Plugin.fpsAvatar) _fpsAvatarInstance = Instantiate(Plugin.MultiGameObject, gameObject.transform);
            SetupAvatar();
            UnloadAvatar();
            _bundleRequest = null;
            if (OnAvatarLoaded != null) OnAvatarLoaded.Invoke();
        }
        #endregion

    }
}
