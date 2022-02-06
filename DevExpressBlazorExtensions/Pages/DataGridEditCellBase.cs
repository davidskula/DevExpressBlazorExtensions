using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace DevExpressBlazorExtensions.Pages
{
    public class DataGridEditCellBase<TRow, TValue> : ComponentBase, IDataGridEditCell<TRow>
    {
        [CascadingParameter(Name = "EditContext")] 
        public IDataGridEditContext<TRow> Context { get; set; }

        [Parameter] 
        public TRow Row { get; set; }

        [Parameter] 
        public string FieldName { get; set; }

        protected bool IsInEdit { get; private set; }

        protected TValue Value
        {
            get => (TValue)Context.GetFieldValue(Row, FieldName);
            set => Context.SetFieldValue(Row, FieldName, value);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Context.RegisterCell(this);
        }

        public void Dispose()
        {
            Context.UnregisterCell(this);
        }
        
        protected void InvokeStartEdit()
        {
            Context.StartEdit(this);
        }

        public virtual void StartEdit()
        {
            if (IsInEdit)
                return;
            IsInEdit = true;
            StateHasChanged();
        }

        public virtual void EndEdit()
        {
            if (!IsInEdit)
                return;
            IsInEdit = false;
            StateHasChanged();
        }
        
        protected virtual void OnKeyDown(KeyboardEventArgs e)
        {
            switch (e.Code)
            {
                case "Enter":
                case "NumpadEnter":
                case "Tab" when !e.ShiftKey:
                case "ArrowRight" when e.ShiftKey:
                    if (!Context.StartEdit(new NextCellSelector<TRow>(this)))
                    {
                        var newRow = Context.AddRow();
                        Context.StartEdit(new FirstCellOfRow<TRow>(newRow), true);
                    }

                    break;
                case "Tab" or "ArrowLeft" when e.ShiftKey:
                    Context.StartEdit(new PrevCellSelector<TRow>(this));
                    break;
                case "ArrowUp" when e.ShiftKey:
                    Context.StartEdit(new PrevCellInColumnSelector<TRow>(this));
                    break;
                case "ArrowDown" when e.ShiftKey:
                    Context.StartEdit(new NextCellInColumnSelector<TRow>(this));
                    break;
            }
        }
    }
}