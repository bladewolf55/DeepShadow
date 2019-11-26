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
        private static GenerationFromType _generationFromType = GenerationFromType.List;
        private static bool _toConsole = false;

        private enum GenerationFromType
        {
            List,
            Object
        }

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
        public static string GenerateEntitiesFromObject<T>(this T entity, bool toConsole = false) where T : class
        {
            return GenerateEntitiesFromObject(entity, null, toConsole);
        }

        public static string GenerateEntitiesFromObject<T>(this T entity, string[] ignoreProperties, bool toConsole = false) where T: class
        {
            _toConsole = toConsole;
            if (entity == null) throw new ArgumentNullException("entity", "entity cannot be null");
            _generationFromType = GenerationFromType.Object;
            InitVariables();
            GenerateEntitiesFromObject(entity, parentVariable: "");
            WriteLine($"\r\nreturn a1;");
            return _result;

        }

        /// <summary>
        /// Returns C# code for all entities related to the supplied list of entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string GenerateEntitiesFromList<T>(this IEnumerable<T> list, bool toConsole = false) where T : class
        {
            _toConsole = toConsole;
            if (list == null) throw new ArgumentNullException("list", "list cannot be null");
            _generationFromType = GenerationFromType.List;
            InitVariables();
            string className = list.First().GetType().FullName;
            WriteLine($"List<{className}> list = new List<{className}>();\r\n");
            GenerateEntitiesFromList(list, parentVariable: "");
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

        private static void GenerateEntitiesFromList(object list, string parentVariable = "", string parentPrincipleProperty = "", string parentCollectionProperty = "")
        {
            int count = 0;
            foreach (var item in (IEnumerable)list)
            {
                GenerateEntitiesFromObject(item, parentVariable, parentPrincipleProperty, parentCollectionProperty);
                count++;
            }
            if (count == 0)
            {
                Type t = list.GetType();
                string typeName = t.Name;
                string entityName = t.GenericTypeArguments[0].FullName;
                GeneratePropertyForEmptyList(typeName, parentVariable, parentCollectionProperty, entityName);
            }
        }

        private static void GenerateEntitiesFromObject<T>(T item, string parentVariable = "", string parentPrincipleProperty = "", string parentCollectionProperty = "", string[] ignoreProperties = null) where T : class
        {
            if (ignoreProperties == null) { ignoreProperties = new string[] { }; }
            var testItem = _startedItems.SingleOrDefault(a => a.Item == item);
            if (testItem != null)
            {
                //still need to set prop, but not create model
                SetNavigationProperty(parentVariable, parentCollectionProperty, parentPrincipleProperty, testItem.ClassVariable);
                return;
            }
            string classVariable = "";

            var type = item.GetType();
            string className = type.FullName;
            if (type.IsValueType || type.FullName == "System.String")
            {
                //it's part of List of values
                classVariable = item.ToString();
                if (type.Name.Contains("String")) { classVariable = $"\"{classVariable}\""; }
            }
            else
            {
                _varNbr++;
                classVariable = $"a{_varNbr}";
                _startedItems.Add(new StartedItem(classVariable, item));
                WriteLine($"{className} {classVariable} = new {className}();");

                foreach (var prop in item.GetType().GetProperties())
                {
                    if (ignoreProperties.Contains(prop.Name))
                    {
                        continue;
                    }
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
                        Type propType = prop.PropertyType;
                        //is it a list of values?
                        if (propType.IsValueOrStringEnumerable())
                        {
                            string initClass = propType.Name;
                            if (initClass.Contains("`"))
                            {
                                initClass = initClass.Substring(0, initClass.IndexOf("`"));
                            }
                            string initType = propType.GenericTypeArguments[0].Name;
                            WriteLine($"{classVariable}.{prop.Name} = new {initClass}<{initType}>();");
                        }
                        GenerateEntitiesFromList(propValue, classVariable, "", prop.Name);
                    }
                    else
                    {
                        Write($"{classVariable}.{prop.Name} = ");
                        if (propValue != null && propTypeName.Contains("String")) { Write("@\""); }
                        string propValueText = propValue == null ? "null" : propValue.ToString();
                        if (propValue != null)
                        {
                            if (propTypeName.Contains("String"))
                            {
                                propValueText = propValueText.Replace(@"""", @"""""");
                            }
                            if (propTypeName.Contains("Boolean"))
                            {
                                propValueText = propValueText.ToLower();
                            }
                            if (propTypeName.Contains("DateTime"))
                            {
                                propValueText = $"DateTime.Parse(\"{propValueText}\")";
                            }
                            if (propTypeName.Contains("Decimal"))
                            {
                                propValueText = $"Convert.ToDecimal({propValueText})";
                            }
                        }
                        Write(propValueText);
                        if (propValue != null && propTypeName.Contains("String")) { Write("\""); }
                        Write(";");
                        WriteLine();
                    }
                }
            }
            SetNavigationProperty(parentVariable, parentCollectionProperty, parentPrincipleProperty, classVariable);
            //if no parent and a list generation, add to a list
            if (String.IsNullOrWhiteSpace(parentVariable) & _generationFromType == GenerationFromType.List)
            {
                WriteLine($"list.Add({classVariable});");
            }
        }

        private static void GeneratePropertyForEmptyList(string typeName, string classVariable, string propName, string entityName)
        {
            typeName = typeName.Contains("`") ? typeName.Substring(0, typeName.IndexOf("`")) : typeName;
            WriteLine($"{classVariable}.{propName} = new {typeName}<{entityName}>();");
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
            if (_toConsole) { Console.WriteLine(msg); }
        }

        private static void Write(string msg = "")
        {
            _result += msg;
            if (_toConsole) { Console.Write(msg); }
        }
    }
}
