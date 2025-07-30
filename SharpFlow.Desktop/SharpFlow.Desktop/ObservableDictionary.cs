using System.Collections.Generic;
using System.ComponentModel;

namespace SharpFlow.Desktop
{
	// Create a simple ObservableDictionary class
	public class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		public new TValue this[TKey key]
		{
			get => base[key];
			set
			{
				base[key] = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
			}
		}

		public new void Add(TKey key, TValue value)
		{
			base.Add(key, value);
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
		}
	}
}