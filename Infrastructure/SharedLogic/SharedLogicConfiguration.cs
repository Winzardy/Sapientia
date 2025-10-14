using System;

#if CLIENT
using Sirenix.OdinInspector;
using UnityEngine.Serialization;
using UnityEngine;
#endif

namespace SharedLogic
{
	[Serializable]
	public partial struct SharedLogicConfiguration
	{
#if CLIENT
		[Title("Server")]
		[LabelText("Registrar")]
#endif
		[SerializeReference]
		public ISharedNodesRegistrarFactory registrarFactory;

#if CLIENT
		[LabelText("Data Streamer")]
#endif
		[SerializeReference]
		public ISharedDataStreamerFactory dataStreamerFactory;
	}
}
