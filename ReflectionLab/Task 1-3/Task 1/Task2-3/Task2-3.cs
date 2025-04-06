using System;
using System.IO;
using System.Linq;
using System.Reflection;

public static class Task2_3
{
    const string PrintObjectMethodName = "PrintObject";

    public static void Run()
    {
        Assembly? assembly = null;
        string? dllFullPath = null;

        while (assembly == null)
        {
            Console.Write("Enter the full path to the DLL: ");
            string? inputPath = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(inputPath))
            {
                Console.WriteLine("Invalid input. Please enter a valid path.");
                continue;
            }

            dllFullPath = Path.GetFullPath(inputPath);

            if (!File.Exists(dllFullPath))
            {
                Console.WriteLine($"DLL not found at: {dllFullPath}");
                continue;
            }

            try
            {
                assembly = Assembly.LoadFrom
                    
                    (dllFullPath);
                Console.WriteLine($"Loaded assembly: {assembly.FullName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load assembly: {ex.Message}");
                assembly = null;
            }
        }

        foreach (Type type in assembly.GetTypes())
        {
            Console.WriteLine($"Class: {type.FullName}");
            PrintMembers(type);
        }

        Console.Write("Enter the class name to work with: ");
        string? className = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(className))
        {
            Console.WriteLine("Class name cannot be empty.");
            return;
        }

        Type? selectedType = assembly.GetType(className);
        if (selectedType == null)
        {
            Console.WriteLine("Class not found.");
            return;
        }

        object? instance = InvokeCreateMethod(selectedType);
        if (instance != null)
        {
            InvokePrintObjectMethod(instance, selectedType);
        }
        else
        {
            Console.WriteLine("Object creation failed.");
        }
    }

    static void PrintMembers(Type type)
    {
        PropertyInfo[] properties = type.GetProperties();
        if (properties.Length > 0)
        {
            Console.WriteLine("  Properties:");
            foreach (PropertyInfo prop in properties)
            {
                Console.WriteLine($"    - {prop.Name} ({prop.PropertyType.Name})");
            }
        }

        MethodInfo[] methods = type.GetMethods();
        if (methods.Length > 0)
        {
            Console.WriteLine("  Methods:");
            foreach (MethodInfo method in methods)
            {
                Console.WriteLine($"    - {method.Name} ({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})");
            }
        }

        FieldInfo[] fields = type.GetFields();
        if (fields.Length > 0)
        {
            Console.WriteLine("  Fields:");
            foreach (FieldInfo field in fields)
            {
                Console.WriteLine($"    - {field.Name} ({field.FieldType.Name})");
            }
        }
    }

    static object? InvokeCreateMethod(Type type)
    {
        MethodInfo? createMethod = type.GetMethod("Create", BindingFlags.Static | BindingFlags.Public);
        if (createMethod == null)
        {
            Console.WriteLine("Static method 'Create' not found.");
            return null;
        }

        object[] args = GetMethodArguments(createMethod);
        try
        {
            return createMethod.Invoke(null, args);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error calling Create: {e.InnerException?.Message ?? e.Message}");
            return null;
        }
    }

    static void InvokePrintObjectMethod(object instance, Type type)
    {
        MethodInfo? printMethod = type.GetMethod(PrintObjectMethodName, BindingFlags.Instance | BindingFlags.Public);
        if (printMethod == null)
        {
            Console.WriteLine($"Method '{PrintObjectMethodName}' not found.");
            return;
        }

        try
        {
            printMethod.Invoke(instance, null);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error calling {PrintObjectMethodName}: {e.InnerException?.Message ?? e.Message}");
        }
    }

    static object[] GetMethodArguments(MethodInfo method)
    {
        ParameterInfo[] parameters = method.GetParameters();
        object[] arguments = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            arguments[i] = GetParameterValue(parameters[i]);
        }

        return arguments;
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
}
