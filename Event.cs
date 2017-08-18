using System;
using System.Collections;
using System.IO;

namespace Event_parse
{
    class Event
    {
        public Hashtable _values = new Hashtable();

        public void AddValue(string key, string value)
        {
            _values.Add(key, value);
        }

        public void WriteValues(StreamWriter sw)
        {
            IDictionaryEnumerator enumerator = _values.GetEnumerator();

            while (enumerator.MoveNext())
                sw.WriteLine(enumerator.Key.ToString().PadRight(15) + enumerator.Value);
        }
    }
}
