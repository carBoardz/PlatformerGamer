using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MySinleton
{
    public class Singleton<T> where T : class, new()
    {
        static object _Lock = new object();
        static volatile T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_Lock)
                    {
                        if (instance == null)
                        {
                            instance = new T();
                            Debug.Log($"{typeof(T).Name}的单例创建完成");
                        }
                    }
                }
                return instance;
            }
        }
        protected Singleton() { }
    }
}
