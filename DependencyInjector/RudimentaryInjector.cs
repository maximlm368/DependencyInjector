using System;
using System . Collections . Generic;
using System . Linq;
using System . Reflection;
using Microsoft . Extensions . Configuration . Json;
using Microsoft . Extensions . Configuration;

namespace DependencyInjector
{
    public class DependencyInjection
    {
        /// <summary>
        /// throw ConfigurationErrorException, UnSuccessful_Getting_Exception
        /// </summary>
        /// <param name="interfaceName"></param>
        /// <returns></returns>

        public string GetCtorNameByInterface ( string interfaceName )
        {
            Configuration config = ConfigurationManager . OpenExeConfiguration ( @"C:\Users\RBT\source\repos\WinFormTraining\WinFormTraining\bin\Debug\WinFormTraining.exe" );

            var jsonConfigSource1 = new JsonConfigurationProvider ( TestStreamHelpers . ArbitraryFilePath );
            jsonConfigSource1 . Load ( TestStreamHelpers . StringToStream ( json1 ) );
            

            var builder = new ConfigurationBuilder ( );
            builder . AddJsonFile ( "" );
            var appConfig = builder . Build ( );
            appConfig.





            var dependencies = ( InterfaceImplimentations ) config . GetSection ( "Interface_Implimentations" );

            //dependencies = ( Interface_Implimentations ) ConfigurationManager . GetSection ( "Interface_Implimentations" );

            if ( dependencies == null )
            { throw new Exception ( "qwerty" ); }

            var interfaces = dependencies . InterfaceItems;

            string implimentationName = null;

            for ( var i = 0; i < interfaces . Count; i++ )
            {
                if ( interfaces [ i ] . Interface_Name == interfaceName )
                {
                    implimentationName = interfaces [ i ] . Implimentation;

                    break;
                }
            }
            if ( implimentationName == null )
            { throw new UnSuccessfulGettingException ( interfaceName + " interface is not implimented in 'Config' file " ); }

            return implimentationName;
        }




        public object CreateInstanceByTypeName ( string typeName )
        {
            var ctorType = Type . GetType ( typeName );

            if ( ctorType . IsInterface )
            {
                ctorType = Type . GetType ( GetCtorNameByInterface ( typeName ) );
            }

            var parameterInfoItems = ctorType . GetConstructors ( ) [ 0 ] . GetParameters ( );

            var paramObjects = new object [ parameterInfoItems . Length ];

            // 'primitiveParams' may be 'System.String'

            bool metSimpleParam = false;

            for ( var i = 0; i < parameterInfoItems . Length; i++ )
            {
                var paramType = parameterInfoItems [ i ] . ParameterType;

                if ( metSimpleParam && ( paramType . FullName == "System.String" || paramType . IsPrimitive ) )
                    continue;

                if ( !metSimpleParam && ( paramType . FullName == "System.String" || paramType . IsPrimitive ) )
                {
                    InitializeSimpleParamsInWholeList ( ctorType , parameterInfoItems , paramObjects );

                    metSimpleParam = true;
                }

                if ( paramType . FullName != "System.String" && !paramType . IsPrimitive )
                {
                    paramObjects [ i ] = CreateInstanceByTypeName ( paramType . FullName );
                }
            }

            return Activator . CreateInstance ( ctorType , paramObjects );
        }




        public void InitializeSimpleParamsInWholeList ( Type purposeType , ParameterInfo [ ] allCtorParamInforms , object [ ] allCtorParamObjects )
        {
            var simpleParams = GatherCtorPrimitiveParams ( purposeType . FullName );

            OrderParamsByNumber ( simpleParams , purposeType . FullName );

            CheckParamsMatching ( allCtorParamInforms , simpleParams , purposeType . FullName );

            var wholeParamsList = RenderSimpleParamsOnWholeList ( allCtorParamInforms , simpleParams );

            for ( var j = 0; j < simpleParams . Count; j++ )
            {
                object paramObj = null;

                var numberInCtor = Int32 . Parse ( simpleParams [ j ] . numberAmongCtorParams );

                if ( wholeParamsList [ numberInCtor ] as SimpleParameter != null )
                {
                    var item = ( SimpleParameter ) wholeParamsList [ numberInCtor ];

                    if ( item . parameterTypeName == "System.String" )
                    {
                        paramObj = ( object ) item . value;
                    }
                    else
                    {
                        paramObj = GetParamValue ( ( SimpleParameter ) wholeParamsList [ j ] );
                    }

                    allCtorParamObjects [ Int32 . Parse ( simpleParams [ j ] . numberAmongCtorParams ) ] = paramObj;
                }
            }
        }




