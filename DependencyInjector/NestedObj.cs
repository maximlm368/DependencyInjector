#define DEBUG
using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;
using System . Diagnostics;


namespace DependencyInjector
{
    /// <summary>
    /// dgfgfgddfgd
    /// </summary>
    class NestedObject
    {
        private ResolverByConfigFile _abstractionResolver;

        private int _nestingNodeOrdinalNumberInTree;

        private ConstructorInfo _ctor;

        private List<Type> _temporarilyAbsentParamsOfCtor;

        public Type _typeOfObject { get; private set; }

        public object _objectItself { get; private set; }

        public bool _isInitialized { get; private set; }


        public NestedObject ( Type nestedType,  int nestingNodeOrdinalNumberInTree )
        {
            var method = MethodInfo . GetCurrentMethod ( );
            var currentTypeName = method . ReflectedType . Name;
            var currentMethodName = method . Name;
            var dot = ".";
            var messageSense = " must recieve not null 'nestedType' argument";

            _typeOfObject = nestedType ?? throw new ArgumentNullException ( currentTypeName + dot + currentMethodName + messageSense );
            _nestingNodeOrdinalNumberInTree = nestingNodeOrdinalNumberInTree;
            _isInitialized = false;
            _abstractionResolver = new ResolverByConfigFile ( );
            ReplaceAbstractionByImplimentation ( );
            MakeGenericTypeViaConfig ( );
        }


        private void MakeGenericTypeViaConfig ( )
        {
            if ( _typeOfObject . IsGenericType     &&     _typeOfObject . IsGenericTypeDefinition )
            {
                var genericArgs = _typeOfObject . GetGenericArguments ( ) . ToList ( );
                var genericArgNames = genericArgs . GetListOfItemPiece ( ( arg ) => { return arg . Name; } );
                var valuesForGenericParams = _abstractionResolver . GetValuesOfGenericParams ( _typeOfObject );
                var types = valuesForGenericParams . GetValuesSortedAccordingListOfKeys<string , Type> ( genericArgNames );
                _typeOfObject = _typeOfObject . MakeGenericType ( types . ToArray ( ) );
            }
        }


        private void ReplaceAbstractionByImplimentation ( )
        {
            var typeName = _abstractionResolver . GetImplimentationNameByAbstraction ( _typeOfObject . FullName , _nestingNodeOrdinalNumberInTree );

            if ( _typeOfObject . IsInterface     ||     _typeOfObject . IsAbstract )
            {
                _typeOfObject = Type . GetType ( typeName );
            }

            var canHaveInheritant = ! _typeOfObject . IsSealed;

            if ( canHaveInheritant )
            {
                try
                {
                    typeName = _abstractionResolver . GetImplimentationNameByAbstraction ( _typeOfObject . FullName , _nestingNodeOrdinalNumberInTree );
                    _typeOfObject = Type . GetType ( typeName );
                }
                catch( ConfigResolutionExeption )
                {
                }
            }
        }


        #region Initialize
        
        public void InitializeYourSelf ( object templateForSubstitution )
        {
            var method = MethodInfo . GetCurrentMethod ( );
            var currentTypeName = method . ReflectedType . Name;
            var currentMethodName = method . Name;
            var dot = ".";
            var messageSense = " must recieve not null 'templateForSubstitution' argument";

            _objectItself = templateForSubstitution ?? throw new ArgumentNullException ( currentTypeName + dot + currentMethodName + messageSense );
            _isInitialized = true;
        }


        public void InitializeYourSelf ( Type parentType , int ordinalNumberAmongCtorParams )
        {
            _objectItself = _abstractionResolver . GetValueOfSimpleParam ( parentType , ordinalNumberAmongCtorParams , _typeOfObject );
            _isInitialized = true;
        }


        public void InitializeYourSelf ( object [ ] ctorParams )
        {
            GetCtor ( );
            _objectItself = _ctor . Invoke ( ctorParams );
            _isInitialized = true;
        }


        public void InitializeYourSelfWithoutSomeParams ( object [ ] presentCtorParams , List <Type> without )
        {
            var method = MethodInfo . GetCurrentMethod ( );
            var currentTypeName = method . ReflectedType . Name;
            var currentMethodName = method . Name;
            var dot = ".";
            var messageSense = " can not recieve null argument";
            
            if ( presentCtorParams == null )
            {
                throw new ArgumentNullException ( currentTypeName + dot + currentMethodName + messageSense + " 'ctorParams'" );
            }

            var namesOfTypesOfParams = GetNamesOfTypesOfObjects ( presentCtorParams );
            var messageForUser = _typeOfObject . FullName + " must have constructor with params that have types (considering order) : " + namesOfTypesOfParams;

            _temporarilyAbsentParamsOfCtor = without ?? throw new ArgumentNullException ( currentTypeName + dot + currentMethodName + messageSense + " 'without'" );

            try
            {       
                _objectItself = Activator . CreateInstance ( _typeOfObject , presentCtorParams );    
            }
            catch ( MissingMethodException )
            {
              #if DEBUG
                Debug . WriteLine ( messageForUser );
              #endif

                throw new MissingMethodException ( messageForUser );
            }

            _isInitialized = true;
        }


