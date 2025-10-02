using System;
using UnityEngine.Serialization;
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
		[FormerlySerializedAs("dataHandlerFactory")]
		[Sirenix.OdinInspector.LabelText("Data Handler")]
#endif
		[SerializeReference]
		public ISharedDataSerializerFactory dataSerializerFactory;
	}
}
