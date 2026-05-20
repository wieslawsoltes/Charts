using System;
using System.Collections.ObjectModel;

namespace Avalonia.Collections
{
    public class AvaloniaList<T> : ObservableCollection<T>
    {
        public AvaloniaList() : base() { }

        public AvaloniaList(System.Collections.Generic.IEnumerable<T> collection) : base(collection) { }

        public void AddRange(System.Collections.Generic.IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            foreach (var item in items)
            {
                Add(item);
            }
        }
    }
}
