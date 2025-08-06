#if HE_SYSCORE

using HeathenEngineering.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UX
{
    public class ScenesManager : MonoBehaviour
    {
        [Header("References")]
        #region Game Events
        public SceneProcessStateGameEvent Started; 
        public SceneProcessStateGameEvent Updated;
        public SceneProcessStateGameEvent Completed;
        #endregion

        [Header("Events")]
        #region Unity Events
        public UnitySceneProcessStateEvent evtStarted;
        public UnitySceneProcessStateEvent evtUpdated;
        public UnitySceneProcessStateEvent evtCompleted;
        #endregion

        private void Start()
        {
            API.Scenes.behaviour = this;
            API.Scenes.EventStarted.AddListener(HandleStarted);
            API.Scenes.EventUpdated.AddListener(HandleUpdated);
            API.Scenes.EventCompleted.AddListener(HandleCompleted);
        }

        private void OnDestroy()
        {
            API.Scenes.EventStarted.RemoveListener(HandleStarted);
            API.Scenes.EventUpdated.RemoveListener(HandleUpdated);
            API.Scenes.EventCompleted.RemoveListener(HandleCompleted);

            if (API.Scenes.behaviour == this)
                API.Scenes.behaviour = null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "<Pending>")]
        private void HandleCompleted(SceneProcessState arg0)
        {
            Started?.Invoke(this, arg0);
            evtStarted.Invoke(arg0);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "<Pending>")]
        private void HandleUpdated(SceneProcessState arg0)
        {
            Updated?.Invoke(this, arg0);
            evtUpdated.Invoke(arg0);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "<Pending>")]
        private void HandleStarted(SceneProcessState arg0)
        {
            Completed?.Invoke(this, arg0);
            evtCompleted.Invoke(arg0);
        }

        public static void Transition(string from, string to, bool setActive, Action<SceneProcessState> callback = null) => API.Scenes.Transition(from, to, setActive, callback);

        public static void Transition(List<string> from, List<string> to, string setActive = null, Action<SceneProcessState> callback = null) => API.Scenes.Transition(from, to, setActive, callback);

        public static void Transition(int from, int to, bool setToAsActive = false, Action<SceneProcessState> callback = null) => API.Scenes.Transition(from, to, setToAsActive, callback);

        public static void Transition(IEnumerable<int> from, IEnumerable<int> to, int activeScene = -1, Action<SceneProcessState> callback = null) => API.Scenes.Transition(from, to, activeScene, callback);

        public static void Load(string scene, bool setActive, Action<SceneProcessState> callback = null) => API.Scenes.Load(scene, setActive, callback);

        public static void Load(List<string> scenes, string setActive = null, Action<SceneProcessState> callback = null) => API.Scenes.Load(scenes, setActive, callback);

        public static void Load(int scene, bool setActive = false, Action<SceneProcessState> callback = null) => API.Scenes.Load(scene, setActive, callback);

        public static void Load(IEnumerable<int> scenes, int activeScene = -1, Action<SceneProcessState> callback = null) => API.Scenes.Load(scenes, activeScene, callback);

        public static void Unload(string scene, Action<SceneProcessState> callback = null) => API.Scenes.Unload(scene, callback);

        public static void Unload(List<string> scenes, Action<SceneProcessState> callback = null) => API.Scenes.Unload(scenes, callback);

        public static void Unload(int scene, Action<SceneProcessState> callback = null) => API.Scenes.Unload(scene, callback);

        public static void Unload(IEnumerable<int> scenes, Action<SceneProcessState> callback = null) => API.Scenes.Unload(scenes, callback);

        public static void SetSceneActive(int buildIndex) => API.Scenes.SetSceneActive(buildIndex);
    }
}

#endif