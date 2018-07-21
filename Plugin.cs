using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Text;
using IllusionPlugin;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

namespace CustomAvatar
{
    public class Plugin : IPlugin
    {
        public string Name
        {
            get { return "Custom Avatars"; }
        }

        public string Version
        {
            get { return "2.0"; }
        }

        public delegate void AvatarChangeAction(string path);
        public static event AvatarChangeAction OnAvatarChanged;

        private static List<string> _avatarPaths;
        public static string _currentAvatarPath;
        private static AvatarScript _currentAvatar;

        private AvatarScript tmpAvatar = null;
        private string tmpPath = "";

        public static bool fpsAvatar = false;

        private bool _init;
        public static GameObject MultiGameObject;
        public static void Log(string data)
        {
            Console.WriteLine("[Avatar Plugin] " + data);
            File.AppendAllText(@"MultiLog.txt", "[Avatar Plugin] " + data + Environment.NewLine);
        }

        public void OnApplicationStart()
        {
            if (_init) return;
            _init = true;

            fpsAvatar = PlayerPrefs.GetInt("fpsAvatar",0) == 1;

            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;

            var avatars = RetrieveCustomAvatars();
            if (avatars.Count == 0)
            {
                Plugin.Log("No custom avatars found.");
                return;
            }
            
            _currentAvatarPath = PlayerPrefs.GetString("lastAvatar", null);
            if (_currentAvatarPath == null || !avatars.Contains(_currentAvatarPath))
            {
                _currentAvatarPath = avatars[0];
            }
        }

        public void OnApplicationQuit()
        {
            SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
        }

        private void SceneManagerOnActiveSceneChanged(Scene arg0, Scene scene)
        {
            GameObject origin = GameObject.Find("Origin");

            if (origin != null)
            {
                LoadNewAvatar(_currentAvatarPath);
            }
            else
            {
                Plugin.Log("Origin not found");
            }
        }

        public static List<string> RetrieveCustomAvatars()
        {
           
            _avatarPaths = (Directory.GetFiles(Path.Combine(Application.dataPath, "../CustomAvatars/"),
                "*.avatar", SearchOption.AllDirectories).ToList());
            Plugin.Log("Found " + _avatarPaths.Count + " avatars");
            return _avatarPaths;
        }

        public void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                if (GameObject.Find("Origin") == null) return;
                RetrieveCustomAvatars();
                if (_avatarPaths.Count == 1) return;
                var index = _avatarPaths.IndexOf(_currentAvatarPath);
                index = (index + 1) % _avatarPaths.Count;

                var newAvatar = _avatarPaths[index];
                LoadNewAvatar(newAvatar);
                if (OnAvatarChanged != null) OnAvatarChanged(newAvatar);
            }
            else if (Input.GetKeyDown(KeyCode.PageDown))
            {
                if (GameObject.Find("Origin") == null) return;
                RetrieveCustomAvatars();
                if (_avatarPaths.Count == 1) return;
                var index = _avatarPaths.IndexOf(_currentAvatarPath);
                index -= 1;
                if (index < 0)
                {
                    index = _avatarPaths.Count - 1;
                }

                var newAvatar = _avatarPaths[index];
                LoadNewAvatar(newAvatar);
                if (OnAvatarChanged != null) OnAvatarChanged(newAvatar);
            }

            if (Input.GetKeyDown(KeyCode.Home))
            {
                fpsAvatar = !fpsAvatar;
                PlayerPrefs.SetInt("fpsAvatar", Convert.ToInt32(fpsAvatar));
                LoadNewAvatar(_currentAvatarPath);
            }
        }

        public void LoadNewAvatar(string path)
        {
            tmpAvatar = AvatarScript.SpawnAvatar(path, false);
            tmpPath = path;
            if (tmpAvatar != null)
            {
                tmpAvatar.OnAvatarLoaded += AvatarLoaded;
                if (_currentAvatar != null)
                {
                    _currentAvatar.UnloadAvatar();
                    UnityEngine.Object.Destroy(_currentAvatar.gameObject);
                }
            }
        }

        private void AvatarLoaded()
        {
            Plugin.Log("avatar loaded");
            if (tmpAvatar != null)
            {
                tmpAvatar.HideFromView(true);
                GameObject gameObject = GameObject.Find("Origin");
                tmpAvatar.gameObject.transform.parent = gameObject.transform;
                tmpAvatar.gameObject.transform.localPosition = Vector3.zero;
                tmpAvatar.gameObject.transform.localRotation = Quaternion.identity;
                PlayerPrefs.SetString("lastAvatar", _currentAvatarPath);
                _currentAvatar = this.tmpAvatar;
                _currentAvatarPath = this.tmpPath;
            }
            else
            {
                Plugin.Log("avatar == null");
            }
        }

        public void OnFixedUpdate()
        {

        }

        public void OnLevelWasInitialized(int level)
        {


        }

        public void OnLevelWasLoaded(int level)
        {

        }
    }
}