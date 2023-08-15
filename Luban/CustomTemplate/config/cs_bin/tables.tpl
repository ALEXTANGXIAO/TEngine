using Bright.Serialization;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

{{
    name = x.name
    namespace = x.namespace
    tables = x.tables
}}
namespace {{namespace}}
{
   
public sealed class {{name}}
{
    {{~for table in tables ~}}
{{~if table.comment != '' ~}}
    /// <summary>
    /// {{table.escape_comment}}
    /// </summary>
{{~end~}}
    public {{table.full_name}} {{table.name}} {get; private set; }
    {{~end~}}

    public {{name}}() { }
    
    public async UniTask LoadAsync(System.Func<string, UniTask<ByteBuf>> loader)
    {
        var tables = new System.Collections.Generic.Dictionary<string, object>();
		List<UniTask> list = new List<UniTask>();
        {{~for table in tables ~}}
		list.Add(UniTask.Create(async () =>
		{
			{{table.name}} = new {{table.full_name}}(await loader("{{table.output_data_file}}")); 
			tables.Add("{{table.full_name}}", {{table.name}});
		}));
        {{~end~}}

		await UniTask.WhenAll(list);

        {{~for table in tables ~}}
        {{table.name}}.Resolve(tables); 
        {{~end~}}
    }

    public void TranslateText(System.Func<string, string, string> translator)
    {
        {{~for table in tables ~}}
        {{table.name}}.TranslateText(translator); 
        {{~end~}}
    }
}

}