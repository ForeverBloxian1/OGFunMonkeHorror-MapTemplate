using System;
using Lua;

public static class MapScriptValidator
{
    public static bool Validate(string source, out string error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(source)) return true;

        try
        {
            var state = LuaState.Create();
            state.Load(source, "MapScript");
            return true;
        }
        catch (Exception e)
        {
            error = e.Message;
            return false;
        }
    }
}
