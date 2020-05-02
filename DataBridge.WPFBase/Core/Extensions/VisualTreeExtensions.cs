namespace DataBridge.GUI.Core.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// Erweiterung für den VisualTree (Linq-Unterstützung)
    /// </summary>
    public static class VisualTreeExtensions
    {
        /// <summary>
        /// Gibt die Children nach TYP an
        /// </summary>
        /// <typeparam name="T">Der Typ der children</typeparam>
        /// <param name="element">Das Element</param>
        /// <returns>Eine Liste von children</returns>
        public static List<T> GetChildrenByType<T>(this UIElement element)
        where T : UIElement
        {
            return GetChildrenByType<T>(element, null);
        }

        /// <summary>
        /// Gibt an ob Kinder existieren
        /// </summary>
        /// <typeparam name="T">Der Typ der Kinder</typeparam>
        /// <param name="element">Das Element</param>
        /// <param name="condition">Eine Bedingung</param>
        /// <returns>gibt zurück ob Kinder existieren</returns>
        public static bool HasChildrenByType<T>(this UIElement element, Func<T, bool> condition)
                where T : UIElement
        {
            return GetChildrenByType<T>(element, condition).Count != 0;
        }

        /// <summary>
        /// Gibt die Children nach TYP an
        /// </summary>
        /// <typeparam name="T">Der Typ der children</typeparam>
        /// <param name="element">Das Element</param>
        /// <param name="condition">Die Bedingung</param>
        /// <returns>Eine Liste von children</returns>
        public static List<T> GetChildrenByType<T>(this UIElement element, Func<T, bool> condition)
            where T : UIElement
        {
            List<T> results = new List<T>();
            GetChildrenByType<T>(element, condition, results);
            return results;
        }

        /// <summary>
        /// Gibt die Children nach TYP an
        /// </summary>
        /// <typeparam name="T">Der Typ der children</typeparam>
        /// <param name="element">Das Element</param>
        /// <param name="condition">Die Bedingung</param>
        /// <param name="results">Eine Ergebnisliste</param>
        private static void GetChildrenByType<T>(UIElement element, Func<T, bool> condition, List<T> results)
            where T : UIElement
        {
            if (element != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
                {
                    UIElement child = VisualTreeHelper.GetChild(element, i) as UIElement;
                    if (child != null)
                    {
                        T t = child as T;
                        if (t != null)
                        {
                            if (condition == null)
                            {
                                results.Add(t);
                            }
                            else if (condition(t))
                            {
                                results.Add(t);
                            }
                        }

                        GetChildrenByType<T>(child, condition, results);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a collection of descendant elements.
        /// </summary>
        public static IEnumerable<DependencyObject> Descendants(this DependencyObject item)
        {
            ILinqTree<DependencyObject> adapter = new VisualTreeAdapter(item);
            foreach (var child in adapter.Children())
            {
                yield return child;

                foreach (var grandChild in child.Descendants())
                {
                    yield return grandChild;
                }
            }
        }

        /// <summary>
        /// Returns a collection containing this element and all descendant elements.
        /// </summary>
        public static IEnumerable<DependencyObject> DescendantsAndSelf(this DependencyObject item)
        {
            yield return item;

            foreach (var child in item.Descendants())
            {
                yield return child;
            }
        }

        /// <summary>
        /// Returns a collection of ancestor elements.
        /// </summary>
        public static IEnumerable<DependencyObject> Ancestors(this DependencyObject item)
        {
            ILinqTree<DependencyObject> adapter = new VisualTreeAdapter(item);

            var parent = adapter.Parent;
            while (parent != null)
            {
                yield return parent;
                adapter = new VisualTreeAdapter(parent);
                parent = adapter.Parent;
            }
        }

        /// <summary>
        /// Returns a collection containing this element and all ancestor elements.
        /// </summary>
        public static IEnumerable<DependencyObject> AncestorsAndSelf(this DependencyObject item)
        {
            yield return item;

            foreach (var ancestor in item.Ancestors())
            {
                yield return ancestor;
            }
        }

        /// <summary>
        /// Returns a collection of child elements.
        /// </summary>
        public static IEnumerable<DependencyObject> Elements(this DependencyObject item)
        {
            ILinqTree<DependencyObject> adapter = new VisualTreeAdapter(item);
            foreach (var child in adapter.Children())
            {
                yield return child;
            }
        }

        /// <summary>
        /// Returns a collection of the sibling elements before this node, in document order.
        /// </summary>
        public static IEnumerable<DependencyObject> ElementsBeforeSelf(this DependencyObject item)
        {
            if (item.Ancestors().FirstOrDefault() == null)
                yield break;
            foreach (var child in item.Ancestors().First().Elements())
            {
                if (child.Equals(item))
                    break;
                yield return child;
            }
        }

        /// <summary>
        /// Returns a collection of the after elements after this node, in document order.
        /// </summary>
        public static IEnumerable<DependencyObject> ElementsAfterSelf(this DependencyObject item)
        {
            if (item.Ancestors().FirstOrDefault() == null)
                yield break;
            bool afterSelf = false;
            foreach (var child in item.Ancestors().First().Elements())
            {
                if (afterSelf)
                    yield return child;

                if (child.Equals(item))
                    afterSelf = true;
            }
        }

        /// <summary>
        /// Returns a collection containing this element and all child elements.
        /// </summary>
        public static IEnumerable<DependencyObject> ElementsAndSelf(this DependencyObject item)
        {
            yield return item;

            foreach (var child in item.Elements())
            {
                yield return child;
            }
        }

        /// <summary>
        /// Returns a collection of descendant elements which match the given type.
        /// </summary>
        public static IEnumerable<DependencyObject> Descendants<T>(this DependencyObject item)
        {
            return item.Descendants().Where(i => i is T).Cast<DependencyObject>();
        }

        /// <summary>
        /// Returns a collection of the sibling elements before this node, in document order
        /// which match the given type.
        /// </summary>
        public static IEnumerable<DependencyObject> ElementsBeforeSelf<T>(this DependencyObject item)
        {
            return item.ElementsBeforeSelf().Where(i => i is T).Cast<DependencyObject>();
        }

        /// <summary>
        /// Returns a collection of the after elements after this node, in document order
        /// which match the given type.
        /// </summary>
        public static IEnumerable<DependencyObject> ElementsAfterSelf<T>(this DependencyObject item)
        {
            return item.ElementsAfterSelf().Where(i => i is T).Cast<DependencyObject>();
        }

        /// <summary>
        /// Returns a collection containing this element and all descendant elements
        /// which match the given type.
        /// </summary>
        public static IEnumerable<DependencyObject> DescendantsAndSelf<T>(this DependencyObject item)
        {
            return item.DescendantsAndSelf().Where(i => i is T).Cast<DependencyObject>();
        }

        /// <summary>
        /// Returns a collection of ancestor elements which match the given type.
        /// </summary>
        public static IEnumerable<DependencyObject> Ancestors<T>(this DependencyObject item)
        {
            return item.Ancestors().Where(i => i is T).Cast<DependencyObject>();
        }

        /// <summary>
        /// Returns a collection containing this element and all ancestor elements
        /// which match the given type.
        /// </summary>
        public static IEnumerable<DependencyObject> AncestorsAndSelf<T>(this DependencyObject item)
        {
            return item.AncestorsAndSelf().Where(i => i is T).Cast<DependencyObject>();
        }

        /// <summary>
        /// Returns a collection of child elements which match the given type.
        /// </summary>
        public static IEnumerable<DependencyObject> Elements<T>(this DependencyObject item)
        {
            return item.Elements().Where(i => i is T).Cast<DependencyObject>();
        }

        /// <summary>
        /// Returns a collection containing this element and all child elements.
        /// which match the given type.
        /// </summary>
        public static IEnumerable<DependencyObject> ElementsAndSelf<T>(this DependencyObject item)
        {
            return item.ElementsAndSelf().Where(i => i is T).Cast<DependencyObject>();
        }
    }

    /// <summary>
    /// Adapts a DependencyObject to provide methods required for generate
    /// a Linq To Tree API
    /// </summary>
    public class VisualTreeAdapter : ILinqTree<DependencyObject>
    {
        private readonly DependencyObject _item;

        /// <summary>
        ///
        /// </summary>
        /// <param name="item"></param>
        public VisualTreeAdapter(DependencyObject item)
        {
            this._item = item;
        }

        /// <summary>
        /// Gibt eine Liste mit den Kinderobjekte im Visualtree zurück
        /// </summary>
        /// <returns>die Kinderobjekte</returns>
        public IEnumerable<DependencyObject> Children()
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(this._item);
            for (int i = 0; i < childrenCount; i++)
            {
                yield return VisualTreeHelper.GetChild(this._item, i);
            }
        }

        /// <summary>
        /// das Vaterobjekt im Visualtree
        /// </summary>
        public DependencyObject Parent
        {
            get
            {
                return VisualTreeHelper.GetParent(this._item);
            }
        }
    }

    /// <summary>
    /// Defines an interface that must be implemented to generate the LinqToTree methods
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ILinqTree<T>
    {
        /// <summary>
        /// die Kinderobjekte
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> Children();

        /// <summary>
        /// der Parent
        /// </summary>
        T Parent { get; }
    }

    /// <summary>
    ///
    /// </summary>
    public static class EnumerableTreeExtensions
    {
        /// <summary>
        /// Applies the given function to each of the items in the supplied
        /// IEnumerable.
        /// </summary>
        private static IEnumerable<DependencyObject> DrillDown(this IEnumerable<DependencyObject> items,
            Func<DependencyObject, IEnumerable<DependencyObject>> function)
        {
            foreach (var item in items)
            {
                foreach (var itemChild in function(item))
                {
                    yield return itemChild;
                }
            }
        }

        /// <summary>
        /// Applies the given function to each of the items in the supplied
        /// IEnumerable, which match the given type.
        /// </summary>
        public static IEnumerable<DependencyObject> DrillDown<T>(this IEnumerable<DependencyObject> items,
            Func<DependencyObject, IEnumerable<DependencyObject>> function)
            where T : DependencyObject
        {
            foreach (var item in items)
            {
                foreach (var itemChild in function(item))
                {
                    if (itemChild is T)
                    {
                        yield return (T)itemChild;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a collection of descendant elements.
        /// </summary>
        public static IEnumerable<DependencyObject> Descendants(this IEnumerable<DependencyObject> items)
        {
            return items.DrillDown(i => i.Descendants());
        }

        /// <summary>
        /// Returns a collection containing this element and all descendant elements.
        /// </summary>
        public static IEnumerable<DependencyObject> DescendantsAndSelf(this IEnumerable<DependencyObject> items)
        {
            return items.DrillDown(i => i.DescendantsAndSelf());
        }

        /// <summary>
        /// Returns a collection of ancestor elements.
        /// </summary>
        public static IEnumerable<DependencyObject> Ancestors(this IEnumerable<DependencyObject> items)
        {
            return items.DrillDown(i => i.Ancestors());
        }

        /// <summary>
        /// Returns a collection containing this element and all ancestor elements.
        /// </summary>
        public static IEnumerable<DependencyObject> AncestorsAndSelf(this IEnumerable<DependencyObject> items)
        {
            return items.DrillDown(i => i.AncestorsAndSelf());
        }

        /// <summary>
        /// Returns a collection of child elements.
        /// </summary>
        public static IEnumerable<DependencyObject> Elements(this IEnumerable<DependencyObject> items)
        {
            return items.DrillDown(i => i.Elements());
        }

        /// <summary>
        /// Returns a collection containing this element and all child elements.
        /// </summary>
        public static IEnumerable<DependencyObject> ElementsAndSelf(this IEnumerable<DependencyObject> items)
        {
            return items.DrillDown(i => i.ElementsAndSelf());
        }

        /// <summary>
        /// Returns a collection of descendant elements which match the given type.
        /// </summary>
        public static IEnumerable<DependencyObject> Descendants<T>(this IEnumerable<DependencyObject> items)
            where T : DependencyObject
        {
            return items.DrillDown<T>(i => i.Descendants());
        }

        /// <summary>
        /// Returns a collection containing this element and all descendant elements.
        /// which match the given type.
        /// </summary>
        public static IEnumerable<DependencyObject> DescendantsAndSelf<T>(this IEnumerable<DependencyObject> items)
            where T : DependencyObject
        {
            return items.DrillDown<T>(i => i.DescendantsAndSelf());
        }

        /// <summary>
        /// Returns a collection of ancestor elements which match the given type.
        /// </summary>
        public static IEnumerable<DependencyObject> Ancestors<T>(this IEnumerable<DependencyObject> items)
            where T : DependencyObject
        {
            return items.DrillDown<T>(i => i.Ancestors());
        }

        /// <summary>
        /// Returns a collection containing this element and all ancestor elements.
        /// which match the given type.
        /// </summary>
        public static IEnumerable<DependencyObject> AncestorsAndSelf<T>(this IEnumerable<DependencyObject> items)
            where T : DependencyObject
        {
            return items.DrillDown<T>(i => i.AncestorsAndSelf());
        }

        /// <summary>
        /// Returns a collection of child elements which match the given type.
        /// </summary>
        public static IEnumerable<DependencyObject> Elements<T>(this IEnumerable<DependencyObject> items)
            where T : DependencyObject
        {
            return items.DrillDown<T>(i => i.Elements());
        }

        /// <summary>
        /// Returns a collection containing this element and all child elements.
        /// which match the given type.
        /// </summary>
        public static IEnumerable<DependencyObject> ElementsAndSelf<T>(this IEnumerable<DependencyObject> items)
            where T : DependencyObject
        {
            return items.DrillDown<T>(i => i.ElementsAndSelf());
        }
    }
}