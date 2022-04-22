using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleContainer
{
    public class SmartContainer
    {
        private static readonly Lazy<SmartContainer> _lazy = new Lazy<SmartContainer>(() => new SmartContainer());
        public static SmartContainer Instance => _lazy.Value;


        #region 添加项

        public void ScanAssemblies(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length <= 0)
            {
                return;
            }

            Assembly assembly = null;
            Type[] types = null;
            Array.ForEach(assemblies, item =>
            {
                assembly = Assembly.Load(item.FullName);
                types = assembly.GetTypes().Where(t => t.IsClass).ToArray();
                Register(types);
            });
        }

        private void Register(Type[] types)
        {
            foreach (var type in types)
            {
                ScanInterfaces(type);
                ScanProperties(type);
            }
        }

        private void ScanInterfaces(Type type)
        {
            Type[] interfaces = type.IsInterface ? null : type.GetInterfaces();
            if (interfaces == null || interfaces.Length <= 0)
                return;

            Type[] dependencyInterfaces = interfaces.Where(i => i == typeof(IDependencyService)).ToArray();
            if (dependencyInterfaces.Length <= 0)
                return;

            ValueItem valueItem = null;
            if (interfaces.Length == 1)
            {
                valueItem = new ValueItem() { ImplementType = type, InterfaceType = interfaces.FirstOrDefault() };
                LocalCache.Instance.TryAdd(string.Empty, valueItem);
                return;
            }

            foreach (var inter in interfaces)
            {
                if (inter == typeof(IDependencyService))
                    continue;

                valueItem = new ValueItem() { ImplementType = type, InterfaceType = inter };
                LocalCache.Instance.TryAdd(string.Empty, valueItem);
            }
        }


        private void ScanProperties(Type type)
        {
            PropertyInfo[] properties = type.GetProperties().Where(item => item.GetCustomAttribute<PropertyInjectAttribute>() != null).ToArray();
            if (properties == null || properties.Count() <= 0)
                return;

            Array.ForEach(properties, item =>
            {
                if (!LocalCache.Instance.ContainsKey(item.PropertyType.Name))
                {
                    ScanInterfaces(item.PropertyType);
                }
            });
        }

        public void Register<T>(T instance) where T : class
        {
            LocalCache.Instance.AddOrUpdate(string.Empty, new ValueItem() { ImplementType = instance.GetType(), Instance = instance });
        }

        #endregion 添加项

        #region  获取项

        public T Resolve<T>()
        {
            return Resolve<T>(string.Empty, null);
        }

        public T Resolve<T>(string name)
        {
            return Resolve<T>(name, null);
        }

        private string GetKeyName<T>(string name, Type implementType)
        {
            if (string.IsNullOrEmpty(name))
            {
                if (implementType == null)
                {
                    string key = LocalCache.Instance.GetKeyByInterface(typeof(T));
                    if (string.IsNullOrEmpty(key))
                        return default(T).ToString();

                    return key;
                }

                return implementType.Name;
            }

            return name;
        }
        private T Resolve<T>(string name, Type implementType)
        {
            string dicKey = GetKeyName<T>(name, implementType);
            var value = LocalCache.Instance.TryGetValue(dicKey);
            if (value != null)
            {
                if (value.Instance == null)
                {
                    var instance = GetInstance<T>(value.ImplementType);
                    value.Instance = instance;
                    return instance;
                }
                else
                {
                    return (T)value.Instance;
                }
            }

            return default(T);
        }

        /// <summary>
        /// 注入构造函数值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        private T GetInstance<T>(Type type)
        {
            T instance = default(T);
            var ConstructorsInfo = type.GetConstructors();
            if (ConstructorsInfo.Count() <= 0)
            {
                instance = (T)Activator.CreateInstance(type);
                InjectPropertyValue(instance);
                return instance;
            }

            var dicCountParameters = new Dictionary<int, ParameterInfo[]>();
            foreach (var item in ConstructorsInfo)
            {
                var tempParameters = item.GetParameters();
                dicCountParameters.Add(tempParameters.Count(), tempParameters);
            }
            //如果没有指定特性，则默认取参数最多的一个
            var parameters = dicCountParameters.OrderByDescending(c => c.Key).FirstOrDefault().Value;
            if (parameters == null || parameters.Length <= 0)
            {
                instance = (T)Activator.CreateInstance(type);
                InjectPropertyValue(instance);
                return instance;
            }

            List<object> param = new List<object>();
            ValueItem valueItem = null;
            foreach (var item in parameters)
            {
                valueItem = LocalCache.Instance.TryGetValue(item.ParameterType.Name);
                if (valueItem != null)
                {
                    valueItem.Instance = Resolve<object>(valueItem.ToString());
                    param.Add(valueItem.Instance);
                }
            }

            instance = (T)Activator.CreateInstance(type, param.ToArray());
            InjectPropertyValue(instance);
            return instance;
        }

        /// <summary>
        /// 注入属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        private void InjectPropertyValue<T>(T instance)
        {
            PropertyInfo[] properties = instance.GetType().GetProperties().Where(item => item.GetCustomAttribute<PropertyInjectAttribute>() != null).ToArray();
            if (properties == null || properties.Count() <= 0)
                return;


            ValueItem valueItem = null;
            string key = string.Empty;
            Array.ForEach(properties, item =>
            {
                key = LocalCache.Instance.GetKeyByInterface(item.PropertyType);
                Resolve<object>(key);

                valueItem = LocalCache.Instance.TryGetValue(key);
                item.SetValue(instance, valueItem.Instance);
            });
        }

        #endregion  获取项
    }
}

