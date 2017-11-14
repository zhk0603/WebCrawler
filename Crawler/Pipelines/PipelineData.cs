using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Pipelines
{
    public class PipelineData : Dictionary<string, object>
    {
        public void SetNextData(object data)
        {
            this[Constants.PipelineDataKey] = data;
        }

        public object GetPreviousData()
        {
            TryGetValue(Constants.PipelineDataKey, out var @object);
            return @object;
        }

        public T GetPreviousData<T>()
        {
            return GetValue<T>(Constants.PipelineDataKey);
        }

        public T GetValue<T>(string key)
        {
            TryGetValue(key, out var value);
            if (value is T variable)
            {
                return variable;
            }
            return default(T);
        }

        public bool TryGetValue<T>(string key, out T value)
            where T : class
        {
            if (TryGetValue(key, out var @object))
            {
                if (@object is T variable)
                {
                    value = variable;
                    return true;
                }
            }

            value = default(T);
            return false;
        }
    }
}