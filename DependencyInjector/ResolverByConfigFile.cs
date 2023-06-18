using System;
using System . Collections . Generic;
using System . Text;
using Microsoft . Extensions . Configuration . Json;
using Microsoft . Extensions . Configuration;

namespace DependencyResolver
{
    class ResolverByConfigFile
    {
        private static ResolverByConfigFile _resolverByFile = null;

        private IConfigurationRoot _configRoot;

        private string _configFilePath;

        private string _attributeSection;


        private ResolverByConfigFile ( string confFilePath )
        {
            _configFilePath = confFilePath;
            _attributeSection = "AttributeName";
            var builder = new ConfigurationBuilder ( );
            builder . AddJsonFile ( _configFilePath );
            _configRoot = builder . Build ( );
        }


        internal static ResolverByConfigFile GetResolverByFile ( string confFilePath )
        {
           if( _resolverByFile == null )
           {
                _resolverByFile = new ResolverByConfigFile ( confFilePath );
           }

            return _resolverByFile;
        }


        internal string GetImplimentationNameByAbstraction ( string abstractionFullName , int nodeOrdinalNumber )
        {
            var implimentationName = GetImplimentationName ( abstractionFullName , nodeOrdinalNumber );

            if ( implimentationName . Length == 0 )
            {
                implimentationName = GetCommonImplimentationName ( abstractionFullName );
            }

            if ( implimentationName . Length == 0 )
            {
                throw new ConfigResolutionExeption ( abstractionFullName + " implimentation not found" );
            }

            return implimentationName;
        }


        private string GetImplimentationName ( string abstractionFullName , int nodeOrdinalNumber )
        {
            var implimentationName = "";
            var implimentations = _configRoot . GetSection ( "Implimentations:Items" );

            foreach ( var implimentationItem in implimentations . GetChildren ( ) )
            {
                var fromConfigInterfaceName = implimentationItem . GetSection ( "abstractionFullName" ) . Value;
                var abstractionNameCoincides = NamesAreEqual ( fromConfigInterfaceName , abstractionFullName );
                var ordinalNumberAsString = implimentationItem 
                                          . GetSection ( "ordinalNumberCountingFromLeftToRightFromRootAsZeroInDependencyTree" ) . Value;
                var numberCoincides = OrdinalNumbersAreEqual ( ordinalNumberAsString , nodeOrdinalNumber , abstractionFullName );

                if ( abstractionNameCoincides    &&    numberCoincides )
                {
                    implimentationName = implimentationItem . GetSection ( "implimentationFullName" ) . Value;
                    break;
                }
            }

            return implimentationName;
        }


        private string GetCommonImplimentationName ( string abstractionFullName )
        {
            var implimentationName = "";
            var implimentations = _configRoot . GetSection ( "CommonImplimentations:Items" );

            foreach ( var implimentationItem in implimentations . GetChildren ( ) )
            {
                var fromConfigInterfaceName = implimentationItem . GetSection ( "abstractionFullName" ) . Value;
                var abstractionNameCoincides = NamesAreEqual ( fromConfigInterfaceName , abstractionFullName );

                if ( abstractionNameCoincides )
                {
                    implimentationName = implimentationItem . GetSection ( "implimentationFullName" ) . Value;
                    break;
                }
            }

            return implimentationName;
        }


        private bool OrdinalNumbersAreEqual ( string ordinalNumberAsString , int ordinalNumber , string abstractionFullName )
        {
            var equal = false;
            var expMessage = "section with " + abstractionFullName + " in config file has incorrect value of "
                                                    + " 'ordinalNumberCountingFromLeftToRightFromRootAsZeroInDependencyTree' ";

            try
            {
                var configOrdinalNumber = Int32 . Parse ( ordinalNumberAsString );

                if( configOrdinalNumber == ordinalNumber )
                {
                    equal = true;
                }
            }
            catch ( FormatException )
            {
                throw new ConfigResolutionExeption ( expMessage );
            }

            return equal;
        }


        private bool NamesAreEqual ( string firstAbstractionFullName,  string secondAbstractionFullName )
        {
            firstAbstractionFullName = firstAbstractionFullName . RidOfBlanks ( );
            secondAbstractionFullName = secondAbstractionFullName . RidOfBlanks ( );

            if ( firstAbstractionFullName == secondAbstractionFullName )
            {
                return true;
            }

            return false;
        }


