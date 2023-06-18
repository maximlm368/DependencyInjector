using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace DependencyResolver
{
   public static class AbstractionResolver
   {
       private static ResolverByConfigFile _resolverByConfigFile = null;

       /// <summary>
       /// custom attribute marks method allows us to pass on
       /// temporarily absent parameters ( objects of that types )
       /// </summary>
       private static string _attributeNameOfResolvingCiruitMethod = null;


       public static object GetObjectOf( Type aimType, string confFilePath, string attributeNameOfResolvingCiruitMethod )
       {
            if ( _resolverByConfigFile == null )
            {
                _resolverByConfigFile = ResolverByConfigFile.GetResolverByFile ( confFilePath );
            }

            if( _attributeNameOfResolvingCiruitMethod == null )
            {
                _attributeNameOfResolvingCiruitMethod = attributeNameOfResolvingCiruitMethod;
            }

            var depTree = new DependencyTree ( aimType );
            var aimObject = depTree.BuildAimObject ( );

            return aimObject;
       }


       internal static ResolverByConfigFile GetConfigResolver ()
       {
            if ( _resolverByConfigFile == null )
            {
                throw new Exception ( "Resolver.GetConfigResolver can't be invoked before Resolver.GetObjectOf" );
            }

            return _resolverByConfigFile;
       }


       internal static string GetAttributeName ()
       {
            if( _attributeNameOfResolvingCiruitMethod == null )
            {
                throw new Exception ( "Resolver.GetConfigResolver can't be invoked before Resolver.GetObjectOf" );
            }

            return _attributeNameOfResolvingCiruitMethod;
       }

   }


}