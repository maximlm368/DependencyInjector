using System;
using System . Collections . Generic;
using System . Text;
using Microsoft . Extensions . Configuration . Json;
using Microsoft . Extensions . Configuration;

namespace DependencyInjector
{
    class ResolverByConfigFile
    {
        private IConfigurationRoot _configRoot;

        private string _configFilePath;


        public ResolverByConfigFile ( )
        {
            _configFilePath = "jsconfig1.json";
            var builder = new ConfigurationBuilder ( );
            builder . AddJsonFile ( _configFilePath );
            _configRoot = builder . Build ( );
        }


        public string GetImplimentationNameByAbstraction ( string abstractionFullName )
        {
            var implimentationName = "";
            var implimentations = _configRoot . GetSection ( "Implimentations:Items" );

            foreach ( var implimentationItem   in   implimentations . GetChildren ( ) )
            {
                var fromConfigInterfaceName = implimentationItem . GetSection ( "abstractionFullName" ) . Value;

                if ( NamesAreEqual ( fromConfigInterfaceName, abstractionFullName ) )
                {
                    implimentationName = implimentationItem . GetSection ( "implimentationFullName" ) . Value;
                    break;
                }
            }

            if ( implimentationName . Length == 0 )
            {
                throw new ImplimentationNotFoundExeption ( abstractionFullName + " implimentation not found" );
            }

            return implimentationName;
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


        public Dictionary<string , Type> GetValuesOfGenericParams ( Type genericType )
        {
            var typeNames = new Dictionary <string ,  Type> ( );
            var valueSections = _configRoot . GetSection ( "ValuesForGenerics:Items:" + genericType . FullName );

            if ( ! valueSections . Exists ( ) )
            {
                throw new ImplimentationNotFoundExeption ( _configFilePath + " does not include type params for " + genericType . FullName );
            }

            foreach ( var typeParam    in   valueSections . GetChildren ( ) )
            {
                var varName = typeParam . GetSection ( "var" ) . Value;
                var varValue = Type . GetType ( typeParam . GetSection ( "valueFullName" ) . Value )
                typeNames . Add ( varName ,  varValue );
            }

            return typeNames;
        }


        public object GetValueOfSimpleParam ( Type parentType , int ordinalNumberAmongCtorParams , Type targetType)
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
                throw new ImplimentationNotFoundExeption ( 
                                                           _configFilePath + " does not include type params for " + parentType . FullName + " with " +
                                                           ordinalNumberAmongCtorParams + " number of param and type of param " + targetType . FullName  
                                                         );
            }

            return simpleValue;
        }


        public int GetCtorOrdinalNumber ( Type targetType, int nodeOrdinalNumber )
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


    }



    class ImplimentationNotFoundExeption : Exception 
    {
         public override string Message { get; }

         public ImplimentationNotFoundExeption ( string message )
         {
            Message = message;
         }
    }
}
