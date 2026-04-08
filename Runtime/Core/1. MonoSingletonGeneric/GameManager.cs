using UnityEngine;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// Example GameManager singleton.
    /// Inherits MonoSingletongeneric so it persists across scenes
    /// and is accessible globally via GameManager.Instance.
    /// </summary>
    public class GameManager : MonoSingletongeneric<GameManager>
    {
        protected override void Awake()
        {
            base.Awake();
        }
    }
}