        internal Dictionary<string , Type> GetValuesOfGenericParams ( Type genericType )
        {
            var typeNames = new Dictionary <string ,  Type> ( );
            var valueSections = _configRoot . GetSection ( "ValuesForGenerics:Items:" + genericType . FullName );

            if ( ! valueSections . Exists ( ) )
            {
                throw new ConfigResolutionExeption ( _configFilePath + " does not include type params for " + genericType . FullName );
            }

            foreach ( var typeParam    in   valueSections . GetChildren ( ) )
            {
                var varName = typeParam . GetSection ( "var" ) . Value;
                var varValue = Type . GetType ( typeParam . GetSection ( "valueFullName" ) . Value )
                typeNames . Add ( varName ,  varValue );
            }

            return typeNames;
        }


        internal object GetValueOfSimpleParam ( Type parentType , int ordinalNumberAmongCtorParams , Type targetType)
        {
            object simpleValue = null;
            var simpleParamSections = _configRoot . GetSection ( "SimpleParameters:Items" );

            foreach ( var simpleParam   in    simpleParamSections . GetChildren ( ) )
            {
                var parentCtorName = simpleParam . GetSection ( "parentCtorFullName" ) . Value;
                var numberAmongCtorParams = simpleParam . GetSection ( "ordinalNumberInCtorParamsArray" ) . Value;
                var parameterTypeName = simpleParam . GetSection ( "parameterTypeFullName" ) . Value;
                
                var parentCoincides = parentType . FullName == parentCtorName;
                var numberCoincides = ordinalNumberAmongCtorParams == Int32 . Parse ( numberAmongCtorParams );
                var targetTypeCoincedes = targetType . FullName == parameterTypeName;

                if ( parentCoincides   &&   numberCoincides   &&   targetTypeCoincedes )
                {
                    simpleValue = (object) simpleParam . GetSection ( "value" ) . Value;
                    break;
                }
            }

            if ( simpleValue == null )
            {
                throw new ConfigResolutionExeption ( 
                                                     _configFilePath + " does not include type params for " + parentType . FullName + " with " +
                                                     ordinalNumberAmongCtorParams + " number of param and type of param " + targetType . FullName  
                                                    );
            }

            return simpleValue;
        }


        internal int GetCtorOrdinalNumber ( Type targetType, int nodeOrdinalNumber )
        {
            var targetCtorNumber = 0;
            var ctorNumbers = _configRoot . GetSection ( "CtorNumbers:Items" );

            foreach ( var number    in    ctorNumbers . GetChildren ( ) )
            {
                var ctorName = number . GetSection ( "ctorFullName" ) . Value;
                var nodeNumber = number . GetSection ( "ordinalNumberCountingFromLeftToRightFromRootAsZeroInDependencyTree" ) . Value;
                var targetTypeCoincedes = targetType . FullName == ctorName;
                var numberCoincides = nodeOrdinalNumber == Int32 . Parse ( nodeNumber );

                if ( targetTypeCoincedes    &&    numberCoincides )
                {
                    try
                    {
                        targetCtorNumber = Int32 . Parse ( number . GetSection ( "ordinalNumberInCtorsArray" ) . Value );
                    }
                    catch( Exception ){ }

                    break;
                }
            }

            return targetCtorNumber;
        }



        //internal string GetAttributeName ( )
        //{
        //    var attributeName = "";
        //    var attributeNameSection = _configRoot.GetSection ( _attributeSection );

        //    ThrowExeptionIfSectionDoesNotExist ( attributeNameSection );
        //    attributeName = attributeNameSection.Value;
        //    var exeptionMessage = _attributeSection + " must have value that presents full name of class of attribute";

        //    var attributeNameIsEmpty = attributeName.Length == 0;

        //    if ( attributeNameIsEmpty )
        //    {
        //        throw new ConfigResolutionExeption ( exeptionMessage );
        //    }

        //    var isNotFullName = !attributeName.Contains ( "." );

        //    if ( isNotFullName )
        //    {
        //        throw new ConfigResolutionExeption ( exeptionMessage );
        //    }

        //    return attributeName;
        //}


        //private void ThrowExeptionIfSectionDoesNotExist ( IConfigurationSection section )
        //{
        //    if ( !section.Exists ( ) )
        //    {
        //        var exeptionMessage = _attributeSection + " section absent in configfile";
        //        throw new ConfigResolutionExeption ( exeptionMessage );
        //    }
        //}
    }



    class ConfigResolutionExeption : Exception 
    {
         public override string Message { get; }

         internal ConfigResolutionExeption ( string message )
         {
            Message = message;
         }
    }
}
