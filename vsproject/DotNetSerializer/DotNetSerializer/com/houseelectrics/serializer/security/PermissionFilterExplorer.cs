using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Explorer = com.houseelectrics.serializer.Explorer;
using SecurityException = System.Security.SecurityException;
using MemberInfo = System.Reflection.MemberInfo;

namespace com.houseelectrics.serializer.security
{    
    public class PermissionFilterExplorer : Explorer
    {
        private bool throwException = false;
        public bool ThrowException { get { return throwException; } set { throwException = value; } } 

        private Explorer underlyingExplorer=null;
        public Explorer UnderlyingExplorer { get { return underlyingExplorer; } set { this.underlyingExplorer = value; } }

        public NodeExpander NodeExpander { get { return underlyingExplorer.NodeExpander; } set { underlyingExplorer.NodeExpander = value; } }

        Func<string, bool> checkPermissionByName;
        /*
         * required code to check permission by name
         */
        public Func<string, bool> CheckPermissionByName { get {return checkPermissionByName; } set {checkPermissionByName=value;} }
        public void explore(object root, MoveAway down, MoveBack up, OnLeaf leaf)
        {
            if (CheckPermissionByName==null)
            {
                //todo write test for this
                throw new SecurityException("cant check permissions no Check Permission By Name code");
            }
            if (UnderlyingExplorer == null)
            {
                throw new SecurityException("cant check permissions no UnderlyingExplorer");
            }

            OnLeaf filteredOnLeaf = (from, propertyName, to, index) =>
                {
                    if (!isDeniedByPermission(from, propertyName))
                    {
                        leaf(from, propertyName, to, index);
                    }
                };

            MoveAway filteredMoveAway = ( from, propertyName, to, isIndexed, index) =>
                {
                    if (!isDeniedByPermission(from, propertyName))
                    {
                        return down(from, propertyName, to, isIndexed, index);
                    }
                    else
                    {
                        return false;
                    }
                };

            MoveBack filteredMoveBack = (from,  propertyName,  to,  isIndexed) =>
            {
                if (!isDeniedByPermission(from, propertyName))
                {
                    up(from, propertyName, to, isIndexed);
                }
            };

            UnderlyingExplorer.explore(root, filteredMoveAway, filteredMoveBack, filteredOnLeaf);
        }

        bool isDeniedByPermission(object from, string propertyName)
        {
            if (from == null) { return false; }
                Type fromType = from.GetType();
                MemberInfo []mis = fromType.GetMember(propertyName);
                if (mis.Count()<1)
                    {
                        throw new Exception("unable to locate property " + fromType.FullName + "." + propertyName);
                    }
                object[] attribute = mis[0].GetCustomAttributes(typeof(PermissionRequirementAttribute), true);
                if (attribute.Length > 0)
                    {
                    PermissionRequirementAttribute pra = (PermissionRequirementAttribute) attribute[0];
                    bool hasPermission = CheckPermissionByName(pra.PermissionName);
                    if (!hasPermission)
                        {
                        if (ThrowException)
                            {
                                throw new SecurityException("cannot access property " +
                                    fromType.FullName + "." + propertyName + " without permisson " + pra.PermissionName);
                            }
                        else
                            {
                                return true;
                            }
                        }
                    }
               return false;
        }

    }

    public static class PermissionFilterHelper
    {
        public static void injectPermissionFilter(this Object2Json object2Json, Func<string, bool> securityCheck, bool throwException)
        {
            PermissionFilterExplorer pfe = new PermissionFilterExplorer();
            pfe.ThrowException = throwException;
            object2Json.ExplorerFactory = () =>
            {
                pfe.UnderlyingExplorer = new ObjectExplorerImpl();
                pfe.CheckPermissionByName = securityCheck;
                return pfe;
            };
        }

    }

}
