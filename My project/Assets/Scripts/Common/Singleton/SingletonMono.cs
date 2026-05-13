using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MySinleton
{
    public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        static volatile T instance;
        private static readonly object _lock = new object();
        private static bool isDestroyed = false;
        private static bool isApplicationQuitting = false;
        protected bool IsValidSingleton => instance == this;
        public static GameObject GameObject => Instance?.gameObject;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (isApplicationQuitting)
                        {
                            return null;
                        }
                        if (instance == null)
                        {
                            GameObject go = new GameObject(typeof(T).Name);
                            instance = go.AddComponent<T>();
                            DontDestroyOnLoad(go);
                        }
                        else
                        {
                            instance.gameObject.name = typeof(T).Name;
                        }
                    }
                }
                return instance;
            }
        }
        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                Debug.LogWarning($"[{typeof(T).Name}] 检测到重复实例，已自动销毁！");
                return;
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                isDestroyed = true;
                instance = null;
            }
        }

        // 防止在编辑器模式下退出时报错
        protected virtual void OnApplicationQuit()
        {
            isApplicationQuitting = true;
            isDestroyed = false;
            instance = null;
        }
    }
}