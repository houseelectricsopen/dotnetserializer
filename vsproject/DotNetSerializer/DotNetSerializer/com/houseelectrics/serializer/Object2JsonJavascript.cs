using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.houseelectrics.serializer
{
    public class Object2JsonJavascript
    {
        //todo parameterise resolve leaf default function based on these
#pragma warning disable  0414
        string visitedProperty = "_visited_";
#pragma warning restore  0414
        public const string Type2DefaultsMapPlaceholder = "_type2Defaults__"; 
        public const string ResolveLeafDefaultFunction =
@"function _resolveLeafDefaults(o)
        {
              if (o._visited_) return o;
              var typeAlias = o._t_;
              if (!typeAlias) return o;
              for (p in o)
                 {    
                 if (p=='_t_') continue;
                 var val = o[p];             
                 // if its an array iterate and explore
                 if (typeof(val._a_)!='undefined') 
                    {
                       for (var done=0; done<val.length; done++)
                            {
                            _resolveLeafDefaults(val[done]);
                            }
                     continue;                     
                    }
                  else
                  {
                  if (val._t_)
                              {
                                  _resolveLeafDefaults(o[p]);                       
                              }

                   }   
                 }
              var _type2Defaults_ = _typeAlias2LeafDefaults_[typeAlias].propertyName2DefaultValue;
              // do the defaults
               for (var p in _type2Defaults_)
                            {
                            if (typeof(o[p])==='undefined') {o[p]=_type2Defaults_[p];}
                            }
              o._visited_=true;
              return o;
        }";

public static string getObjectResolverJS(string ObjectResolverFunctionName, string IdTag, string ReferenceTag)
        {
            String js = @"function " + ObjectResolverFunctionName + @"(o, ref2o)
{
if (!o) return;
if (!ref2o) ref2o = {};
function perSubO(o, subo, ref2o, suboKey)
   {
       if (subo && subo._ref_)
         {
          o[suboKey]=ref2o[subo._ref_];
         }             
       else if (subo && subo._id_)
         {
            ref2o[subo._id_]=subo;
            resolveRefs(subo, ref2o);
         }              
   }
//is array ?
//this condition is superfluous since this sam code write the array !
if ((o._a_) /*|| (Array && Array.isArray && Array.isArray(o))*/)
{
    for (var i =0; i<o.length; i++)
       {
           var subo=o[i];
          perSubO(o, subo, ref2o, i);
       }       
}
else
{
    for (var p in o)
    {
       var subo = o[p];
       perSubO(o, subo, ref2o, p);
     }
}
return o;
}
";
            js = js.Replace("_id_", IdTag);
            js = js.Replace("_ref_", ReferenceTag);
            return js;
        }

public static string getMarkAsArrayJSFunction(string MarkAsArrayFunctionName)
{
    return
@"//mark array so browser specific testing is not required 
function " + MarkAsArrayFunctionName + @"(arr) {arr._a_='y'; return arr;}";
}   


    }
}
