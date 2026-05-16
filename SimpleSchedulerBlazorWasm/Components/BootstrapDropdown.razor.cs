using Microsoft.AspNetCore.Components;

namespace SimpleSchedulerBlazorWasm.Components;

public partial class BootstrapDropdown<TValue>
{
    [Parameter] public TValue? Value { get; set; }
    [Parameter] public EventCallback<TValue?> ValueChanged { get; set; }
    [Parameter] public IEnumerable<BootstrapDropdownItem<TValue>> Items { get; set; } = Array.Empty<BootstrapDropdownItem<TValue>>();
    [Parameter] public string Placeholder { get; set; } = "Select...";
    [Parameter] public string EmptyLabel { get; set; } = "(none)";
    [Parameter] public bool IncludeEmpty { get; set; } = true;
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Id { get; set; }

    private string? SelectedLabel
    {
        get
        {
            if (Value is null)
            {
                return null;
            }
            foreach (BootstrapDropdownItem<TValue> item in Items)
            {
                if (IsSelected(item.Value))
                {
                    return item.Label;
                }
            }
            return null;
        }
    }

    private bool IsSelected(TValue? candidate) =>
        EqualityComparer<TValue>.Default.Equals(candidate!, Value!);

    private async Task SelectAsync(TValue? value)
    {
        if (IsSelected(value))
        {
            return;
        }
        Value = value;
        await ValueChanged.InvokeAsync(value);
    }
}

public record BootstrapDropdownItem<TValue>(TValue? Value, string Label);
