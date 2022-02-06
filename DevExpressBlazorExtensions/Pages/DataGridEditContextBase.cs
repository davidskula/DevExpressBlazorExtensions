using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Blazor;

namespace DevExpressBlazorExtensions.Pages
{
    public interface IDataGridEditContext<TRow>
    {
        void RegisterCell(IDataGridEditCell<TRow> cell);
        void UnregisterCell(IDataGridEditCell<TRow> cell);
        void StartEdit(IDataGridEditCell<TRow> cell);
        bool StartEdit(IDataGridEditCellSelector<TRow> selector, bool keep = false);
        TRow AddRow();

        object GetFieldValue(TRow row, string fieldName);
        void SetFieldValue(TRow row, string fieldName, object value);
    }

    public abstract class DataGridEditContextBase<TRow> : IDataGridEditContext<TRow>
    {
        private readonly List<IDataGridEditCell<TRow>> cellList = new List<IDataGridEditCell<TRow>>();
        private IDataGridEditCell<TRow> actualEditedCell;
        private DxDataGrid<TRow> grid;
        private IList<TRow> rowColl;
        private Func<TRow> initNewRow;
        private IDataGridEditCellSelector<TRow> activeSelector;
        
        public void Initialize(DxDataGrid<TRow> grid, IList<TRow> rowColl, Func<TRow> initNewRow)
        {
            this.grid = grid;
            this.rowColl = rowColl;
            this.initNewRow = initNewRow;
        }
        
        public void RegisterCell(IDataGridEditCell<TRow> cell)
        {
            cellList.Add(cell);
            if (activeSelector != null)
            {
                if (TryStartEdit(activeSelector))
                    activeSelector = null;
            }
        }

        public void UnregisterCell(IDataGridEditCell<TRow> cell)
        {
            cellList.Remove(cell);
            if (actualEditedCell == cell)
                actualEditedCell = null;
        }

        public void StartEdit(IDataGridEditCell<TRow> cell)
        {
            actualEditedCell?.EndEdit();
            actualEditedCell = cell;
            if (actualEditedCell != null)
            {
                actualEditedCell.StartEdit();
                grid.SetDataRowSelected(actualEditedCell.Row, true);
            }
        }

        public bool StartEdit(IDataGridEditCellSelector<TRow> selector, bool keep = false)
        {
            var ret = TryStartEdit(selector);
            if (keep && !ret)
                activeSelector = selector;
            else
                activeSelector = null;
            return ret;
        }

        private bool TryStartEdit(IDataGridEditCellSelector<TRow> selector)
        {
            var select = selector.Select(cellList);
            if (select != null)
                StartEdit(select);
            return select != null;
        }

        public TRow AddRow()
        {
            var row = initNewRow();
            rowColl.Add(row);
            return row;
        }

        public abstract object GetFieldValue(TRow row, string fieldName);

        public abstract void SetFieldValue(TRow row, string fieldName, object value);
    }

    public interface IDataGridEditCell<TRow> : IDisposable
    {
        TRow Row { get; }
        string FieldName { get; }
        
        void StartEdit();
        void EndEdit();
    }

    public interface IDataGridEditCellSelector<TRow>
    {
        IDataGridEditCell<TRow> Select(List<IDataGridEditCell<TRow>> list);
    }

    public class NextCellSelector<TRow> : IDataGridEditCellSelector<TRow>
    {
        private readonly IDataGridEditCell<TRow> actCell;

        public NextCellSelector(IDataGridEditCell<TRow> actCell)
        {
            this.actCell = actCell;
        }
        
        public IDataGridEditCell<TRow> Select(List<IDataGridEditCell<TRow>> cellList)
        {
            var index = cellList.IndexOf(actCell);
            index++;
            if (cellList.Count <= index)
                return null;
            return cellList[index];
        }
    }
    
    public class PrevCellSelector<TRow> : IDataGridEditCellSelector<TRow>
    {
        private readonly IDataGridEditCell<TRow> actCell;

        public PrevCellSelector(IDataGridEditCell<TRow> actCell)
        {
            this.actCell = actCell;
        }
        
        public IDataGridEditCell<TRow> Select(List<IDataGridEditCell<TRow>> cellList)
        {
            var index = cellList.IndexOf(actCell);
            index--;
            if (cellList.Count <= index || index < 0)
                return null;
            return cellList[index];
        }
    }

    public class NextCellInColumnSelector<TRow> : IDataGridEditCellSelector<TRow>
    {
        private readonly IDataGridEditCell<TRow> actCell;

        public NextCellInColumnSelector(IDataGridEditCell<TRow> actCell)
        {
            this.actCell = actCell;
        }
        
        public IDataGridEditCell<TRow> Select(List<IDataGridEditCell<TRow>> cellList)
        {
            var index = cellList.IndexOf(actCell);
            for (int i = index + 1; i < cellList.Count; i++)
            {
                var select = cellList[i];
                if (select.FieldName == actCell.FieldName)
                    return select;
            }
            return null;
        }
    }
    
    public class PrevCellInColumnSelector<TRow> : IDataGridEditCellSelector<TRow>
    {
        private readonly IDataGridEditCell<TRow> actCell;

        public PrevCellInColumnSelector(IDataGridEditCell<TRow> actCell)
        {
            this.actCell = actCell;
        }
        
        public IDataGridEditCell<TRow> Select(List<IDataGridEditCell<TRow>> cellList)
        {
            var index = cellList.IndexOf(actCell);
            for (int i = index - 1; i >= 0; i--)
            {
                var select = cellList[i];
                if (select.FieldName == actCell.FieldName)
                    return select;
            }
            return null;
        }
    }

    public class FirstCellOfRow<TRow> : IDataGridEditCellSelector<TRow>
    {
        private readonly TRow row;

        public FirstCellOfRow(TRow row)
        {
            this.row = row;
        }
        
        public IDataGridEditCell<TRow> Select(List<IDataGridEditCell<TRow>> list)
        {
            return list.FirstOrDefault(x => Equals(x.Row, row));
        }
    }
}