        /// <summary>
        ///   throw
        ///   ConfigurationErrorException,
        ///   
        /// </summary>
        /// <param name="ctorFullName"></param>
        /// <returns></returns>

        public List<SimpleParameter> GatherCtorPrimitiveParams ( string ctorFullName )
        {
            Configuration config = ConfigurationManager . OpenExeConfiguration ( @"C:\Users\RBT\source\repos\WinFormTraining\WinFormTraining\bin\Debug\WinFormTraining.exe" );

            var paramConfig = ( PrimitiveParametersFromConfiguration ) config . GetSection ( "Primitive_Parameters" );

            var parametrs = paramConfig . parametersSection;

            var simpleCtorParams = new List<SimpleParameter> ( );

            var str = "";

            for ( var i = 0; i < parametrs . Count; i++ )
            {
                var ctorName = RidSeparator ( '-' , parametrs [ i ] . Implimentation );

                ctorName .

                if ( ctorName == ctorFullName )
                {
                    var item = new SimpleParameter ( );

                    str += ctorName;

                    item . ownerCtorFullName = ctorName;

                    item . numberAmongCtorParams = parametrs [ i ] . Number;

                    item . parameterTypeName = parametrs [ i ] . Parameter_type_name;

                    item . value = parametrs [ i ] . Value;

                    simpleCtorParams . Add ( item );
                }
            }
            return simpleCtorParams;
        }




        private string RidSeparator ( char separator , string handledStr )
        {
            string result = "";

            for ( var j = 0; j < handledStr . Length; j++ )
            {
                if ( handledStr [ j ] . Equals ( separator ) )
                { break; }

                else
                { result += handledStr [ j ]; }
            }
            return result;
        }




        /// <summary>
        /// throws FormatException,   Params_Matching_Exception
        /// </summary>
        /// <param name="primitiveParams"></param>
        /// <param name="ctorName"></param>

        private void OrderParamsByNumber ( List<SimpleParameter> primitiveParams , string ctorName )        //  throws formatException
        {
            var counter = primitiveParams . Count - 1;

            for ( var i = 0; i < primitiveParams . Count; i++ )
            {
                for ( var j = 0; j < counter; j++ )
                {
                    if ( Int32 . Parse ( primitiveParams [ j ] . numberAmongCtorParams ) > Int32 . Parse ( primitiveParams [ j + 1 ] . numberAmongCtorParams ) )

                    { var p = primitiveParams [ j ]; primitiveParams [ j ] = primitiveParams [ j + 1 ]; primitiveParams [ j + 1 ] = p; }
                }

                counter -= i;
            }

            var repeated_numbers = "";

            for ( var i = 0; i < primitiveParams . Count - 1; i++ )
            {
                if ( Int32 . Parse ( primitiveParams [ i ] . numberAmongCtorParams ) == Int32 . Parse ( primitiveParams [ i + 1 ] . numberAmongCtorParams ) )
                {
                    repeated_numbers += " , " + primitiveParams [ i ] . numberAmongCtorParams;
                }
            }

            if ( repeated_numbers . Length != 0 )
            { throw new ParamsMatchingException ( ctorName + " implimentation repeats numbers " + repeated_numbers ); }
        }




        /// <summary>
        /// throws  OverflowException, ArgumentNullException, FormatException
        /// </summary>

