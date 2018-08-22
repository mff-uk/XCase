using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCase.Model;
using XCase.View.Controls;
using System.Diagnostics;

namespace XCase.View
{
    /// <summary>
    /// A class for automatic layout of PSM diagrams.
    /// These diagrams have strictly tree structure where also the order of children is important,
    /// that's why user layouting is not supported and fixed automatic layout performed.
    /// </summary>
    static public class TreeLayout
    {
        /// <summary>
        /// Space on canvas between two neighbouring subtrees.
        /// </summary>
        private const int horizontalSpace = 20;

        /// <summary>
        /// Space on canvas between a parent and its children.
        /// </summary>
        private const int verticalSpace = 40;

        /// <summary>
        /// Indicates whether layouting is actually active.
        /// </summary>
        private static bool active = true;

        /// <summary>
        /// Completely performs layouting of a PSM diagram.
        /// </summary>
        /// <param name="canvas">The diagram (resp. canvas) to be layouted.</param>
        public static void LayoutDiagram(XCaseCanvas canvas)
        {
            if (active)
            {
                PSMDiagram diagram = canvas.Diagram as PSMDiagram;
                if (diagram != null)
                {
                    Debug.WriteLine("Layouting...");
                    double left = TreeLayout.horizontalSpace;
                    foreach (PSMSuperordinateComponent aClass in diagram.Roots)
                    {
                        left += TreeLayout.DrawTree(canvas, aClass, TreeLayout.verticalSpace / 2, left) + TreeLayout.horizontalSpace;
                    }

                    foreach (PSMDiagramReference reference in diagram.DiagramReferences)
                    {
                        left += TreeLayout.DrawTree(canvas, reference, TreeLayout.verticalSpace / 2, left) + TreeLayout.horizontalSpace;
                    }
                }
            }
        }

        /// <summary>
        /// Switches layouting on.
        /// </summary>
        public static void SwitchOn()
        {
            active = true;
        }

        /// <summary>
        /// Suppresses layouting.
        /// Used e.g. when loading diagrams to prevent layouting after each added element.
        /// </summary>
        public static void SwitchOff()
        {
            active = false;
        }

        /// <summary>
        /// Draws all children of given root element and counts width of its subtree.
        /// </summary>
        /// <param name="diagram">Diagram to be layouted</param>
        /// <param name="root">Root element of layouted subtree</param>
        /// <param name="top">Location of the upper border of the root's children</param>
        /// <param name="left">Location of the left border of the entire subtree</param>
        /// <returns>Width of the subtree (root not included)</returns>
        private static double DrawSubtree(XCaseCanvas diagram, NamedElement root, double top, double left)
        {
            double right = left;
            if (root is PSMSuperordinateComponent)
            {
                PSMSuperordinateComponent aClass = root as PSMSuperordinateComponent;
                NamedElement child = null;
                if (aClass.Components.Count > 0)
                {
                    foreach (PSMSubordinateComponent component in aClass.Components)
                    {
                        if (component is PSMAssociation)
                        {
                            child = (component as PSMAssociation).Child;
                        }
                        else
                        {
                            if (component is PSMAttributeContainer)
                            {
                                child = component as PSMAttributeContainer;
                            }
                            else
                            {
                                if (component is PSMContentChoice || component is PSMContentContainer)
                                {
                                    child = component as PSMSuperordinateComponent;
                                }
                            }
                        }
                        if (child != null)
                        {
                            right += DrawTree(diagram, child, top, right) + horizontalSpace;
                        }
                        child = null;
                    }
                }
                if (aClass is PSMClass && (aClass as PSMClass).Specifications.Count > 0)
                {
                    foreach (Generalization specification in (aClass as PSMClass).Specifications)
                    {
                        right += DrawTree(diagram, specification.Specific, top, right) + horizontalSpace;
                    }
                }
                if (right != left) right -= horizontalSpace;
            }
            else
            {
                if (root is PSMClassUnion)
                {
                    PSMClassUnion classUnion = root as PSMClassUnion;
                    if (classUnion.Components.Count > 0)
                    {
                        foreach (PSMAssociationChild component in classUnion.Components)
                        {
                            right += DrawTree(diagram, component, top, right) + horizontalSpace;
                        }
                        right = right - horizontalSpace;
                    }
                }
            }
            return right - left;
        }

        /// <summary>
        /// Draws given root element and all its children and counts width of its subtree.
        /// </summary>
        /// <param name="diagram">Diagram to be layouted</param>
        /// <param name="root">Root element of layouted subtree</param>
        /// <param name="top">Location of the upper border of the root</param>
        /// <param name="left">Location of the left border of the entire subtree</param>
        /// <returns>Width of the subtree (root included)</returns>
        private static double DrawTree(XCaseCanvas diagram, NamedElement root, double top, double left)
        {
            if (!diagram.ElementRepresentations.IsElementPresent(root)) return -horizontalSpace;
            XCaseViewBase element = (diagram.ElementRepresentations[root] as XCaseViewBase);
            double height = element.ActualHeight;
            double width = element.ActualWidth;
            double right = left + TreeLayout.DrawSubtree(diagram, root, top + height + verticalSpace, left);
            if (right == left)
            {
                right = left + width;
            }
            else
            {
                if (right < left + width)
                {
                    double subtreeWidth = right - left;
                    TreeLayout.DrawSubtree(diagram, root, top + height + verticalSpace, left + (width - subtreeWidth) / 2);
                    right = left + width;
                }
            }
            element.X = Math.Round((right + left) / 2 - width / 2);
            element.ViewHelper.X = element.X;
            element.Y = Math.Round(top);
            element.ViewHelper.Y = element.Y;
            return right - left;
        }
    }
}
