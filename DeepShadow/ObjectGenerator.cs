using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DeepShadow
{
    public class ObjectGenerator
    {
        private int _varNbr = 0;
        List<StartedItem> startedItems = new List<StartedItem>();

        public void GenerateObjectsFromList(Type type, object list, string parentVariable = "", string parentPrincipleProperty = "", string parentCollectionProperty = "")
        {
            foreach (var item in (IEnumerable)list)
            {
                GenerateObjects(item, parentVariable, parentPrincipleProperty, parentCollectionProperty);
            }
        }

        public void GenerateObjectsFromList<T>(T list, string parentVariable = "", string parentPrincipleProperty = "", string parentCollectionProperty = "") where T : class, IEnumerable
        {
            foreach (var item in list)
            {
                GenerateObjects(item, parentVariable, parentPrincipleProperty, parentCollectionProperty);
            }
        }

        public void GenerateObjects<T>(T item, string parentVariable = "", string parentPrincipleProperty = "", string parentCollectionProperty = "") where T : class
        {
            var testItem = startedItems.SingleOrDefault(a => a.Item == item);
            if (testItem != null)
            {
                //still need to set prop, but not create model
                SetNavigationProperty(parentVariable, parentCollectionProperty, parentPrincipleProperty, testItem.ClassVariable);
                return;
            }
            _varNbr++;
            string classVariable = $"a{_varNbr}";
            startedItems.Add(new StartedItem(classVariable, item));

            string className = item.GetType().FullName;
            Console.WriteLine($"{className} {classVariable} = new {className}();");
            foreach (var prop in item.GetType().GetProperties())
            {
                object propValue = prop.GetValue(item);
                string propTypeName = prop.PropertyType.FullName;
                //is it a parent principle?
                if (IsParentPrincipal(propValue, prop))
                {
                    GenerateObjects(propValue, classVariable, prop.Name);
                }
                //is it a child collection?
                else if (IsChildCollection(propValue, prop))
                {
                    GenerateObjectsFromList(prop.PropertyType, propValue, classVariable, "", prop.Name);
                }
                else
                {
                    Console.Write($"{classVariable}.{prop.Name} = ");
                    if (propValue != null && propTypeName.Contains("String")) { Console.Write("\""); }
                    string propValueText = propValue == null ? "null" : propValue.ToString();
                    if (propValue != null)
                    {
                        //replace \ with \\
                        propValueText = propValueText.Replace(@"\", @"\\");
                        //replace crlf
                        propValueText = propValueText.Replace("\r", "\\r");
                        propValueText = propValueText.Replace("\n", "\\n");
                        if (propTypeName.Contains("Boolean"))
                        {
                            propValueText = propValueText.ToLower();
                        }
                        if (propTypeName.Contains("DateTime"))
                        {
                            propValueText = $"DateTime.Parse(\"{propValueText}\")";
                        }
                    }
                    Console.Write(propValueText);
                    if (propValue != null && propTypeName.Contains("String")) { Console.Write("\""); }
                    Console.Write(";");
                    Console.WriteLine();
                }
            }
            SetNavigationProperty(parentVariable, parentCollectionProperty, parentPrincipleProperty, classVariable);
            //if no parent, add to a list
            if (String.IsNullOrWhiteSpace(parentVariable))
            {
                Console.WriteLine($"list.Add({classVariable});");
            }
        }

        private void SetNavigationProperty(string parentVariable, string parentCollectionProperty, string parentPrincipleProperty, string classVariable)
        {
            if (!String.IsNullOrWhiteSpace(parentVariable))
            {
                if (!String.IsNullOrWhiteSpace(parentCollectionProperty))
                {
                    Console.WriteLine($"{parentVariable}.{parentCollectionProperty}.Add({classVariable});");
                };
                if (!String.IsNullOrWhiteSpace(parentPrincipleProperty))
                {
                    Console.WriteLine($"{parentVariable}.{parentPrincipleProperty} = {classVariable};");
                };
            }
        }

        private bool IsParentPrincipal(object value, PropertyInfo property)
        {
            return value != null && !property.PropertyType.Name.Contains("String") && property.PropertyType.IsClass && !property.IsNonStringEnumerable();
        }

        private bool IsChildCollection(object value, PropertyInfo property)
        {
            return value != null && !property.PropertyType.Name.Contains("String") && property.IsNonStringEnumerable();
        }
    }
}
