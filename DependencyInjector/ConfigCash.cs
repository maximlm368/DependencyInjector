using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration;

namespace DependencyResolver
{ 
   class Implementations
   {
       public List<ImplementationItem> Items;
   }

   class ImplementationItem
   {
        public string abstractionFullName;

        public string implementationFullName;

        public string ordinalNumberCountingFromLeftToRightFromRootAsZeroInDependencyTree;
   }



    class CommonImplementations
    {
        public List<CommonImplementationItem> Items;
    }

    class CommonImplementationItem
    {
        public string abstractionFullName;

        public string implementationFullName;
    }



    class ValuesForGenerics
    {
        public Dictionary <string , List<GenericValue>> Items;
    }

    class GenericValue
    {
        public string var;

        public string valueFullName;
    }



    class SimpleParameters
    {
        public List<SimpleParameterItem> items;
    }

    class SimpleParameterItem
    {
        public string parentCtorFullName;

        public string ordinalNumberInCtorParamsArray;

        public string parameterTypeFullName;

        public string value;
    }



    class CtorNumbers
    {
        public List<CtorNumberItem> items;
    }

    class CtorNumberItem
    {
        public string ctorFullName;

        public string ordinalNumberCountingFromLeftToRightFromRootAsZeroInDependencyTree;

        public string ordinalNumberInCtorsArray;
    }



    class AttributeName
    {
    
    }

}