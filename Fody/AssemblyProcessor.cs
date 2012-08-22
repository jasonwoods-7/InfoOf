using System.Linq;

public partial class ModuleWeaver
{

    public void ProcessMethods()
    {
        foreach (var type in ModuleDefinition.GetTypes())
        {
            foreach (var method in type.Methods.Where(x=>!x.IsAbstract))
            {
                ProcessMethod(method);
            }
        }
    }

}