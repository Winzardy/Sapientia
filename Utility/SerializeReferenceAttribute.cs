#if !CLIENT
using System;
using System.Diagnostics;

/// Надоело везде писать UnityEngine.SerializeReference и оборачивать в дефайны,
/// но писать вначале всеравно нужно...
/// <code>
/// #if CLIENT
/// using UnityEngine;
/// #endif
/// </code>
[Conditional("CLIENT")]
public class SerializeReferenceAttribute : Attribute
{
}
#endif
