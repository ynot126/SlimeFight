#nullable enable
using Cysharp.Threading.Tasks;

public interface IActionExecution
{
    UniTask ExecuteAsync(ActionContext ctx, ActionTarget target);
}
