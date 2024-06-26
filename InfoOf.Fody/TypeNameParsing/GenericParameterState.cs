class GenericParameterState(TypeNameState parentState) : IState
{
    IList<AssemblyNameState> genericParameters = new List<AssemblyNameState>();

    public IState OnGenericTypeStart()
    {
        if (genericParameters.Count != 0)
        {
            throw new WeavingException("Unexpected generic type start");
        }

        return new TokenReadState(new AssemblyNameState(this));
    }

    public IState OnGenericTypeEnd(IState currentState)
    {
        if (currentState is GenericParameterState)
        {
            return parentState.OnGenericTypeEnd(currentState);
        }

        genericParameters.Add((AssemblyNameState)currentState);
        return this;
    }

    public IState OnAssemblyNameSeparator() =>
        throw new WeavingException("Unexpected assembly name separator");

    public IState OnGenericParamSeparator(IState currentState)
    {
        if (currentState is GenericParameterState)
        {
            return parentState.OnGenericParamSeparator(currentState);
        }

        genericParameters.Add((AssemblyNameState)currentState);
        return new TokenReadState(new AssemblyNameState(this));
    }

    public IState OnReadToken(string token) =>
        throw new WeavingException("Unexpected name token");

    public string Token { get; set; }

    public IState Validate()
    {
        parentState.Validate();
        return this;
    }

    public ParsedTypeName Build() =>
        new()
        {
            Assembly = Token,
            TypeName = parentState.Token,
            GenericParameters = genericParameters.Select(_ => _.Build()).ToList()
        };
}