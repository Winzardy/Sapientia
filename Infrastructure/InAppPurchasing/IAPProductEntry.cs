#if DebugLog
#define ENABLE_IAP_EMPTY_CHECK
#endif

using System;
using System.Collections.Generic;
using Sapientia;
using Sapientia.Extensions;

namespace InAppPurchasing
{
	[Serializable]
	public class IAPConsumableProductEntry : IAPProductEntry
	{
		public override IAPProductType Type => IAPProductType.Consumable;
	}

	[Serializable]
	public class IAPNonConsumableProductEntry : IAPProductEntry
	{
		public override IAPProductType Type => IAPProductType.NonConsumable;
	}

	[Serializable]
	public class IAPSubscriptionProductEntry : IAPProductEntry
	{
		public override IAPProductType Type => IAPProductType.Subscription;
	}

	public abstract partial class IAPProductEntry : IExternallyIdentifiable
	{
		/// <summary>
		/// Этот идентификатор служит определением покупки внутри проекта, Id который в итоге используется в <see cref="InAppPurchasing"/> может отличаться!
		/// </summary>
		internal string id;

		public abstract IAPProductType Type { get; }

		/// <summary>
		/// [Normal Priority] Обычно имя совпадает с id, можно установить свое, но также есть Billing -> Id (самый приоритетный)
		/// </summary>
		public bool useCustomId;

		public string customId;

		public Dictionary<IAPBillingEntry, string> billingToId;

		public static implicit operator IAPProductType(IAPProductEntry entry) => entry.Type;
		public static implicit operator bool(IAPProductEntry entry) => entry != null;

		/// <inheritdoc cref="id"/>
		public string Id => id;

		void IExternallyIdentifiable.SetId(string id) => this.id = id;

		public override string ToString()
			=> "IAP Product:\n" +
				$"	Id: {id}\n" +
				$"	Type: {Type}";
	}

	public static class IAPProductEntryExtensions
	{
		public static string GetId(this IAPProductEntry product, in IAPBillingEntry billing)
		{
#if ENABLE_IAP_EMPTY_CHECK
			if (product.id.IsNullOrEmpty())
				throw new Exception("IAP Product Entry id is empty!");
#endif
			if (billing && product.billingToId.TryGetValue(billing, out var name))
				return name;

			return product.useCustomId ? product.customId : product.id;
		}
	}
}
