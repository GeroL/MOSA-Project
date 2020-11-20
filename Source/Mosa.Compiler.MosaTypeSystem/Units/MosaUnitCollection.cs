// Copyright (c) MOSA Project. Licensed under the New BSD License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mosa.Compiler.MosaTypeSystem
{
	public class MosaUnitCollection<T, TKey> : KeyedCollection<TKey, T>
	{
		private readonly Func<T, TKey> _getKeyForItem;

		public MosaUnitCollection(Func<T, TKey> getKeyForItem)
		{
			_getKeyForItem = getKeyForItem ?? throw new ArgumentNullException(nameof(getKeyForItem));
		}

		public MosaUnitCollection(Func<T, TKey> getKeyForItem, IEqualityComparer<TKey> equalityComparer)
			: base(equalityComparer)
		{
			_getKeyForItem = getKeyForItem ?? throw new ArgumentNullException(nameof(getKeyForItem));
		}

		public MosaUnitCollection(MosaUnitCollection<T, TKey> other)
		{
			_getKeyForItem = other._getKeyForItem;
			foreach (var item in other)
				Add(item);
		}

		protected override TKey GetKeyForItem(T item) => _getKeyForItem(item);

		/// <summary>
		/// Returns true if item was updated. Returns false if item was added
		/// </summary>
		/// <returns></returns>
		public bool AddOrUpdate(T item)
		{
			if (Contains(item))
			{
				Remove(item);
				Add(item);
				return true;
			}

			Add(item);
			return false;
		}

		/// <summary>
		/// Return true if item was added; otherwise false
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool TryAdd(T item)
		{
			if (Contains(item))
				return false;

			Add(item);
			return true;
		}
	}
}
