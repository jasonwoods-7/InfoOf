using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

public partial class ModuleWeaver
{

    void ProcessMethod(MethodDefinition method)
    {
        var actions = new List<Action<ILProcessor>>();
        foreach (var instruction in method.Body.Instructions)
        {
            if (instruction.OpCode == OpCodes.Call)
            {
                var methodReference = instruction.Operand as MethodReference;
                if (methodReference == null)
                {
                    continue;
                }
                if (methodReference.DeclaringType.Name != "Info")
                {
                    continue;
                }
                var copy = instruction;
                if (methodReference.Name == "OfMethod")
                {
                    actions.Add(x => HandleOfMethod(copy,x));
                    continue;
                }
                if (methodReference.Name == "OfField")
                {
                    actions.Add(x => HandleOfField(copy,x));
                    continue;
                }
                if (methodReference.Name == "OfType")
                {
                    actions.Add(x => HandleOfType(copy,x));
                    continue;
                }
                if (methodReference.Name == "OfPropertyGet")
                {
                    actions.Add(x => HandleOfPropertyGet(copy,x));
                    continue;
                }
                if (methodReference.Name == "OfPropertySet")
                {
                    actions.Add(x => HandleOfPropertySet(copy,x));
                    continue;
                }
            }
        }
        if (actions.Count == 0)
        {
            return;
        }
        method.Body.SimplifyMacros();
        foreach (var action in actions)
        {
            action(method.Body.GetILProcessor());
        }
        method.Body.OptimizeMacros();
    }

    TypeDefinition GetTypeDefinition(string typeName)
    {
        var typeDefinition = ModuleDefinition.GetTypes()
            .FirstOrDefault(x => x.FullName == typeName);
        if (typeDefinition == null)
        {
            throw new WeavingException(string.Format("Could not find type named '{0}'.", typeName));
        }
        return typeDefinition;
    }
    TypeDefinition GetTypeDefinition(string assemblyName, string typeName)
    {
        ModuleDefinition moduleDefinition;
        if (assemblyName == ModuleDefinition.Assembly.Name.Name)
        {
            moduleDefinition = ModuleDefinition;
        }
        else
        {

            moduleDefinition = AssemblyResolver.Resolve(assemblyName).MainModule;   
        }

        var typeDefinition = moduleDefinition.GetTypes().FirstOrDefault(x => x.FullName == typeName);
        if (typeDefinition == null)
        {
            throw new WeavingException(string.Format("Could not find type named '{0}'.", typeName));
        }
        return typeDefinition;
    }


    string GetLdString(Instruction previous)
    {
        if (previous.OpCode != OpCodes.Ldstr)
        {
            LogErrorPoint("Expected a string", previous.SequencePoint);
        }
        return (string) previous.Operand;
    }
}