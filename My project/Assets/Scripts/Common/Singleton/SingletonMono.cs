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
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = FindObjectOfType<T>();
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
                gameObject.name = typeof(T).Name;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                Debug.LogWarning($"[{typeof(T).Name}] 检测到重复实例，已自动销毁！");
            }
        }

        protected virtual void OnDestroy()
        {
            isDestroyed = true;
            instance = null;
        }

        // 防止在编辑器模式下退出时报错
        protected virtual void OnApplicationQuit()
        {
            isDestroyed = false;
            instance = null;
        }
    }
}