        private void CheckParamsMatching ( ParameterInfo [ ] paramsFromReflection , List<SimpleParameter> paramsFromConfig , string ctorName )
        {
            var difference = paramsFromConfig . ToList ( );

            var primitiveParamInfoItems = SelectPrimitivParams ( paramsFromReflection );

            for ( var i = 0; i < primitiveParamInfoItems . Count; i++ )
            {
                var number = primitiveParamInfoItems [ i ] . Position;

                var foundInReflection = false;

                for ( var j = 0; j < paramsFromConfig . Count; j++ )
                {
                    if ( Int32 . Parse ( paramsFromConfig [ j ] . numberAmongCtorParams ) == number )
                    {
                        if ( paramsFromConfig [ j ] . parameterTypeName != primitiveParamInfoItems [ i ] . ParameterType . FullName )
                        {
                            throw new ParamsMatchingException ( paramsFromConfig [ j ] . parameterTypeName + "  'parameter_type_name'  is uncorrect;" );
                        }
                        foundInReflection = true;

                        if ( !CheckValueType ( paramsFromConfig [ j ] ) )
                        {
                            throw new ParamsMatchingException ( paramsFromConfig [ j ] . parameterTypeName + "  value  is  not  matching  'parameter_type_name';" );
                        }
                        difference . Remove ( paramsFromConfig [ j ] );
                    }
                }
                if ( !foundInReflection )
                { throw new ParamsMatchingException ( primitiveParamInfoItems [ i ] . Name + "  not found in config.file;" ); }
            }

            if ( difference . Count != 0 )
            {
                var errorDescription = "";

                for ( var i = 0; i < primitiveParamInfoItems . Count; i++ )
                { errorDescription += ctorName + " implimentation " + difference [ i ] . numberAmongCtorParams + "  is uncorrect;"; }

                throw new ParamsMatchingException ( errorDescription );
            }
        }




        /// <summary>
        /// throws  OverflowException, ArgumentNullException
        /// </summary>
        /// <param name="paramsFromConfig"></param>

        private bool CheckValueType ( SimpleParameter paramsFromConfig )
        {
            bool result = true;

            try
            {
                if ( paramsFromConfig . parameterTypeName == "System.Int16" )
                    Int16 . Parse ( paramsFromConfig . value );

                if ( paramsFromConfig . parameterTypeName == "System.Int32" )
                    Int32 . Parse ( paramsFromConfig . value );

                if ( paramsFromConfig . parameterTypeName == "System.Int64" )
                    Int64 . Parse ( paramsFromConfig . value );

                if ( paramsFromConfig . parameterTypeName == "System.Double" )
                    Double . Parse ( paramsFromConfig . value );

                if ( paramsFromConfig . parameterTypeName == "System.Single" )
                    Single . Parse ( paramsFromConfig . value );

                if ( paramsFromConfig . parameterTypeName == "System.Boolean" )
                    Boolean . Parse ( paramsFromConfig . value );
            }
            catch ( FormatException ) { result = false; }

            return result;
        }




        private List<ParameterInfo> SelectPrimitivParams ( ParameterInfo [ ] paramsFromReflection )
        {
            var result = new List<ParameterInfo> ( );

            for ( var i = 0; i < paramsFromReflection . Length; i++ )
            {
                if ( paramsFromReflection [ i ] . ParameterType . IsPrimitive || paramsFromReflection [ i ] . ParameterType . FullName == "System.String" )
                {
                    result . Add ( paramsFromReflection [ i ] );
                }
            }
            return result;
        }




        private List<object> RenderSimpleParamsOnWholeList ( ParameterInfo [ ] paramsFromReflection , List<SimpleParameter> configParamDescriptions )
        {
            if ( paramsFromReflection . Length < configParamDescriptions . Count )
            { throw new ParamsMatchingException ( " App.config has more parameters than metadata " ); }

            var completedParameterList = new List<object> ( );

            for ( var i = 0; i < paramsFromReflection . Length; i++ )
            {
                completedParameterList . Add ( null );
            }

            for ( var i = 0; i < configParamDescriptions . Count; i++ )
            {
                var number = Int32 . Parse ( configParamDescriptions [ i ] . numberAmongCtorParams );

                completedParameterList [ number ] = configParamDescriptions [ i ];
            }
            return completedParameterList;
        }




        public SimpleParameter GetSimpleParameter ( string ctorFullName , int paramSerialNumber )
        {
            Configuration config = ConfigurationManager . OpenExeConfiguration ( @"C:\Users\RBT\source\repos\WinFormTraining\WinFormTraining\bin\Debug\WinFormTraining.exe" );

            var paramConfig = ( PrimitiveParametersFromConfiguration ) config . GetSection ( "Primitive_Parameters" );

            var parametrs = paramConfig . parametersSection;

            SimpleParameter item = new SimpleParameter ( );

            for ( var i = 0; i < parametrs . Count; i++ )
            {
                var ctorName = parametrs [ i ] . Implimentation . RidOfSeparator ( '-' );

                var ctorsCoincide = ( ctorName == ctorFullName );

                var serialNumbersCoincide = ( paramSerialNumber . ToString ( ) == parametrs [ i ] . Number );

                if ( ctorsCoincide && serialNumbersCoincide )
                {
                    item = new SimpleParameter ( );

                    item . ownerCtorFullName = ctorName;

                    item . numberAmongCtorParams = parametrs [ i ] . Number;

                    item . parameterTypeName = parametrs [ i ] . Parameter_type_name;

                    item . value = parametrs [ i ] . Value;

                    break;
                }
            }
            return item;
        }




