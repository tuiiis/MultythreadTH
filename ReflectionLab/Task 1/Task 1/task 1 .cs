using System.Reflection;
using reflection;

Type? type = null;
while (type == null)
{
    Console.Write("Class full name: ");
    string? className = Console.ReadLine();
    type = Type.GetType(className);
    if (type == null)
    {
        Console.WriteLine("404. Class not found, please specify: ");
        foreach (var t in Assembly.GetExecutingAssembly().GetTypes())
        {
            Console.WriteLine(t.FullName);
        }
    }
}

// instance

ConstructorInfo? constructor = type.GetConstructors().FirstOrDefault();
object? instance = null;
while (instance == null) // while ^^
{
    if (constructor != null)
    {

        ParameterInfo[] constructorParams = constructor.GetParameters();
        object[] constructorArgs = new object[constructorParams.Length];

        for (int i = 0; i < constructorParams.Length; i++)
        {
            Console.WriteLine($"Enter an agrument for {type} constructor: ({constructorParams[i].ParameterType.Name} {constructorParams[i].Name}): ");
            Type paramType = constructorParams[i].ParameterType;
            if (paramType.IsArray)
            {
                Console.WriteLine("Use comams for array elements:");
            }
                string? input = Console.ReadLine();


            try
            {
                if (paramType.IsArray)
                {
                    Type? elementType = paramType.GetElementType();
                    string[] elements = input.Split(',');
                    Array array = Array.CreateInstance(elementType, elements.Length);

                    for (int j = 0; j < elements.Length; j++)
                    {
                        array.SetValue(Convert.ChangeType(elements[j].Trim(), elementType), j);
                    }

                    constructorArgs[i] = array;
                }
                else
                {
                    constructorArgs[i] = Convert.ChangeType(input, paramType);
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
            instance = constructor.Invoke(constructorArgs);
        }
        catch (TargetInvocationException e) when (e.InnerException != null) // bet on how long it took for me to fix it
        {
            Console.WriteLine($"Constructor error: {e.InnerException.Message}");
            continue;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error: {e.Message}");
            continue;
        }
    }
    else
    {
        instance = Activator.CreateInstance(type);
    }
}


Console.WriteLine($"Istance of type {type} is created");


// method


MethodInfo[] methods = type.GetMethods();
MethodInfo? method = null;

while (method == null)
{
    Console.Write("Method name: ");
    string? methodName = Console.ReadLine();

    var matchingMethods = methods.Where(m => m.Name == methodName).ToArray();

    if (matchingMethods.Length == 0)
    {
        Console.WriteLine("404. Method not found. Available methods:");
        foreach (var m in methods)
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
            if (int.TryParse(Console.ReadLine(), out int index) && index >= 0 && index < matchingMethods.Length)
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

ParameterInfo[] parameters = method.GetParameters();
object[] arguments = new object[parameters.Length];

for (int i = 0; i < parameters.Length; i++)
{
    while (true)
    {
        Console.Write($"Enter value for {parameters[i].Name} ({parameters[i].ParameterType.Name}): ");
        Type paramType = parameters[i].ParameterType;
        string? input = Console.ReadLine();

        try
        {
            if (paramType.IsArray)
            {
                Console.WriteLine("Use commas for array elements:");
                input = Console.ReadLine();
                Type elementType = paramType.GetElementType();
                string[] elements = input.Split(',');

                Array array = Array.CreateInstance(elementType, elements.Length);
                for (int j = 0; j < elements.Length; j++)
                {
                    array.SetValue(Convert.ChangeType(elements[j].Trim(), elementType), j);
                }

                arguments[i] = array;
            }
            else
            {
                arguments[i] = Convert.ChangeType(input, paramType);
            }
            break;
        }
        catch (Exception e)
        {
            Console.WriteLine("Wrong input type, try again.");
            Console.WriteLine(e.Message);
        }
    }
}

object? result = method.Invoke(instance, arguments);
Console.WriteLine($"Method result: {result}");