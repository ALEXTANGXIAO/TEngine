using Bright.Serialization;

{{
    name = x.name
    namespace = x.namespace
    tables = x.tables

}}
namespace {{namespace}}
{
   
public partial class {{name}}
{
    {{~for table in tables ~}}
{{~if table.comment != '' ~}}
    /// <summary>
    /// {{table.escape_comment}}
    /// </summary>
{{~end~}}
    public {{table.full_name}} {{table.name}} {get; }
    {{~end~}}

    public {{name}}(System.Func<string, ByteBuf> idxLoader,System.Func<string, ByteBuf> dataLoader)
    {
        var tables = new System.Collections.Generic.Dictionary<string, object>();
        {{~for table in tables ~}}
        {{table.name}} = new {{table.full_name}}(idxLoader("{{table.output_data_file}}"),"{{table.output_data_file}}",dataLoader); 
        tables.Add("{{table.full_name}}", {{table.name}});
        {{~end~}}

        PostInit();
        {{~for table in tables ~}}
        {{table.name}}.CacheTables(tables); 
        {{~end~}}
    }
    
    partial void PostInit();
}

}