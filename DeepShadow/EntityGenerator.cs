using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DeepShadow
{
    public static class EntityGenerator
    {
        private static int _varNbr = 0;
        private static List<StartedItem> _startedItems = new List<StartedItem>();
        private static string _result = "";

        private static void InitVariables()
        {
            _varNbr = 0;
            _startedItems = new List<StartedItem>();
            _result = "";
        }

        /// <summary>
        /// Returns C# code for all entities related to the supplied entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string GenerateEntitiesFromObject<T>(this T entity) where T: class
        {
            if (entity == null) throw new ArgumentNullException("entity", "entity cannot be null");
            InitVariables();
            GenerateEntitiesFromObject(entity, parentVariable:"");
            WriteLine($"\r\nreturn a1;");
            return _result;
        }

        /// <summary>
        /// Returns C# code for all entities related to the supplied list of entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string GenerateEntitiesFromList<T>(this IEnumerable<T> list) where T : class
        {
            if (list == null) throw new ArgumentNullException("list","list cannot be null");
            InitVariables();
                string className = list.First() .GetType().FullName;
                WriteLine($"List<{className}> list = new List<{className}>();\r\n");
            GenerateEntitiesFromList(list, parentVariable:"");
            WriteLine($"\r\nreturn list;");
            return _result;
        }

        private static void GenerateEntitiesFromList<T>(IEnumerable<T> list, string parentVariable = "", string parentPrincipleProperty = "", string parentCollectionProperty = "") where T : class
        {
            foreach (var item in list)
            {
                GenerateEntitiesFromObject(item, parentVariable, parentPrincipleProperty, parentCollectionProperty);
            }
        }

        private static void GenerateEntitiesFromList(Type type, object list, string parentVariable = "", string parentPrincipleProperty = "", string parentCollectionProperty = "")
        {
            foreach (var item in (IEnumerable)list)
            {
                GenerateEntitiesFromObject(item, parentVariable, parentPrincipleProperty, parentCollectionProperty);
            }
        }

        private static void GenerateEntitiesFromObject<T>(T item, string parentVariable = "", string parentPrincipleProperty = "", string parentCollectionProperty = "") where T : class
        {
            var testItem = _startedItems.SingleOrDefault(a => a.Item == item);
            if (testItem != null)
            {
                //still need to set prop, but not create model
                SetNavigationProperty(parentVariable, parentCollectionProperty, parentPrincipleProperty, testItem.ClassVariable);
                return;
            }
            _varNbr++;
            string classVariable = $"a{_varNbr}";
            string className = item.GetType().FullName;
            _startedItems.Add(new StartedItem(classVariable, item));
            WriteLine($"{className} {classVariable} = new {className}();");

            foreach (var prop in item.GetType().GetProperties())
            {
                object propValue = prop.GetValue(item);
                string propTypeName = prop.PropertyType.FullName;
                //is it a parent principle?
                if (prop.IsParentPrincipal(propValue))
                {
                    GenerateEntitiesFromObject(propValue, classVariable, prop.Name);
                }
                //is it a child collection?
                else if (prop.IsChildCollection(propValue))
                {
                    GenerateEntitiesFromList(prop.PropertyType, propValue, classVariable, "", prop.Name);
                }
                else
                {
                    Write($"{classVariable}.{prop.Name} = ");
                    if (propValue != null && propTypeName.Contains("String")) { Write("\""); }
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
                    Write(propValueText);
                    if (propValue != null && propTypeName.Contains("String")) { Write("\""); }
                    Write(";");
                    WriteLine();
                }
            }
            SetNavigationProperty(parentVariable, parentCollectionProperty, parentPrincipleProperty, classVariable);
            //if no parent, add to a list
            if (String.IsNullOrWhiteSpace(parentVariable))
            {
                WriteLine($"list.Add({classVariable});");
            }
        }

        private static void SetNavigationProperty(string parentVariable, string parentCollectionProperty, string parentPrincipleProperty, string classVariable)
        {
            if (!String.IsNullOrWhiteSpace(parentVariable))
            {
                if (!String.IsNullOrWhiteSpace(parentCollectionProperty))
                {
                    WriteLine($"{parentVariable}.{parentCollectionProperty}.Add({classVariable});");
                };
                if (!String.IsNullOrWhiteSpace(parentPrincipleProperty))
                {
                    WriteLine($"{parentVariable}.{parentPrincipleProperty} = {classVariable};");
                };
            }
        }

        private static void WriteLine(string msg = "")
        {
            _result += msg + "\r\n";
        }

        private static void Write(string msg = "")
        {
            _result += msg;
        }
    }
}
