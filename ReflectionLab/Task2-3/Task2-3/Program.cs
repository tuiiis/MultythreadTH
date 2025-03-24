using System.Reflection;

Console.Write("Enter the full path to the DLL: ");
string dllFullPath = Path.GetFullPath(Console.ReadLine());

if (!File.Exists(dllFullPath))
{
    Console.WriteLine($"DLL not found at: {dllFullPath}");
    return;
}

Assembly assembly = Assembly.LoadFrom(dllFullPath);
Console.WriteLine($"Loaded assembly: {assembly.FullName}");


Type[] types = assembly.GetTypes();


foreach (var type in types)
{
    Console.WriteLine($"Class: {type.FullName}");


    var properties = type.GetProperties();
    if (properties.Length > 0)
    {
        Console.WriteLine("  Properties:");
        foreach (var prop in properties)
        {
            Console.WriteLine($"   - {prop.Name} ({prop.PropertyType.Name})");
        }
    }


    var methodlist = type.GetMethods();
    if (methodlist.Length > 0)
    {
        Console.WriteLine("  Methods:");
        foreach (var method in methodlist)
        {
            Console.WriteLine($"    - {method.Name} ({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})");
        }
    }

    var fields = type.GetFields();
    if (fields.Length > 0)
    {
        Console.WriteLine("  Fields:");
        foreach (var field in fields)
        {
            Console.WriteLine($"    - {field.Name} ({field.FieldType.Name})");
        }
    }
}

Console.WriteLine("Enter the name of the class you want to work with:");
string? className = Console.ReadLine();
Type? selectedType = types.FirstOrDefault(t => t.FullName == className);

if (selectedType == null)
{
    Console.WriteLine("Class not found.");
    return;
}

MethodInfo? createMethod = selectedType.GetMethod("Create", BindingFlags.Static | BindingFlags.Public);
object? result = null;
if (createMethod != null)
{
    ParameterInfo[] methodParams = createMethod.GetParameters();
    object[] methodArgs = new object[methodParams.Length];

    for (int i = 0; i < methodParams.Length; i++)
    {
        Console.WriteLine($"Enter an argument for {createMethod.Name} method: ({methodParams[i].ParameterType.Name} {methodParams[i].Name}): ");
        string? input = Console.ReadLine();

        try
        {
            if (methodParams[i].ParameterType.IsArray)
            {
                Console.WriteLine("Use commas for array elements:");
                Type? elementType = methodParams[i].ParameterType.GetElementType();
                string[] elements = input.Split(',');
                Array array = Array.CreateInstance(elementType, elements.Length);

                for (int j = 0; j < elements.Length; j++)
                {
                    array.SetValue(Convert.ChangeType(elements[j].Trim(), elementType), j);
                }

                methodArgs[i] = array;
            }
            else
            {
                methodArgs[i] = Convert.ChangeType(input, methodParams[i].ParameterType);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Wrong input type");
            Console.WriteLine(e.Message);
            i--;
            continue;
        }
    }

    try
    {
        result = createMethod.Invoke(null, methodArgs);
        Console.WriteLine($"Method {createMethod.Name} returned: {result}");
    }
    catch (TargetInvocationException e) when (e.InnerException != null)
    {
        Console.WriteLine($"Constructor error: {e.InnerException.Message}");
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error calling method: {e.Message}");
    }

}
else
{
    Console.WriteLine("Static method 'Create' not found.");
}

if (result != null)
{
    MethodInfo? printMethod = selectedType.GetMethod("PrintObject", BindingFlags.Instance | BindingFlags.Public);

    if (printMethod != null)
    {
        try
        {
            printMethod.Invoke(result, null);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error calling PrintObject: {e.Message}");
        }
    }
    else
    {
        Console.WriteLine("Method 'PrintObject' not found");
    }
}
else
{
    Console.WriteLine("Object creation failed, cannot call PrintObject().");
}


Console.ReadLine();