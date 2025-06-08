using System.Collections.Generic;

namespace VA.TMP.Integration.Core
{
    public class Pipeline<T>
    {
        protected readonly List<IFilter<T>> filters = new List<IFilter<T>>();

        public Pipeline<T> Register(IFilter<T> filter)
        {
            filters.Add(filter);
            return this;
        }

        public void Execute(T input)
        {
            foreach (var filter in filters)
            {
                filter.Execute(input);
            }
        }
    }
}