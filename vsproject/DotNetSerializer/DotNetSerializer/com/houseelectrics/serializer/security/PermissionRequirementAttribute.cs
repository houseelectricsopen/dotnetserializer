using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.houseelectrics.serializer.security
{
    [System.AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Property )]
    public class PermissionRequirementAttribute : System.Attribute
    {
        private string permissionName;
        public string PermissionName { get { return permissionName; } }
        public PermissionRequirementAttribute(String permissionName)
        {
            this.permissionName = permissionName;
        }
    }
}
