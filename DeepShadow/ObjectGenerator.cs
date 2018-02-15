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
        public static string GenerateEntities<T>(this T entity) where T: class
        {
            InitVariables();
            if (_varNbr == 0)
            {
                string className = entity.GetType().FullName;
                WriteToResult($"{className} entity = new {className}();\r\n");
            }
            GenerateEntities(entity);
            WriteToResult($"\r\nreturn entity;");
            return _result;
        }

        /// <summary>
        /// Returns C# code for all entities related to the supplied list of entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string GenerateEntities<T>( IEnumerable<T> list) where T : class
        {
            InitVariables();
            if (_varNbr == 0)
            {
                string className = list.First() .GetType().FullName;
                WriteToResult($"List<{className}> list = new List<{className}>();\r\n");
            }
            GenerateEntities(list);
            WriteToResult($"\r\nreturn list;");
            return _result;
        }

        private static void GenerateEntities<T>(IEnumerable<T> list, string parentVariable = "", string parentPrincipleProperty = "", string parentCollectionProperty = "") where T : class
        {
            foreach (var item in list)
            {
                GenerateEntities(item, parentVariable, parentPrincipleProperty, parentCollectionProperty);
            }
        }

        private static void GenerateEntities(Type type, object list, string parentVariable = "", string parentPrincipleProperty = "", string parentCollectionProperty = "")
        {
            foreach (var item in (IEnumerable)list)
            {
                GenerateEntities(item, parentVariable, parentPrincipleProperty, parentCollectionProperty);
            }
        }

        private static void GenerateEntities<T>(T item, string parentVariable = "", string parentPrincipleProperty = "", string parentCollectionProperty = "") where T : class
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
            WriteToResult($"{className} {classVariable} = new {className}();");

            foreach (var prop in item.GetType().GetProperties())
            {
                object propValue = prop.GetValue(item);
                string propTypeName = prop.PropertyType.FullName;
                //is it a parent principle?
                if (prop.IsParentPrincipal(propValue))
                {
                    GenerateEntities(propValue, classVariable, prop.Name);
                }
                //is it a child collection?
                else if (prop.IsChildCollection(propValue))
                {
                    GenerateEntities(prop.PropertyType, propValue, classVariable, "", prop.Name);
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
                    WriteToResult();
                }
            }
            SetNavigationProperty(parentVariable, parentCollectionProperty, parentPrincipleProperty, classVariable);
            //if no parent, add to a list
            if (String.IsNullOrWhiteSpace(parentVariable))
            {
                WriteToResult($"list.Add({classVariable});");
            }
        }

        private static void SetNavigationProperty(string parentVariable, string parentCollectionProperty, string parentPrincipleProperty, string classVariable)
        {
            if (!String.IsNullOrWhiteSpace(parentVariable))
            {
                if (!String.IsNullOrWhiteSpace(parentCollectionProperty))
                {
                    WriteToResult($"{parentVariable}.{parentCollectionProperty}.Add({classVariable});");
                };
                if (!String.IsNullOrWhiteSpace(parentPrincipleProperty))
                {
                    WriteToResult($"{parentVariable}.{parentPrincipleProperty} = {classVariable};");
                };
            }
        }

        private static void WriteToResult(string msg = "\r\n")
        {
            _result += msg + "\r\n";
        }
    }
}
