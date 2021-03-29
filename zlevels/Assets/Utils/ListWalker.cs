using System.Collections.Generic;

namespace ZLevels.Utils
{
    /// <summary>
    /// ToDo: check if referenced list is valid and if it is still valid
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListWalker<T>
    {
        public T Current => list[selected];

        private List<T> list;
        private int selected;

        public ListWalker(List<T> list) => this.list = list;

        public T Next()
        {
            selected = (selected + 1) % list.Count;
            return Current;
        }

        public T Previous()
        {
            if (--selected < 0) selected = list.Count - 1;
            return Current;
        }
    }
}