        /// <summary>
        /// throws  OverflowException, ArgumentNullException, FormatException
        /// </summary>
        /// <param name="config_parameters"></param>

        public object GetParamValue ( SimpleParameter configParamDescription )
        {
            object result = new object ( );

            if ( configParamDescription . parameterTypeName == "System.Int16" )
                result = ( object ) Int16 . Parse ( configParamDescription . value );

            if ( configParamDescription . parameterTypeName == "System.Int32" )
                result = ( object ) Int32 . Parse ( configParamDescription . value );

            if ( configParamDescription . parameterTypeName == "System.Int64" )
                result = ( object ) Int64 . Parse ( configParamDescription . value );

            if ( configParamDescription . parameterTypeName == "System.Double" )
                result = ( object ) Double . Parse ( configParamDescription . value );

            if ( configParamDescription . parameterTypeName == "System.Single" )
                result = ( object ) Single . Parse ( configParamDescription . value );

            if ( configParamDescription . parameterTypeName == "System.Boolean" )
                result = ( object ) Boolean . Parse ( configParamDescription . value );

            return result;
        }




        public object [ ] FindAllPrimitiveParams ( ParameterInfo [ ] allParams )
        {
            var param_values = new object [ allParams . Length ];

            List<SimpleParameter> primitive_params = null;

            for ( var i = 0; i < allParams . Length; i++ )
            {
                string param_implimentation_name = allParams [ i ] . ParameterType . FullName;

                if ( primitive_params != null && ( param_implimentation_name == "System.String" || allParams [ i ] . ParameterType . IsPrimitive ) )
                    continue;

                if ( primitive_params == null && ( param_implimentation_name == "System.String" || allParams [ i ] . ParameterType . IsPrimitive ) )
                {
                    //  Assign_ctor_primitive_params_once_new ( primitive_params , type , parameters_info , param_values );
                }
            }

            return param_values;
        }




        private object [ ] Assign_ctor_primitive_params_once_new ( List<SimpleParameter> primitive_params , Type type , ParameterInfo [ ] parameters_info )
        {
            object [ ] param_values = new object [ parameters_info . Length ];

            primitive_params = GatherCtorPrimitiveParams ( type . FullName );

            OrderParamsByNumber ( primitive_params , type . FullName );

            CheckParamsMatching ( parameters_info , primitive_params , type . FullName );

            var all_ctor_params = RenderSimpleParamsOnWholeList ( parameters_info , primitive_params );

            for ( var j = 0; j < primitive_params . Count; j++ )
            {
                object param_obj = null;

                var counter = Int32 . Parse ( primitive_params [ j ] . numberAmongCtorParams );

                if ( all_ctor_params [ counter ] . parameterTypeName == "System.String" )

                { param_obj = ( object ) all_ctor_params [ counter ] . value; }

                else
                {
                    param_obj = GetParamValue ( all_ctor_params [ j ] );
                }
                param_values [ Int32 . Parse ( primitive_params [ j ] . numberAmongCtorParams ) ] = param_obj;
            }

            return param_values;
        }
    }



    public class UnSuccessfulGettingException : Exception
    {
        public UnSuccessfulGettingException ( string message ) : base ( message ) { }
    }

    public class ParamsMatchingException : Exception
    {
        public ParamsMatchingException ( string message ) : base ( message ) { }
    }


    public class InterfaceImplimentationNames
    {
        public string intrfaceName;

        public string implimentationName;
    }


    public class SimpleParameter
    {
        public string ownerCtorFullName;

        public string numberAmongCtorParams;

        public string parameterTypeName;

        public string value;
    }






    public class PrimitiveParametersFromConfiguration : ConfigurationSection
    {
        [ConfigurationProperty ( "Parameters" )]
        public Parameters parametersSection
        {
            get
            {
                var inter = ( ( Parameters ) ( base [ "Parameters" ] ) );

                return inter;
            }
        }
    }


    [ConfigurationCollection ( typeof ( ParameterElement ) , AddItemName = "Parameter" )]

