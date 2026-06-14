using Sapientia.Data;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Указатель карты (Map) на данные одного In/Out — в каком регионе они лежат + как до них добраться.
	/// <list type="bullet">
	/// <item><b>Static</b> — данные в блобе: <see cref="data"/> это <b>прямая self-relative ссылка</b>
	/// (<c>data.GetPtr()</c>); резолв делается <b>на месте</b> через ref слота в пуле (на копии адрес сломается).</item>
	/// <item><b>Cache</b>/<b>Persistence</b> — данные в Runtime-памяти: <c>data.byteOffset</c> это офсет слайса
	/// в блоке региона; резолв <c>база региона + офсет</c> делает владелец Runtime-памяти.</item>
	/// </list>
	/// In пишет указатель на данные своего источника (того же Out); Out — на собственную ячейку.
	/// </summary>
	public struct RegionPtr
	{
		public MemoryRegion region;
		public RelativePtr<byte> data;
	}
}
