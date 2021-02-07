using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace ChuckHill2
{
    /// <summary>
    /// Use this instead of List<T> and BindingList<T>, as a DataSource for DataGridView object. It provides built-in support for sorting columns in a DataGridView.
    /// If there are any non-default column or row attributes (eg. font, color, etc) they will need to be reset within the DataGridView.Sorted event.
    /// </summary>
    /// <typeparam name="T">typeof array element</typeparam>
    public class SortableBindingList<T> : BindingList<T>
    {
        private readonly Dictionary<Type, PropertyComparer<T>> comparers = new Dictionary<Type, PropertyComparer<T>>(typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Length);
        private bool isSorted;
        private ListSortDirection listSortDirection;
        private PropertyDescriptor propertyDescriptor;

        /// <summary>
        /// The dynamic current list of items in SortableBindingList.
        /// </summary>
        public List<T> List { get { return base.Items as List<T>; } }

        /// <summary>
        /// Create a new empty SortableBindingList
        /// </summary>
        public SortableBindingList() : base(new List<T>()) { }

        /// <summary>
        /// Create a new  SortableBindingList initialized with the specified list
        /// </summary>
        /// <param name="enumeration">List to of items to initialize SortableBindingList with.</param>
        public SortableBindingList(IEnumerable<T> enumeration) : base(new List<T>(enumeration)) { }

        /// <summary>
        /// Create a new empty SortableBindingList with an initial capacity
        /// </summary>
        /// <param name="capacity">SortableBindingList initial capacity.</param>
        public SortableBindingList(int capacity) : base(new List<T>(capacity)) { }

        protected override bool SupportsSortingCore { get { return true; } }
        protected override bool IsSortedCore { get { return this.isSorted; } }
        protected override PropertyDescriptor SortPropertyCore { get { return this.propertyDescriptor; } }
        protected override ListSortDirection SortDirectionCore { get { return this.listSortDirection; } }
        protected override bool SupportsSearchingCore { get { return true; } }
        protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
        {
            List<T> itemsList = (List<T>)this.Items;

            Type propertyType = property.PropertyType;
            PropertyComparer<T> comparer;
            if (!this.comparers.TryGetValue(propertyType, out comparer))
            {
                comparer = new PropertyComparer<T>(property, direction);
                this.comparers.Add(propertyType, comparer);
            }

            comparer.SetPropertyAndDirection(property, direction);
            itemsList.Sort(comparer);

            this.propertyDescriptor = property;
            this.listSortDirection = direction;
            this.isSorted = true;

            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
        protected override void RemoveSortCore()
        {
            this.isSorted = false;
            this.propertyDescriptor = base.SortPropertyCore;
            this.listSortDirection = base.SortDirectionCore;

            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
        protected override int FindCore(PropertyDescriptor property, object key)
        {
            int count = this.Count;
            for (int i = 0; i < count; ++i)
            {
                T element = this[i];
                if (property.GetValue(element).Equals(key))
                {
                    return i;
                }
            }
            return -1;
        }

        private class PropertyComparer<U> : IComparer<U>
        {
            private readonly IComparer comparer;
            private PropertyDescriptor propertyDescriptor;
            private int reverse;
            public PropertyComparer(PropertyDescriptor property, ListSortDirection direction)
            {
                this.propertyDescriptor = property;
                Type comparerForPropertyType = typeof(Comparer<>).MakeGenericType(property.PropertyType);
                this.comparer = (IComparer)comparerForPropertyType.InvokeMember("Default", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public, null, null, null);
                this.SetListSortDirection(direction);
            }
            public int Compare(U x, U y) { return this.reverse * this.comparer.Compare(this.propertyDescriptor.GetValue(x), this.propertyDescriptor.GetValue(y)); }
            private void SetPropertyDescriptor(PropertyDescriptor descriptor) { this.propertyDescriptor = descriptor; }
            private void SetListSortDirection(ListSortDirection direction) { this.reverse = direction == ListSortDirection.Ascending ? 1 : -1; }
            public void SetPropertyAndDirection(PropertyDescriptor descriptor, ListSortDirection direction)
            {
                this.SetPropertyDescriptor(descriptor);
                this.SetListSortDirection(direction);
            }
        }
    }
}