    public class Parameters : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement ( )
        {
            return new ParameterElement ( );
        }

        protected override object GetElementKey ( ConfigurationElement element )
        {
            return ( ( ParameterElement ) ( element ) ) . Implimentation;
        }

        public ParameterElement this [ int index ]
        {
            get { return ( ParameterElement ) BaseGet ( index ); }
        }
    }


    public class ParameterElement : ConfigurationElement
    {

        [ConfigurationProperty ( "implimentation" , DefaultValue = "" , IsKey = false , IsRequired = false )]

        public string Implimentation
        {
            get { return ( ( string ) ( base [ "implimentation" ] ) ); }
            set { base [ "implimentation" ] = value; }
        }


        [ConfigurationProperty ( "number" , DefaultValue = "" , IsKey = false , IsRequired = false )]

        public string Number
        {
            get { return ( ( string ) ( base [ "number" ] ) ); }
            set { base [ "number" ] = value; }
        }


        [ConfigurationProperty ( "parameter_type_name" , DefaultValue = "" , IsKey = false , IsRequired = false )]

        public string Parameter_type_name
        {
            get { return ( ( string ) ( base [ "parameter_type_name" ] ) ); }
            set { base [ "parameter_type_name" ] = value; }
        }


        [ConfigurationProperty ( "value" , DefaultValue = "" , IsKey = false , IsRequired = false )]

        public string Value
        {
            get { return ( ( string ) ( base [ "value" ] ) ); }
            set { base [ "value" ] = value; }
        }

    }




    public class InterfaceImplimentations : ConfigurationSection
    {
        [ConfigurationProperty ( "Interfaces" )]
        public Interfaces InterfaceItems
        {
            get
            {
                var inter = ( ( Interfaces ) ( base [ "Interfaces" ] ) );

                return inter;
            }
        }
    }


    [ConfigurationCollection ( typeof ( InterfaceElement ) , AddItemName = "Interface" )]
    public class Interfaces : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement ( )
        {
            return new InterfaceElement ( );
        }

        protected override object GetElementKey ( ConfigurationElement element )
        {
            return ( ( InterfaceElement ) ( element ) ) . Interface_Name;
        }

        public InterfaceElement this [ int index ]
        {
            get { return ( InterfaceElement ) BaseGet ( index ); }
        }
    }


    public class InterfaceElement : ConfigurationElement
    {

        [ConfigurationProperty ( "interface" , DefaultValue = "" , IsKey = true , IsRequired = true )]
        public string Interface_Name
        {
            get { return ( ( string ) ( base [ "interface" ] ) ); }
            set { base [ "interface" ] = value; }
        }

        [ConfigurationProperty ( "implimentation" , DefaultValue = "" , IsKey = false , IsRequired = false )]
        public string Implimentation
        {
            get { return ( ( string ) ( base [ "implimentation" ] ) ); }
            set { base [ "implimentation" ] = value; }
        }
    }






    //---------------------------------------


    public interface ISomeInterface { double Method_1 ( int a , double b ); string Method_2 ( int a , int b ); }




    public interface IAnotherSomeInterface { int Another_Method_1 ( int a , int b ); string Another_Method_2 ( int a , string b , string c ); }




    public class SomeClass : ISomeInterface
    {
        private string someProperty;

        private IAnotherSomeInterface anotherSomeClass;



        public SomeClass ( IAnotherSomeInterface anotherClass , string prop ) { this . anotherSomeClass = anotherClass; this . someProperty = prop; }



        public double Method_1 ( int a , double b )
        {
            return a + b;
        }



        public string Method_2 ( int a , int b )
        {
            return anotherSomeClass . Another_Method_1 ( a , b ) . ToString ( ) + "  /  " + someProperty;
        }

    }



    public class AnotherSomeClass : IAnotherSomeInterface
    {
        private int digital = 0;

        private int digital_2 = 0;


        public AnotherSomeClass ( int a , int b )
        {
            digital = a;
            digital_2 = b;
        }


        public int Another_Method_1 ( int a , int b )
        {
            return a + b + digital + digital_2;
        }

        public string Another_Method_2 ( int a , string b , string c )
        {
            return "It is ok";
        }

        public string Another_Method_3 ( string param ) { /*var a = new DependencyInjection . DependencyInjection ( ); return a . Get_Constructor_name ( param );*/  return ""; }
    }











}