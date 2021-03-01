namespace Leap.Data.Utilities {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    public static class ObjectDictionaryConvertorExtensions {
        public static IDictionary<string, object> ToDictionary(this object values) {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (values != null) {
                foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(values)) {
                    object obj = propertyDescriptor.GetValue(values);
                    dict.Add(propertyDescriptor.Name, obj);
                }
            }

            return dict;
        }
    }
}