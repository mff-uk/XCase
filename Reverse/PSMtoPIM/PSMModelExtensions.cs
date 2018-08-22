using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCase.Model;

namespace XCase.Reverse
{
    internal static class GetAllExtensions
    {
        #region Get All SubElements

        static List<PSMElement> GetAllPSMSubElements(this PSMElement E, bool IncludeCurrent)
        {
            List<PSMElement> OC = new List<PSMElement>();

            GetSubElements(OC, E);
            if (!IncludeCurrent) OC.Remove(E);

            return OC;
        }

        static void GetSubElements(List<PSMElement> OC, PSMElement E)
        {
            if (E is PSMSuperordinateComponent)
            {
                GetSubElements(OC, E as PSMSuperordinateComponent);
            }
            else if (E is PSMAssociation)
            {
                GetSubElements(OC, E as PSMAssociation);
            }
            else if (E is PSMAttributeContainer)
            {
                GetSubElements(OC, E as PSMAttributeContainer);
            }
        }

        static void GetSubElements(List<PSMElement> OC, PSMSuperordinateComponent E)
        {
            foreach (PSMSubordinateComponent C in E.Components) GetSubElements(OC, C);
            OC.Add(E);
        }

        static void GetSubElements(List<PSMElement> OC, PSMAssociation E)
        {
            GetSubElements(OC, E.Child);
            OC.Add(E);
        }

        static void GetSubElements(List<PSMElement> OC, PSMAttributeContainer E)
        {
            OC.Add(E);
        }

        #endregion

        /// <summary>
        /// Gets all PSM Classes in the subtree of <paramref name="E"/>
        /// </summary>
        /// <param name="E">PSMElement which is the root of the searched subtree</param>
        /// <returns>List of PSMClasses</returns>
        public static List<PSMClass> GetAllPSMSubClasses(this PSMElement E)
        {
            return GetAllPSMSubElements(E, false).Where<PSMElement>(El => El is PSMClass).Cast<PSMClass>().ToList<PSMClass>();
        }

        /// <summary>
        /// Gets all leaf PSMClasses in the subtree of <paramref name="E"/>
        /// </summary>
        /// <param name="E">Root of the searched subtree</param>
        /// <returns>List of PSMClasses that are leaves (in the sense of PSMClasses only) of the subtree of <paramref name="E"/></returns>
        public static List<PSMClass> GetAllPSMClassLeaves(this PSMElement E)
        {
            List<PSMClass> AllSubClasses = E.GetAllPSMSubClasses();
            List<PSMClass> Leaves = AllSubClasses.Where<PSMClass>(C => C.GetAllPSMSubClasses().Count == 0).ToList<PSMClass>();
            return Leaves;
        }

        
        /// <summary>
        /// Gets the nearest PSMClass on the path from <paramref name="E"/> to a root
        /// </summary>
        /// <param name="E">The PSMElement to start the search in</param>
        /// <returns>PSMClass that is the parent</returns>
        public static PSMClass GetPSMClassParent(this PSMElement E)
        {
            PSMSuperordinateComponent parent;
            if (E is PSMSubordinateComponent)
                parent = (E as PSMSubordinateComponent).Parent;
            else if (E is PSMClass)
                parent = (E as PSMClass).ParentAssociation == null ? null : (E as PSMClass).ParentAssociation.Parent;
            else parent = null;

            while (parent != null && !(parent is PSMClass))
            {
                if (parent is PSMSubordinateComponent)
                {
                    parent = (parent as PSMSubordinateComponent).Parent;
                }
                else parent = null;
            }
            return parent as PSMClass;
        }
        
        /// <summary>
        /// Gets all PSMElements on the path from <paramref name="E"/> to the root of the PSM Diagram
        /// </summary>
        /// <param name="E">The PSMElement to start the search in</param>
        /// <returns>List of PSMElement ancestors</returns>
        public static List<PSMElement> GetAllAncestors(this PSMElement E)
        {
            List<PSMElement> Ancestors = new List<PSMElement>();
            PSMElement current = E;
            while (current != null)
            {
                if (current is PSMSubordinateComponent)
                {
                    current = (current as PSMSubordinateComponent).Parent;
                }
                else if (current is PSMClass)
                {
                    current = (current as PSMClass).ParentAssociation;
                    if (current != null)
                    {
                        Ancestors.Add(current);
                        current = (current as PSMAssociation).Parent;
                    }
                }
                if (current != null) Ancestors.Add(current);
            }
            return Ancestors;
        }

