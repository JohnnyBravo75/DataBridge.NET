namespace DataBridge.GUI.Core.Extensions
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using DataBridge.Extensions;
    using GUI.Core.Utils;

    public static class DataGridExtensions
    {
        public static DataGridRow GetSelectedRow(this DataGrid grid)
        {
            if (grid == null)
            {
                throw new System.ArgumentNullException("grid");
            }

            return (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(grid.SelectedItem);
        }

        /// <summary>
        /// Gets the <c>DataGridRow</c> at the specified index.
        /// </summary>
        /// <param name="dataGrid">The <c>DataGrid</c>.</param>
        /// <param name="index">The index of the <c>DataGridRow</c>.</param>
        /// <returns>The <c>DataGridRow</c> at the specified index.</returns>
        public static DataGridRow GetRow(this DataGrid grid, int index)
        {
            if (grid == null)
            {
                throw new System.ArgumentNullException("grid");
            }

            DataGridRow row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index) as DataGridRow;
            if (row == null && grid.EnableRowVirtualization && grid.IsLoaded && grid.IsVisible)
            {
                // May be virtualized, bring into view and try again.
                grid.ScrollIntoView(grid.Items[index]);
                grid.UpdateLayout();
                row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index) as DataGridRow;
            }

            return row;
        }

        /// <summary>
        /// Gets the <c>DataGridRow</c> for the specified item.
        /// </summary>
        /// <param name="dataGrid">The <c>DataGrid</c>.</param>
        /// <param name="item">The item.</param>
        /// <returns>The <c>DataGridRow</c> for the specified item.</returns>
        public static DataGridRow GetRow(this DataGrid grid, object item)
        {
            if (grid == null)
            {
                throw new System.ArgumentNullException("grid");
            }

            int index = grid.Items.IndexOf(item);

            return ((index != -1) ? grid.GetRow(index) : null);
        }

        public static DataGridCell GetCell(this DataGrid grid, DataGridRow row, int column)
        {
            if (grid == null)
            {
                throw new System.ArgumentNullException("grid");
            }

            if (row != null)
            {
                DataGridCellsPresenter presenter = VisualTreeUtil.GetVisualChild<DataGridCellsPresenter>(row);

                if (presenter == null)
                {
                    grid.ScrollIntoView(row, grid.Columns[column]);
                    presenter = VisualTreeUtil.GetVisualChild<DataGridCellsPresenter>(row);
                }

                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                return cell;
            }
            return null;
        }

        public static DataGridCell GetCell(this DataGrid grid, int row, int column)
        {
            if (grid == null)
            {
                throw new System.ArgumentNullException("grid");
            }

            DataGridRow rowContainer = grid.GetRow(row);
            return grid.GetCell(rowContainer, column);
        }

        public static void GenerateColumnsFromList<T>(this DataGrid grid, IEnumerable<T> objectList)
        {
            if (objectList.Any())
            {
                objectList.First().GetType().GetProperties().ForEach(a =>
                {
                    grid.AddColumn(a.Name, a.Name, a.PropertyType);
                });
            }
        }

        /// <summary>
        /// Adds a column to the datagrid an binds it.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="name">The column name.</param>
        /// <param name="header">The column header/caption.</param>
        /// <param name="dataType">datatype of the column.</param>
        /// <exception cref="System.ArgumentNullException">grid</exception>
        public static void AddColumn(this DataGrid grid, string name, string header = "", System.Type dataType = null)
        {
            if (grid == null)
            {
                throw new System.ArgumentNullException("grid");
            }

            if (string.IsNullOrEmpty(header))
            {
                header = name;
            }

            if (dataType == null)
            {
                dataType = typeof(string);
            }

            DataGridBoundColumn gridColumn = null;

            if (dataType == typeof(string))
            {
                gridColumn = new DataGridTextColumn();
            }
            else if (dataType == typeof(bool) || dataType == typeof(bool?))
            {
                gridColumn = new DataGridCheckBoxColumn();
            }
            else
            {
                gridColumn = new DataGridTextColumn();
            }

            if (gridColumn != null)
            {
                // gridColumn.SetValue(FrameworkElement.DataContextProperty, grid.DataContext);
                //var colHeader = gridColumn.Header as FrameworkElement;
                //if (colHeader != null)
                //{
                //    colHeader.SetValue(FrameworkElement.DataContextProperty, grid.DataContext);
                //}

                gridColumn.Header = header;
                gridColumn.Binding = new Binding(name)
                {
                    Mode = System.Windows.Data.BindingMode.TwoWay,
                    UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
                };

                grid.Columns.Add(gridColumn);
            }
        }

        /// <summary>
        /// Synchronizes the columns of a datagrid and a datatable
        /// Needs to be done, because later created columns in a datatbale do not
        /// get visible in the datagrid (no Rebind, Items.Refresh(),... helps),
        /// so they have to be created manually and bound to the datatable
        /// </summary>
        /// <param name="grid">the datagrid</param>
        /// <param name="table">the datatable</param>
        public static void SynchronizeColumns(this DataGrid grid, DataTable table)
        {
            if (grid == null)
            {
                throw new System.ArgumentNullException("grid");
            }

            // add new columns
            foreach (DataColumn column in table.Columns)
            {
                // DataTable-> DataGrid: add columns to the datagrid, which not exist
                if (!grid.Columns.Any(x => x.Header.ToString() == column.Caption))
                {
                    grid.AddColumn(column.ColumnName, column.Caption, column.DataType);
                }
            }

            // remove not existing columns
            foreach (DataGridColumn column in grid.Columns.ToList())
            {
                // DataGrid -> DataTable: remove columns in the datagrid, which not exist
                if (!table.Columns.Cast<DataColumn>().Any(x => x.Caption == column.Header.ToString()))
                {
                    grid.Columns.Remove(column);
                }
            }
        }
    }
}