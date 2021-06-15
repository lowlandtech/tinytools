using System;
using System.Linq;

namespace TinyTools
{
    public static class InterpolationExtensions
    {
        public static string Interpolate<T>(this string template, T model)
        {
            var props = model.GetType().GetProperties();
            foreach (var prop in props.Where(p => p.GetValue(model) != null))
            {
                template = template.Replace($"{{{prop.Name}}}", prop.GetValue(model).ToString());
            }
            return template;
        }
    }
}
