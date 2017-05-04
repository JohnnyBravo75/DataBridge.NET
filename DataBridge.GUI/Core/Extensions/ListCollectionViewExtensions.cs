namespace DataBridge.GUI.Core.Extensions
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Data;

    public static class ListCollectionViewExtensions
    {
        /// <summary>
        /// Adds the new items to list collection view.
        /// </summary>
        /// <param name="theViewToUpdate">The view to update.</param>
        /// <param name="theNewList">The new list.</param>
        public static void AddNewItemsToListCollectionView(this ListCollectionView theViewToUpdate, List<object> theNewList)
        {
            theViewToUpdate.NewItemPlaceholderPosition = NewItemPlaceholderPosition.AtBeginning;

            foreach (var item in theNewList)

                theViewToUpdate.AddNewItem(item);

            theViewToUpdate.CommitNew();

            theViewToUpdate.NewItemPlaceholderPosition = NewItemPlaceholderPosition.None;
        }

        /// <summary>
        /// Adds the new item to list collection view.
        /// </summary>
        /// <param name="theViewToUpdate">The view to update.</param>
        /// <param name="theNewItem">The new item.</param>
        public static void AddNewItemToListCollectionView(this ListCollectionView theViewToUpdate, object theNewItem)
        {
            theViewToUpdate.NewItemPlaceholderPosition = NewItemPlaceholderPosition.AtBeginning;

            theViewToUpdate.AddNewItem(theNewItem);

            theViewToUpdate.CommitNew();

            theViewToUpdate.NewItemPlaceholderPosition = NewItemPlaceholderPosition.None;
        }

        /// <summary>
        /// Removes the items from list collection view.
        /// </summary>
        /// <param name="theViewToUpdate">The view to update.</param>
        /// <param name="theRemoveList">The remove list.</param>
        public static void RemoveItemsFromListCollectionView(this ListCollectionView theViewToUpdate, List<object> theRemoveList)
        {
            //Finish all other state before remove item

            if (theViewToUpdate.IsAddingNew)

                theViewToUpdate.CommitNew();

            if (theViewToUpdate.IsEditingItem)

                theViewToUpdate.CommitEdit();

            //Remove items

            foreach (var item in theRemoveList)

                theViewToUpdate.Remove(item);

            //theViewToUpdate.Refresh();
        }

        /// <summary>
        /// Removes all items from list collection view.
        /// </summary>
        /// <param name="theViewToUpdate">The view to update.</param>
        public static void RemoveAllItemsFromListCollectionView(this ListCollectionView theViewToUpdate)
        {
            //remove new item place holder

            if (theViewToUpdate.NewItemPlaceholderPosition != NewItemPlaceholderPosition.None)

                theViewToUpdate.NewItemPlaceholderPosition = NewItemPlaceholderPosition.None;

            if (theViewToUpdate.Count > 0)
            {
                int index = 0;

                var theItem = theViewToUpdate.GetItemAt(index);

                while (theItem != null)
                {
                    theViewToUpdate.Remove(theItem);

                    if (theViewToUpdate.Count > 0)

                        theItem = theViewToUpdate.GetItemAt(index);

                    else

                        theItem = null;
                }

                theViewToUpdate.Refresh();
            }
        }
    }
}