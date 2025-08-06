#if HE_SYSCORE

using HeathenEngineering.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeathenEngineering.UX.API
{
    public static class Scenes
    {
        public static UnitySceneProcessStateEvent EventStarted => evtStarted;
        public static UnitySceneProcessStateEvent EventUpdated => evtUpdated;
        public static UnitySceneProcessStateEvent EventCompleted => evtCompleted;

        private static UnitySceneProcessStateEvent evtStarted = new UnitySceneProcessStateEvent();
        private static UnitySceneProcessStateEvent evtUpdated = new UnitySceneProcessStateEvent();
        private static UnitySceneProcessStateEvent evtCompleted = new UnitySceneProcessStateEvent();

        /// <summary>
        /// List the scenes available to be loaded
        /// </summary>
        public static Scene[] AvailableScenes
        {
            get
            {
                Scene[] results = new Scene[SceneManager.sceneCountInBuildSettings];
                for (int i = 0; i < results.Length; i++)
                {
                    results[i] = SceneManager.GetSceneByBuildIndex(i);
                }

                return results;
            }
        }
        /// <summary>
        /// Creates a list reference of all the scenes that are currently loaded.
        /// </summary>
        public static Scene[] LoadedScenes
        {
            get
            {
                Scene[] results = new Scene[SceneManager.sceneCount];
                for (int i = 0; i < results.Length; i++)
                {
                    results[i] = SceneManager.GetSceneAt(i);
                }

                return results;
            }
        }

        internal static ScenesManager behaviour;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            behaviour = null;
            evtStarted = new UnitySceneProcessStateEvent();
            evtUpdated = new UnitySceneProcessStateEvent();
            evtCompleted = new UnitySceneProcessStateEvent();
        }

        /// <summary>
        /// Returns true if a scene with this name is loaded
        /// </summary>
        /// <param name="sceneName">The name of the scene to test</param>
        /// <returns></returns>
        public static bool IsSceneLoaded(string sceneName) => SceneManager.GetSceneByName(sceneName).isLoaded;

        /// <summary>
        /// Returns true if the scene at this build index is loaded
        /// </summary>
        /// <param name="buildIndex">The build index of the scene to test</param>
        /// <returns></returns>
        public static bool IsSceneLoaded(int buildIndex) => SceneManager.GetSceneByBuildIndex(buildIndex).isLoaded;

        /// <summary>
        /// Sets the indicated scene as active if its loaded.
        /// </summary>
        /// <param name="buildIndex">The build index of the scene to set active</param>
        public static void SetSceneActive(int buildIndex)
        {
            if (buildIndex < 0)
            {
                Debug.LogWarning("Attempted to set an invalid scene index as active. No Action Was Taken!");
                return;
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(buildIndex));
        }

        public static void SetSceneActive(Scene scene) => SceneManager.SetActiveScene(scene);

        /// <summary>
        /// Unloads one set of scenes while loading another
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="setActive"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static void Transition(string from, string to, bool setActive, Action<SceneProcessState> callback = null)
        {
            if(behaviour == null)
            {
                Debug.LogError("Attempted to call Scenes.Transition with a ScenesManager present. No action was taken");
                callback?.Invoke(new SceneProcessState { hasError = true, errorMessage = "Attempted to call Scenes.Transition with a ScenesManager present. No action was taken" });
                return;
            }

            var load = new string[] { to };

            var unload = new string[] { from };

            behaviour.StartCoroutine(ProcessState(unload, load, setActive ? to : null, callback));
        }
        /// <summary>
        /// Unloads one set of scenes while loading another
        /// </summary>
        public static void Transition(IEnumerable<string> from, IEnumerable<string> to, string setActive = null, Action<SceneProcessState> callback = null)
        {
            if (behaviour == null)
            {
                Debug.LogError("Attempted to call Scenes.Transition with a ScenesManager present. No action was taken");
                callback?.Invoke(new SceneProcessState { hasError = true, errorMessage = "Attempted to call Scenes.Transition with a ScenesManager present. No action was taken" });
                return;
            }

            behaviour.StartCoroutine(ProcessState(from.ToArray(), to.ToArray(), setActive, callback));
        }
        /// <summary>
        /// Unloads one set of scenes while loading another
        /// </summary>
        /// <remarks>
        /// This unloads the currently loaded scenes
        /// </remarks>
        public static void Transition(string to, Action<SceneProcessState> callback = null)
        {
            if (behaviour == null)
            {
                Debug.LogError("Attempted to call Scenes.Transition with a ScenesManager present. No action was taken");
                callback?.Invoke(new SceneProcessState { hasError = true, errorMessage = "Attempted to call Scenes.Transition with a ScenesManager present. No action was taken" });
                return;
            }

            var current = LoadedScenes;
            var from = new string[current.Length];
            for (int i = 0; i < from.Length; i++)
            {
                from[i] = current[i].name;
            }

            behaviour.StartCoroutine(ProcessState(from, new string[] { to }, to, callback));
        }
        /// <summary>
        /// Unloads one set of scenes while loading another
        /// </summary>
        /// <remarks>
        /// This unloads the currently loaded scenes
        /// </remarks>
        public static void Transition(IEnumerable<string> to, string setActive, Action<SceneProcessState> callback = null)
        {
            if (behaviour == null)
            {
                Debug.LogError("Attempted to call Scenes.Transition with a ScenesManager present. No action was taken");
                callback?.Invoke(new SceneProcessState { hasError = true, errorMessage = "Attempted to call Scenes.Transition with a ScenesManager present. No action was taken" });
                return;
            }

            var current = LoadedScenes;
            var from = new string[current.Length];
            for (int i = 0; i < from.Length; i++)
            {
                from[i] = current[i].name;
            }

            behaviour.StartCoroutine(ProcessState(from, to.ToArray(), setActive, callback));
        }
        /// <summary>
        /// Unloads one set of scenes while loading another
        /// </summary>
        public static void Transition(int from, int to, bool setToAsActive = false, Action<SceneProcessState> callback = null)
        {
            if (behaviour == null)
            {
                Debug.LogError("Attempted to call Scenes.Transition with a ScenesManager present. No action was taken");
                callback?.Invoke(new SceneProcessState { hasError = true, errorMessage = "Attempted to call Scenes.Transition with a ScenesManager present. No action was taken" });
                return;
            }

            SceneProcessState nState = new SceneProcessState()
            {
                loadTargets = new List<int>(to),
                unloadTargets = new List<int>(from),
                setActiveScene = setToAsActive ? to : -1
            };

            behaviour.StartCoroutine(ProcessState(nState, callback));
        }
        /// <summary>
        /// Unloads one set of scenes while loading another
        /// </summary>
        public static void Transition(IEnumerable<int> from, IEnumerable<int> to, int activeScene = -1, Action<SceneProcessState> callback = null)
        {
            if (behaviour == null)
            {
                Debug.LogError("Attempted to call Scenes.Transition with a ScenesManager present. No action was taken");
                callback?.Invoke(new SceneProcessState { hasError = true, errorMessage = "Attempted to call Scenes.Transition with a ScenesManager present. No action was taken" });
                return;
            }

            SceneProcessState nState = new SceneProcessState()
            {
                loadTargets = new List<int>(to),
                unloadTargets = new List<int>(from),
                setActiveScene = activeScene
            };

            behaviour.StartCoroutine(ProcessState(nState, callback));
        }
        /// <summary>
        /// Additivly loads the indicated scene
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="setActive"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static void Load(string scene, bool setActive, Action<SceneProcessState> callback = null)
        {
            if (behaviour == null)
            {
                Debug.LogError("Attempted to call Scenes.Load with a ScenesManager present. No action was taken");
                callback?.Invoke(new SceneProcessState { hasError = true, errorMessage = "Attempted to call Scenes.Load with a ScenesManager present. No action was taken" });
                return;
            }

            behaviour.StartCoroutine(ProcessState(null, new string[] { scene }, setActive ? scene : null, callback));
        }
        /// <summary>
        /// Additivly loads scenes
        /// </summary>
        /// <param name="scenes"></param>
        /// <param name="setActive"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static void Load(IEnumerable<string> scenes, string setActive = null, Action<SceneProcessState> callback = null)
        {
            if (behaviour == null)
            {
                Debug.LogError("Attempted to call Scenes.Load with a ScenesManager present. No action was taken");
                callback?.Invoke(new SceneProcessState { hasError = true, errorMessage = "Attempted to call Scenes.Load with a ScenesManager present. No action was taken" });
                return;
            }

            behaviour.StartCoroutine(ProcessState(null, scenes.ToArray(), setActive, callback));
        }
        /// <summary>
        /// Loads a scene and optioanlly sets it as active
        /// </summary>
        /// <param name="scene">The build index of the scene to load</param>
        /// <param name="setActive">Should the scene be set active when loaded</param>
        /// <param name="callback">This is called when the process completes</param>
        public static void Load(int scene, bool setActive = false, Action<SceneProcessState> callback = null)
        {
            if (behaviour == null)
            {
                Debug.LogError("Attempted to call Scenes.Load with a ScenesManager present. No action was taken");
                callback?.Invoke(new SceneProcessState { hasError = true, errorMessage = "Attempted to call Scenes.Load with a ScenesManager present. No action was taken" });
                return;
            }

            if (scene >= 0 && scene < SceneManager.sceneCountInBuildSettings)
            {
                Load(new int[] { scene }, (setActive ? scene : -1), callback);
            }
            else
            {
                Debug.LogError("Attempted to load a scene by index, invalid scene index (" + scene + ") provided, no action taken!");
            }
        }
        /// <summary>
        /// Loads multiple scenes optionally setting one as active and calling an action when complete
        /// </summary>
        /// <param name="scenes">The scenes to load</param>
        /// <param name="activeScene">The scene to set active ... if -1 no scene will be set active</param>
        /// <param name="callback">The action to call when the process is complete</param>
        public static void Load(IEnumerable<int> scenes, int activeScene = -1, Action<SceneProcessState> callback = null)
        {
            if (behaviour == null)
            {
                Debug.LogError("Attempted to call Scenes.Load with a ScenesManager present. No action was taken");
                callback?.Invoke(new SceneProcessState { hasError = true, errorMessage = "Attempted to call Scenes.Load with a ScenesManager present. No action was taken" });
                return;
            }

            var exit = false;

            foreach (var scene in scenes)
            {
                if (scene < 0 || scene >= SceneManager.sceneCountInBuildSettings)
                {
                    Debug.LogError("Attempted to load a scene by index, invalid scene (" + scene + ") provided, no action taken!");
                    exit = true;
                }
            }

            if (!exit)
            {
                if (activeScene >= SceneManager.sceneCountInBuildSettings)
                {
                    Debug.LogError("Attempted to load and activate a scene by index, invalid scene (" + activeScene + ") provided, no action taken!");
                    exit = true;
                }

                if (!exit)
                {
                    var nState = new SceneProcessState()
                    {
                        unloadTargets = new List<int>(),
                        loadTargets = new List<int>(scenes),
                        setActiveScene = activeScene,
                    };

                    behaviour.StartCoroutine(ProcessState(nState, callback));
                }
            }
        }

        public static void Unload(string scene, Action<SceneProcessState> callback = null)
        {
            if (behaviour == null)
            {
                Debug.LogError("Attempted to call Scenes.Unload with a ScenesManager present. No action was taken");
                callback?.Invoke(new SceneProcessState { hasError = true, errorMessage = "Attempted to call Scenes.Unload with a ScenesManager present. No action was taken" });
                return;
            }

            behaviour.StartCoroutine(ProcessState(new string[] { scene }, null, null, callback));
        }

        public static void Unload(IEnumerable<string> scenes, Action<SceneProcessState> callback = null)
        {
            if (behaviour == null)
            {
                Debug.LogError("Attempted to call Scenes.Unload with a ScenesManager present. No action was taken");
                callback?.Invoke(new SceneProcessState { hasError = true, errorMessage = "Attempted to call Scenes.Unload with a ScenesManager present. No action was taken" });
                return;
            }

            behaviour.StartCoroutine(ProcessState(scenes.ToArray(), null, null, callback));
        }

        /// <summary>
        /// Unloads the indicated scene and calls the indicated action when complete
        /// </summary>
        /// <param name="scene">The scene to unload</param>
        /// <param name="callback">This is called when the process is complete</param>
        public static void Unload(int scene, Action<SceneProcessState> callback = null)
        {
            if (behaviour == null)
            {
                Debug.LogError("Attempted to call Scenes.Unload with a ScenesManager present. No action was taken");
                callback?.Invoke(new SceneProcessState { hasError = true, errorMessage = "Attempted to call Scenes.Unload with a ScenesManager present. No action was taken" });
                return;
            }

            Unload(new int[] { scene }, callback);
        }

        /// <summary>
        /// Unloads multiple scenes if they are loaded and calls the indicated action when the process is complete
        /// </summary>
        /// <param name="scenes">The scenes to be unloaded</param>
        /// <param name="callback">The action to call when complete</param>
        public static void Unload(IEnumerable<int> scenes, Action<SceneProcessState> callback = null)
        {
            if (behaviour == null)
            {
                Debug.LogError("Attempted to call Scenes.Unload with a ScenesManager present. No action was taken");
                callback?.Invoke(new SceneProcessState { hasError = true, errorMessage = "Attempted to call Scenes.Unload with a ScenesManager present. No action was taken" });
                return;
            }

            var nState = new SceneProcessState()
            {
                unloadTargets = new List<int>(scenes),
                loadTargets = new List<int>(),
                setActiveScene = -1,
            };

            behaviour.StartCoroutine(ProcessState(nState, callback));
        }

        private static IEnumerator ProcessState(string[] from, string[] to, string toActive, Action<SceneProcessState> callback)
        {
            SceneProcessState state = new SceneProcessState();

            state.loadProgress = 0;
            state.unloadProgress = 0;
            state.transitionProgress = 0;
            state.complete = false;
            state.hasError = false;

            evtStarted.Invoke(state);

            callback?.Invoke(state);

            yield return new WaitForEndOfFrame();

            List<AsyncOperation> loadOperations = new List<AsyncOperation>();
            List<AsyncOperation> unloadOperations = new List<AsyncOperation>();

            if (to != null)
            {
                foreach (var scene in to)
                {
                    if (!IsSceneLoaded(scene))
                    {
                        var op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
                        op.allowSceneActivation = true;
                        loadOperations.Add(op);
                    }
                }
            }

            if (from != null)
            {
                foreach (var scene in from)
                {
                    if (IsSceneLoaded(scene))
                    {
                        var op = SceneManager.UnloadSceneAsync(scene);
                        op.allowSceneActivation = true;
                        unloadOperations.Add(op);
                    }
                }
            }

            int loadCount = loadOperations.Count;
            int unloadCount = unloadOperations.Count;

            while (loadOperations.Count > 0 || unloadOperations.Count > 0)
            {
                loadOperations.RemoveAll(p => p.isDone);
                unloadOperations.RemoveAll(p => p.isDone);

                float loadProgress = loadCount - loadOperations.Count;
                float unloadProgress = unloadCount - unloadOperations.Count;

                foreach (var op in loadOperations)
                {
                    loadProgress += op.progress;
                }

                foreach (var op in unloadOperations)
                {
                    unloadProgress += op.progress;
                }

                if (loadCount > 0)
                    loadProgress = loadProgress / loadCount;
                else
                    loadProgress = 1f;

                if (unloadCount > 0)
                    unloadProgress = unloadProgress / unloadCount;
                else
                    unloadProgress = 1f;

                state.loadProgress = loadProgress;
                state.unloadProgress = unloadProgress;
                if (loadCount > 0 && unloadCount > 0)
                    state.transitionProgress = (loadProgress + unloadProgress) / 2f;
                else if (loadCount > 0)
                    state.transitionProgress = loadProgress;
                else
                    state.transitionProgress = unloadProgress;

                evtUpdated.Invoke(state);

                callback?.Invoke(state);

                yield return new WaitForEndOfFrame();
            }

            if (!string.IsNullOrEmpty(toActive))
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(toActive));

            state.complete = true;

            evtCompleted.Invoke(state);

            callback?.Invoke(state);
        }

        private static IEnumerator ProcessState(SceneProcessState state, Action<SceneProcessState> callback)
        {
            state.loadProgress = 0;
            state.unloadProgress = 0;
            state.transitionProgress = 0;
            state.complete = false;
            state.hasError = false;

            evtStarted.Invoke(state);

            callback?.Invoke(state);

            //Validate the entries
            if (state.loadTargets != null)
            {
                foreach (var scene in state.loadTargets)
                {
                    if (scene < 0 || SceneManager.sceneCountInBuildSettings <= scene)
                    {
                        state.hasError = true;
                        state.errorMessage += "Load target index (" + scene + ") is out of range.\n";
                    }
                }
            }

            if (state.unloadTargets != null)
            {
                foreach (var scene in state.unloadTargets)
                {
                    if (scene < 0 || SceneManager.sceneCountInBuildSettings <= scene)
                    {
                        state.hasError = true;
                        state.errorMessage += "Unload target index (" + scene + ") is out of range.\n";
                    }
                }
            }

            if (state.setActiveScene >= SceneManager.sceneCountInBuildSettings)
            {
                state.hasError = true;
                state.errorMessage += "The set active scene target (" + state.setActiveScene + ") is invalid.\n";
            }

            if (!state.hasError)
            {
                yield return new WaitForEndOfFrame();

                List<AsyncOperation> loadOperations = new List<AsyncOperation>();
                List<AsyncOperation> unloadOperations = new List<AsyncOperation>();

                if (state.loadTargets != null)
                {
                    foreach (var scene in state.loadTargets)
                    {
                        if (!IsSceneLoaded(scene))
                        {
                            var op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
                            op.allowSceneActivation = true;
                            loadOperations.Add(op);
                        }
                    }
                }

                if (state.unloadTargets != null)
                {
                    foreach (var scene in state.unloadTargets)
                    {
                        if (IsSceneLoaded(scene))
                        {
                            var op = SceneManager.UnloadSceneAsync(scene);
                            op.allowSceneActivation = true;
                            unloadOperations.Add(op);
                        }
                    }
                }

                int loadCount = loadOperations.Count;
                int unloadCount = unloadOperations.Count;

                while (loadOperations.Count > 0 || unloadOperations.Count > 0)
                {
                    loadOperations.RemoveAll(p => p.isDone);
                    unloadOperations.RemoveAll(p => p.isDone);

                    float loadProgress = loadCount - loadOperations.Count;
                    float unloadProgress = unloadCount - unloadOperations.Count;

                    foreach (var op in loadOperations)
                    {
                        loadProgress += op.progress;
                    }

                    foreach (var op in unloadOperations)
                    {
                        unloadProgress += op.progress;
                    }

                    if (loadCount > 0)
                        loadProgress = loadProgress / loadCount;
                    else
                        loadProgress = 1f;

                    if (unloadCount > 0)
                        unloadProgress = unloadProgress / unloadCount;
                    else
                        unloadProgress = 1f;

                    state.loadProgress = loadProgress;
                    state.unloadProgress = unloadProgress;
                    if (loadCount > 0 && unloadCount > 0)
                        state.transitionProgress = (loadProgress + unloadProgress) / 2f;
                    else if (loadCount > 0)
                        state.transitionProgress = loadProgress;
                    else
                        state.transitionProgress = unloadProgress;

                    evtUpdated.Invoke(state);

                    callback?.Invoke(state);

                    yield return new WaitForEndOfFrame();
                }

                if (state.setActiveScene >= 0)
                    SetSceneActive(state.setActiveScene);
            }

            state.complete = true;

            evtCompleted.Invoke(state);

            callback?.Invoke(state);
        }
    }
}


#endif