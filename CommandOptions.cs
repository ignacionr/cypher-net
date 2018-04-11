using System;
using System.Linq;
using System.Collections.Generic;

namespace cypher
{
    internal class CommandOptions
    {
        Dictionary<string, List<string>> _options = new Dictionary<string, List<string>>();
        public CommandOptions(string[] args, IEnumerable<string> boolean_options)
        {
            string current_key = string.Empty;
            _options[current_key] = new List<string>();
            foreach (var arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    current_key = arg.Substring(1);
                    _options[current_key] = new List<string>();
                    if (boolean_options.Contains(current_key))
                    {
                        _options[current_key].Add("yes");
                        current_key = string.Empty;
                    }
                }
                else
                {
                    _options[current_key].Add(arg);
                }
            }
        }
        public string GetOne(string key, string default_value = null)
        {
            string result;
            if (_options.ContainsKey(key))
            {
                var vals = _options[key];
                if (vals.Count > 1)
                {
                    throw new ArgumentException($"Too many values for {key} specified" , key);
                }
                result = vals[0];
            }
            else if (null != default_value)
            {
                result = default_value;
            }
            else
            {
                throw new ArgumentException("Missing " + key, key);
            }
            return result;
        }
        public bool IsPresent(string key)
        {
            return GetOne(key, "no") == "yes";
        }
        public List<string> Get(string key)
        {
            if (!_options.ContainsKey(key))
            {
                throw new ArgumentException($"Missing {key}", key);
            }
            return _options[key];
        }
    }
}