        private string GetNamesOfTypesOfObjects ( object [ ] objects )
        {
            var str = new StringBuilder ( );

            for ( int i = 0;    i < objects.Length;    i++ )
            {
                var typeName = objects [ i ] . GetType ( ) . FullName;
                str . Append ( typeName );

                if( i < objects.Length - 1 )
                {
                    str . Append ( " , " );
                }
            }

            return str . ToString ( );
        }

        # endregion Initialize


        public List<Type> GetCtorParamTypes ( )
        {
            var result = new List<Type> ( );
            var paramInfos = GetCtorParamInfos ( );
            
            if( paramInfos == null )
            {
                return result;
            }

            for ( var counter = 0;     counter < paramInfos . Length;     counter++ )
            {
                Type paramType = paramInfos [ counter ] . ParameterType;
                result . Add ( paramType );
            }

            return result;
        }


        private ParameterInfo [ ] GetCtorParamInfos ( )
        {
            ParameterInfo [ ] paramInfos = null;
            GetCtor ( );
            paramInfos = _ctor . GetParameters ( );
            return paramInfos;
        }


        private void GetCtor ()
        {
            if ( _ctor == null )
            {
                var ctors = _typeOfObject . GetConstructors ( );

                if ( ctors . Length < 1 )
                {
                    throw new Exception ( _typeOfObject . FullName + " does not have available public ctor" );
                }

                var ctorNumber = _abstractionResolver . GetCtorOrdinalNumber ( _typeOfObject , _nestingNodeOrdinalNumberInTree );
                
                try
                {
                    _ctor = ctors [ ctorNumber ];
                }
                catch( ArgumentOutOfRangeException )
                {
                    throw new Exception ( _typeOfObject . FullName + " does not have ctor with number " + ctorNumber + " that is gotten from config" );
                }
            }
        }


        public string GetObjectTypeName ( )
        {
            return _typeOfObject . FullName;
        }


        public void EndUpIinitialization ( List<object> arguments )
        {
            string attributeName;
            var resolver = new ResolverByConfigFile ( );
            attributeName = resolver . GetAttributeName ( );
            var methods = _typeOfObject . GetMethods ( );

            for( var i = 0;    i < methods.Length;    i++ )
            {
                var targetMeth = methods [ i ];
                var targetAttribute = targetMeth . GetCustomAttribute ( Type . GetType ( attributeName ) );

                if( targetAttribute != null )
                {
                    var paramInfos = targetMeth . GetParameters ( );
                    var methName = _typeOfObject . FullName + "." + targetMeth . Name;
                    CheckAccordanceOfArguments ( paramInfos , methName , arguments );
                    targetMeth . Invoke ( _objectItself ,   arguments . ToArray ( ) );
                }
            }
        }


        private void CheckAccordanceOfArguments ( ParameterInfo[] paramInfos, string methName, List<object> realArguments )
        {
            var wrongMethodIsSuggested = paramInfos . Length != _temporarilyAbsentParamsOfCtor . Count;
            wrongMethodIsSuggested = wrongMethodIsSuggested     ||     (realArguments . Count   !=   _temporarilyAbsentParamsOfCtor . Count);

            if ( wrongMethodIsSuggested )
            {
                var message = "Attempt to pass on a set of args with uncorrect length to " + methName;
                throw new ConfigResolutionExeption ( message );
            }

            for ( var i = 0;    i < paramInfos . Length;     i++ )
            {
                var typesDoesntContainsOneFromInfos = ! _temporarilyAbsentParamsOfCtor . Contains ( paramInfos [ i ] . GetType ( ) );

                if ( typesDoesntContainsOneFromInfos ) 
                {
                    var message = "Attempt to pass on a set of args with uncorrect types to " + methName;
                    throw new ConfigResolutionExeption ( message );
                }
            }

            OrderArguments ( paramInfos , realArguments );
        }


        private void OrderArguments ( ParameterInfo [ ] paramInfos , List<object> realArguments )
        {
            var ordered = new List<object> ( );

            for ( var i = 0;     i < paramInfos . Length;     i++ )
            {
                var orderIsWrong = realArguments [ i ] . GetType ( ) . FullName   !=   paramInfos [ i ] . ParameterType . FullName;

                if ( orderIsWrong )
                {
                    var feature = paramInfos [ i ] . ParameterType . FullName;
                    var scratch = i;
                    Func<object , string , bool> checker = ( realItem , correct ) =>
                    {
                        var beingCompared = realItem . GetType ( ) . FullName;

                        if ( beingCompared == correct )
                        {
                            return true;
                        }

                        return false;
                    };

                    var target = realArguments . FindFromScratchItemAccordsFeature ( feature , scratch , checker );
                    realArguments . Exchange ( target , i );
                }
            }
        }

    }

}