using System.Reflection;
using Task_1;

Type type = GetClassType();
object? instance = CreateInstance(type);
Console.WriteLine($"Instance of type {type} is created");

MethodInfo method = SelectMethod(type);
ParameterInfo[] parameters = method.GetParameters();
object[] arguments = new object[parameters.Length];

for (int i = 0; i < parameters.Length; i++)
{
    arguments[i] = GetParameterValue(parameters[i]);
}

object? result = method.Invoke(instance, arguments);
Console.WriteLine($"Method result: {result}");

static Type GetClassType()
{
    while (true)
    {
        Console.Write("Class full name: ");
        string? className = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(className)) continue;

        Type? type = Type.GetType(className);
        if (type != null) return type;

        Console.WriteLine("404. Class not found, available types:");
        foreach (Type availableType in Assembly.GetExecutingAssembly().GetTypes())
        {
            Console.WriteLine(availableType.FullName);
        }
    }
}

static object? CreateInstance(Type type)
{
    ConstructorInfo? constructor = type.GetConstructors().FirstOrDefault();
    object? instance = null;
    while (instance == null)
    {
        if (constructor != null)
        {
            instance = InvokeConstructor(constructor);
        }
        else
        {
            instance = Activator.CreateInstance(type);
        }
    }
    return instance;
}

static object? InvokeConstructor(ConstructorInfo constructor)
{
    ParameterInfo[] constructorParams = constructor.GetParameters();
    object[] constructorArgs = new object[constructorParams.Length];

    for (int i = 0; i < constructorParams.Length; i++)
    {
        constructorArgs[i] = GetParameterValue(constructorParams[i]);
    }

    try
    {
        return constructor.Invoke(constructorArgs);
    }
    catch (TargetInvocationException e) when (e.InnerException != null)
    {
        Console.WriteLine($"Constructor error: {e.InnerException.Message}");
    }
    catch (Exception e)
    {
        Console.WriteLine($"Unexpected error: {e.Message}");
    }
    return null;
}

static object GetParameterValue(ParameterInfo parameter)
{
    while (true)
    {
        Console.Write($"Enter value for {parameter.Name} ({parameter.ParameterType.Name}): ");
        string? input = Console.ReadLine();

        if (input == null)
        {
            Console.WriteLine("Input cannot be null, try again.");
            continue;
        }

        Type paramType = parameter.ParameterType;

        try
        {
            if (paramType.IsArray)
            {
                Console.WriteLine("Use commas for array elements:");
                input = Console.ReadLine();
                if (input == null)
                {
                    Console.WriteLine("Input cannot be null, try again.");
                    continue;
                }

                Type? elementType = paramType.GetElementType();
                if (elementType == null)
                {
                    Console.WriteLine("Element type is null.");
                    continue;
                }

                string[] elements = input.Split(',');

                Array array = Array.CreateInstance(elementType, elements.Length);
                for (int j = 0; j < elements.Length; j++)
                {
                    array.SetValue(Convert.ChangeType(elements[j].Trim(), elementType), j);
                }
                return array;
            }
            else if (paramType.IsEnum)
            {
                Console.WriteLine($"Available values: {string.Join(", ", Enum.GetNames(paramType))}");
                if (Enum.TryParse(paramType, input, true, out object? enumValue) && Enum.IsDefined(paramType, enumValue))
                {
                    return enumValue;
                }
                Console.WriteLine("Invalid enum value, try again.");
            }
            else
            {
                return Convert.ChangeType(input, paramType);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Wrong input type, try again.");
            Console.WriteLine(e.Message);
        }
    }
}

static MethodInfo SelectMethod(Type type)
{
    MethodInfo[] methods = type.GetMethods();
    MethodInfo? method = null;

    while (method == null)
    {
        Console.Write("Method name: ");
        string? methodName = Console.ReadLine();
        if (methodName == null)
        {
            Console.WriteLine("Method name cannot be null, try again.");
            continue;
        }

        MethodInfo[] matchingMethods = methods.Where(m => m.Name == methodName).ToArray();

        if (matchingMethods.Length == 0)
        {
            Console.WriteLine("404. Method not found. Available methods:");
            foreach (MethodInfo m in methods)
            {
                Console.WriteLine($"{m.Name}({string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name))})");
            }
            continue;
        }

        if (matchingMethods.Length == 1)
        {
            method = matchingMethods[0];
        }
        else
        {
            Console.WriteLine("Multiple overloads found. Choose one by entering the index:");
            for (int i = 0; i < matchingMethods.Length; i++)
            {
                Console.WriteLine($"[{i}] {matchingMethods[i].Name}({string.Join(", ", matchingMethods[i].GetParameters().Select(p => p.ParameterType.Name + " " + p.Name))})");
            }

            while (method == null)
            {
                Console.Write("Enter the method index: ");
                string? indexInput = Console.ReadLine();
                if (indexInput == null)
                {
                    Console.WriteLine("Input cannot be null, try again.");
                    continue;
                }

                if (int.TryParse(indexInput, out int index) && index >= 0 && index < matchingMethods.Length)
                {
                    method = matchingMethods[index];
                }
                else
                {
                    Console.WriteLine("Invalid selection, try again.");
                }
            }
        }
    }
    return method;
}