        /// <summary>
        /// Gets all PSMClasses on the path from <paramref name="E"/> to the root of the PSM Diagram
        /// </summary>
        /// <param name="E">The PSMElement to start the search in</param>
        /// <returns>List of PSMClass ancestors</returns>
        public static List<PSMClass> GetAllPSMClassAncestors(this PSMElement E)
        {
            return E.GetAllAncestors().OfType<PSMClass>().ToList<PSMClass>();
        }

        /// <summary>
        /// Gets all PSMClass siblings of <paramref name="C"/> (skipping other components of the PSM Diagram)
        /// </summary>
        /// <param name="C">The PSMClass to get the siblings of</param>
        /// <returns>List of PSMClass siblings</returns>
        public static List<PSMClass> GetAllPSMClassSiblings(this PSMClass C)
        {
            if (C.ParentAssociation == null) return new List<PSMClass>();
            PSMSuperordinateComponent parent = C.ParentAssociation.Parent;
            while (!(parent is PSMClass)) parent = (parent as PSMSubordinateComponent).Parent;
            List<PSMClass> Siblings = parent.GetDirectPSMSubClasses();
            Siblings.Remove(C);
            return Siblings;
        }

        /// <summary>
        /// Gets all previous PSMClass siblings of <paramref name="C"/> (skipping other components of the PSM Diagram)
        /// </summary>
        /// <param name="C">The PSMClass to get the previous siblings of</param>
        /// <returns>List of previous PSMClass siblings</returns>
        public static List<PSMClass> GetAllPSMClassPreviousSiblings(this PSMClass C)
        {
            if (C.ParentAssociation == null) return new List<PSMClass>();
            PSMSuperordinateComponent parent = C.ParentAssociation.Parent;
            while (!(parent is PSMClass)) parent = (parent as PSMSubordinateComponent).Parent;
            List<PSMClass> Siblings = parent.GetDirectPSMSubClasses();
            int idx = Siblings.IndexOf(C);
            List<PSMClass> PreviousSiblings = Siblings.Where<PSMClass>(S => Siblings.IndexOf(S) < idx).ToList<PSMClass>();
            return PreviousSiblings;
        }

        /// <summary>
        /// Gets all following PSMClass siblings of <paramref name="C"/> (skipping other components of the PSM Diagram)
        /// </summary>
        /// <param name="C">The PSMClass to get the following siblings of</param>
        /// <returns>List of following PSMClass siblings</returns>
        public static List<PSMClass> GetAllPSMClassFollowingSiblings(this PSMClass C)
        {
            if (C.ParentAssociation == null) return new List<PSMClass>();
            PSMSuperordinateComponent parent = C.ParentAssociation.Parent;
            while (!(parent is PSMClass)) parent = (parent as PSMSubordinateComponent).Parent;
            List<PSMClass> Siblings = parent.GetDirectPSMSubClasses();
            int idx = Siblings.IndexOf(C);
            List<PSMClass> FollowingSiblings = Siblings.Where<PSMClass>(S => Siblings.IndexOf(S) > idx).ToList<PSMClass>();
            return FollowingSiblings;
        }

        /// <summary>
        /// Gets first PSM Classes on all paths from <paramref name="E"/> to leafs in the subtree of <paramref name="E"/>
        /// </summary>
        /// <param name="E">PSMElement which is the root of the searched subtree</param>
        /// <returns>List of PSMClasses</returns>
        public static List<PSMClass> GetDirectPSMSubClasses(this PSMElement E)
        {
            List<PSMClass> OC = new List<PSMClass>();
            if (E is PSMClass)
            {
                foreach (PSMSubordinateComponent C in (E as PSMClass).Components)
                    GetDirectSubPSMClasses(OC, C);
            }
            else GetDirectSubPSMClasses(OC, E);
            return OC;
        }

        #region Get Direct SubPSMClasses
        static void GetDirectSubPSMClasses(List<PSMClass> OC, PSMElement E)
        {
            if (E is PSMClass)
            {
                GetDirectSubPSMClasses(OC, E as PSMClass);
            }
            else if (E is PSMSuperordinateComponent)
            {
                GetDirectSubPSMClasses(OC, E as PSMSuperordinateComponent);
            }
            else if (E is PSMAssociation)
            {
                GetDirectSubPSMClasses(OC, E as PSMAssociation);
            }
        }
        static void GetDirectSubPSMClasses(List<PSMClass> OC, PSMClass E)
        {
            OC.Add(E);
        }
        static void GetDirectSubPSMClasses(List<PSMClass> OC, PSMSuperordinateComponent E)
        {
            foreach (PSMSubordinateComponent C in E.Components) GetDirectSubPSMClasses(OC, C);
        }
        static void GetDirectSubPSMClasses(List<PSMClass> OC, PSMAssociation E)
        {
            GetDirectSubPSMClasses(OC, E.Child);
        }

        #endregion
    }
}
