namespace Veldrid.SPIRV
{
    public class MacroDefinition
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public MacroDefinition(string name)
        {
            Name = name;
        }

        public MacroDefinition(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}