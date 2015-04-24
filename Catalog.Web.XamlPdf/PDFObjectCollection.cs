using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Catalog.Web.XamlPdf
{
    public interface IPDFObjectCollection
    {
    }

    public class PDFChildObject : PDFObject
    {
        #region Public Properties

        public PDFObject Parent
        {
            get { return GetValue<PDFObject>("Parent"); }
            set { SetValue("Parent", value); }
        }

        #endregion Public Properties
    }
    public class PDFObjectCollection<T> : PDFObject, IList<T>, IPDFObjectCollection
        where T : PDFChildObject
    {
        #region Public Properties

        public int Count
        {
            get { return items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion Public Properties

        #region Public Indexers

        public T this[int index]
        {
            get { return items[index]; }
            set { items[index] = value; }
        }

        #endregion Public Indexers

        #region Public Methods

        public void Add(T item)
        {
            items.Add(item);
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(T item)
        {
            return items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public TX Create<TX>() where TX : T
        {
            var item = Document.CreateObject<TX>();
            item.Parent = this;
            items.Add(item);
            return item;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            items.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return items.Remove(item);
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void WriteProperties(TextWriter writer)
        {
            base.WriteProperties(writer);
            if (Count > 0)
            {
                writer.WriteLine("/Kids [{0}]", string.Join(" ", this.Select(x => x.Ref)));
            }
            writer.WriteLine("/Count {0}", this.Count);
        }

        #endregion Protected Methods

        #region Private Fields

        private readonly List<T> items = new List<T>();

        #endregion Private Fields
    }
}
