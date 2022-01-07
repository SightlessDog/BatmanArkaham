namespace GameCreator.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [AddComponentMenu("")]
    public class CoroutinesManager : Singleton<CoroutinesManager>
    {
        #if UNITY_EDITOR

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInit()
        {
            OnRuntimeInitSingleton();
        }
		
        #endif
    }
}