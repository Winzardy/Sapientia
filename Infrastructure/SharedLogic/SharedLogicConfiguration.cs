using System;
#if CLIENT
using UnityEngine;
#endif

namespace SharedLogic
{
	[Serializable]
	public partial struct SharedLogicConfiguration
	{
#if CLIENT
		[Sirenix.OdinInspector.Title("Server")]
		[Sirenix.OdinInspector.LabelText("Registrar")]
#endif
		[SerializeReference]
		public ISharedNodesRegistrarFactory registrarFactory;

#if CLIENT
		[Sirenix.OdinInspector.LabelText("Data Handler")]
#endif
		[SerializeReference]
		public ISharedDataSerializerFactory dataHandlerFactory;
	}
}
