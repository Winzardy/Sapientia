using System;

namespace SharedLogic
{
	[Serializable]
	public struct SharedLogicConfigurationEntry
	{
#if CLIENT
		[UnityEngine.SerializeReference]
#endif
		public ISharedNodesRegistrar registrar;
	}
}
