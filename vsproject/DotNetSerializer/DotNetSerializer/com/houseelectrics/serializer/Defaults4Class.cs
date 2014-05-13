using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.houseelectrics.serializer
{
    public class Defaults4Class
    {
        string fullClassName;
        public string FullClassName { get { return fullClassName; } set {this.fullClassName=value; } } 
        // Map of related properties
        //private Dictionary<string, Defaults4Class> propertyName2NodeType = new Dictionary<string,Defaults4Class>();
        //public Dictionary<string, Defaults4Class> PropertyName2NodeType { get {return propertyName2NodeType;} }
        private Dictionary<string, object> propertyName2DefaultValue = new Dictionary<string, object>();
        public Dictionary<string, object> PropertyName2DefaultValue { get {return propertyName2DefaultValue;} }

    }
}
