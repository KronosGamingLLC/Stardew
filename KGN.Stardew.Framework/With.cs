using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KGN.Stardew.Framework
{
    //https://github.com/ababik/Remute
    //converted into an extension to use like the with keyword in other languages
    //since i didnt really pay attention when converting and since i just took it at its word, this needs to be reviewed
    //TODO: create a version that can change multiple properties at a time
    //need to compare performance with just newing up objects or other ways to simulate with (like via fody)
    //TODO: how to constrain to only immutable types
    public static class WithExtension
    {
        /// <summary>
        /// An extension method to simulate the "with" keyword from functional languages. An oversimplified description is that it creates a new instance of an immutable object with a specific value changed.
        /// </summary>
        /// <param name="expression">Expression returning the property to be mutated</param>
        /// <param name="value">The value the property is to be set to</param>
        /// <returns>A new instance of the object with the selected property set to the new value.</returns>
        public static TInstance With<TInstance, TValue>(this TInstance instance, Expression<Func<TInstance, TValue>> expression, TValue value)
            where TInstance : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var activationConfiguration = new ActivationConfiguration();
            var activationContextCache = new Dictionary<Type, ActivationContext>();

            var result = value as object;

            var instanceExpression = expression.Body;

            while (instanceExpression is MemberExpression)
            {
                var propertyExpression = instanceExpression as MemberExpression;
                instanceExpression = propertyExpression.Expression;

                var property = propertyExpression.Member as PropertyInfo;

                if (property == null)
                    throw new Exception($"Unable to get property info for {propertyExpression.Member.Name} on type {typeof(TInstance).Name}. This is most likey because this was declared as a field and not a property.");

                var type = property.DeclaringType;

                var activationContext = GetActivationContext(type, activationContextCache, activationConfiguration);

                var lambdaExpression = Expression.Lambda<Func<TInstance, object>>(instanceExpression, expression.Parameters);
                var compiledExpression = lambdaExpression.Compile();
                var currentInstance = compiledExpression.Invoke(instance);

                var arguments = ResolveActivatorArguments(activationContext.ParameterResolvers, property, currentInstance, ref result);
                result = activationContext.Activator.Invoke(arguments);
            }

            return (TInstance)result;
        }

        private static ActivationContext GetActivationContext(Type type, Dictionary<Type, ActivationContext> activationContextCache, ActivationConfiguration activationConfiguration)
        {
            if (activationContextCache.TryGetValue(type, out ActivationContext result))
            {
                return result;
            }

            var constructor = FindConstructor(type, activationConfiguration);
            var activator = GetActivator(constructor);
            var parameterResolvers = GetParameterResolvers(type, constructor, activationConfiguration);

            result = new ActivationContext(type, activator, parameterResolvers);
            activationContextCache[type] = result;

            return result;
        }

        private static ConstructorInfo FindConstructor(Type type, ActivationConfiguration activationConfiguration)
        {
            if (activationConfiguration.Settings.TryGetValue(type, out ActivationSetting setting))
            {
                return setting.Constructor;
            }

            var constructors = type.GetTypeInfo().DeclaredConstructors;

            if (constructors.Count() != 1)
            {
                throw new Exception($"Unable to find appropriate constructor of type '{type.Name}'. Consider to use {nameof(ActivationConfiguration)} parameter.");
            }

            return constructors.Single();
        }

        private static PropertyInfo FindProperty(Type type, ParameterInfo parameter, PropertyInfo[] properties, ActivationConfiguration activationConfiguration)
        {
            if (activationConfiguration.Settings.TryGetValue(type, out ActivationSetting setting))
            {
                if (setting.Parameters.TryGetValue(parameter, out PropertyInfo property))
                {
                    return property;
                }
            }

            properties = properties.Where(x => string.Equals(x.Name, parameter.Name, StringComparison.OrdinalIgnoreCase)).ToArray();

            if (properties.Count() != 1)
            {
                throw new Exception($"Unable to find appropriate property to use as a constructor parameter '{parameter.Name}'. Type '{type.Name}'. Consider to use {nameof(ActivationConfiguration)} parameter.");
            }

            return properties.Single();
        }

        private static Activator GetActivator(ConstructorInfo constructor)
        {
            var parameters = constructor.GetParameters();

            var parameterExpression = Expression.Parameter(typeof(object[]));
            var argumentExpressions = new Expression[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var indexExpression = Expression.Constant(i);
                var paramType = parameters[i].ParameterType;
                var arrayExpression = Expression.ArrayIndex(parameterExpression, indexExpression);
                var arrayExpressionConvert = Expression.Convert(arrayExpression, paramType);
                argumentExpressions[i] = arrayExpressionConvert;
            }

            var constructorExpression = Expression.New(constructor, argumentExpressions);
            var lambdaExpression = Expression.Lambda<Activator>(constructorExpression, parameterExpression);
            var compiledExpression = lambdaExpression.Compile();
            return compiledExpression;
        }

        private static ParameterResolver[] GetParameterResolvers(Type type, ConstructorInfo constructor, ActivationConfiguration activationConfiguration)
        {
            var properties = type.GetTypeInfo().DeclaredProperties.ToArray();
            var parameters = constructor.GetParameters();

            var parameterResolvers = new ParameterResolver[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var property = FindProperty(type, parameter, properties, activationConfiguration);

                var expressionParameter = Expression.Parameter(typeof(object));
                var expressionParameterConvert = Expression.Convert(expressionParameter, type);
                var expressionProperty = Expression.Property(expressionParameterConvert, property);
                var expressionPropertyConvert = Expression.Convert(expressionProperty, typeof(object));
                var lambda = Expression.Lambda<Func<object, object>>(expressionPropertyConvert, expressionParameter);
                var resolver = lambda.Compile();

                var parameterResolver = new ParameterResolver(parameter, property, resolver);
                parameterResolvers[i] = parameterResolver;
            }

            return parameterResolvers;
        }

        private static object[] ResolveActivatorArguments(ParameterResolver[] parameterResolvers, PropertyInfo property, object instance, ref object result)
        {
            var arguments = new object[parameterResolvers.Length];

            for (var i = 0; i < parameterResolvers.Length; i++)
            {
                var parameterResolver = parameterResolvers[i];
                var argument = default(object);

                if (parameterResolver.Property == property)
                {
                    argument = result;
                    result = instance;
                }
                else
                {
                    argument = parameterResolver.Resolver.Invoke(instance);
                }

                arguments[i] = argument;
            }

            return arguments;
        }

        private class ActivationSetting
        {
            public ConstructorInfo Constructor { get; }
            public Dictionary<ParameterInfo, PropertyInfo> Parameters { get; }

            public ActivationSetting(ConstructorInfo constructor, Dictionary<ParameterInfo, PropertyInfo> parameters)
            {
                parameters.ToList().ForEach(x => Validate(constructor, x.Key, x.Value));

                Constructor = constructor;
                Parameters = parameters;
            }

            private static void Validate(ConstructorInfo constructor, ParameterInfo parameter, PropertyInfo property)
            {
                if (parameter.Member != constructor)
                {
                    throw new Exception($"Invalid parameter '{parameter.Name}'. Parameter must be a member of '{constructor.DeclaringType}' constructor.");
                }

                if (property.DeclaringType != constructor.DeclaringType)
                {
                    throw new Exception($"Invalid property '{property.Name}'. Must be a member of '{constructor.DeclaringType}'.");
                }
            }
        }
        private class ActivationConfiguration
        {
            internal Dictionary<Type, ActivationSetting> Settings { get; }

            public ActivationConfiguration()
            {
                Settings = new Dictionary<Type, ActivationSetting>();
            }

            public ActivationConfiguration Configure(ConstructorInfo constructor)
            {
                return Configure(constructor, new Dictionary<ParameterInfo, PropertyInfo>());
            }

            public ActivationConfiguration Configure(ConstructorInfo constructor, Dictionary<ParameterInfo, PropertyInfo> parameters)
            {
                var type = constructor.DeclaringType;
                Settings[type] = new ActivationSetting(constructor, parameters);

                return this;
            }

            public ActivationConfiguration Configure<T>(Expression<Func<T, T>> expression)
            {
                var constructorExpression = expression.Body as NewExpression;

                if (constructorExpression == null)
                {
                    throw new Exception($"Expression must specify constructor of '{typeof(T)}'.");
                }

                var constructor = constructorExpression.Constructor;
                var constructorParameters = constructor.GetParameters();
                var expressionParameters = constructorExpression.Arguments;
                var parameters = new Dictionary<ParameterInfo, PropertyInfo>();

                for (var i = 0; i < constructorParameters.Length; i++)
                {
                    var constructorParameter = constructorParameters[i];
                    var expressionParameter = expressionParameters[i];
                    var propertyExpression = expressionParameter as MemberExpression;
                    var property = propertyExpression?.Member as PropertyInfo;

                    parameters[constructorParameter] = property
                        ?? throw new Exception($"Parameter {expressionParameter} must be a property of '{typeof(T)}'.");
                }

                return Configure(constructor, parameters);
            }
        }
        private class ParameterResolver
        {
            public ParameterInfo Parameter { get; }

            public PropertyInfo Property { get; }

            public Func<object, object> Resolver { get; }

            public ParameterResolver(ParameterInfo parameter, PropertyInfo property, Func<object, object> resolver)
            {
                Parameter = parameter;
                Property = property;
                Resolver = resolver;
            }
        }
        private delegate object Activator(params object[] args);
        private class ActivationContext
        {
            public Type Type { get; }

            public Activator Activator { get; }

            public ParameterResolver[] ParameterResolvers { get; }

            public ActivationContext(Type type, Activator activator, ParameterResolver[] parameterResolvers)
            {
                Type = type;
                Activator = activator;
                ParameterResolvers = parameterResolvers;
            }
        }
    }
}
