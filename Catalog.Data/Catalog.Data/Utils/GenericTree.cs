using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Data.Utils
{
    public class GenericTree<T> where T : GenericTree<T> // recursive constraint  
    {
        // no specific data declaration  

        protected List<T> children;

        public GenericTree()
        {
            this.children = new List<T>();
        }

        public virtual void AddChild(T newChild)
        {
            this.children.Add(newChild);
        }

        public void Traverse(Action<int, T> visitor)
        {
            this.traverse(0, visitor);
        }

        protected virtual void traverse(int depth, Action<int, T> visitor)
        {
            visitor(depth, (T)this);
            foreach (T child in this.children)
                child.traverse(depth + 1, visitor);
        }
    }
}
