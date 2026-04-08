using UnityEngine;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// Generic singleton base class for MonoBehaviour.
    /// Inherit from this to make any manager a persistent, globally accessible singleton.
    /// Persists across scene loads via DontDestroyOnLoad.
    /// </summary>
    public class MonoSingletongeneric<T> : MonoBehaviour where T : MonoSingletongeneric<T>
    {
        private static T instance;

        /// <summary>
        /// The singleton instance. Returns null if no instance exists yet.
        /// </summary>
        public static T Instance { get { return instance; } }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = (T)this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}