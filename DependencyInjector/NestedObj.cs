#define DEBUG
using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;
using System . Diagnostics;


namespace DependencyInjector
{
    class NestedObject
    {
        private ResolverByConfigFile _abstractionResolver;

        private int _nestingNodeOrdinalNumberInTree;

        private ConstructorInfo _ctor;

        //public bool _IsSimple { get; private set; }

        public Type _typeOfObject { get; private set; }

        public object _objectItself
        {
            get
            {
                if ( _isInitialized )
                {
                    return _objectItself;
                }

                throw new Exception ( ");
            }

            private set
            {
                _objectItself = value;
            }
        }

        public bool _isInitialized { get; private set; }


        public NestedObject ( Type nestedType,  int nestingNodeOrdinalNumberInTree )
        {
            _typeOfObject = nestedType ?? throw new ArgumentNullException ( "NestedObject.ctor must recieve not null 'nestedType' argument" );
            _nestingNodeOrdinalNumberInTree = nestingNodeOrdinalNumberInTree;
            _isInitialized = false;
            _abstractionResolver = new ResolverByConfigFile ( );
            SetAbstractionImplimentation ( );
            //SetSimplenessIfItIs ( );
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


        //private void SetSimplenessIfItIs ( )
        //{
        //    if ( _typeOfObject . IsPrimitive    ||    ( _typeOfObject . FullName == "System.String" ) )
        //    {
        //        _IsSimple = true;
        //    }
        //}


        private void SetAbstractionImplimentation ( )
        {
            if ( _typeOfObject . IsInterface || _typeOfObject . IsAbstract )
            {
                _typeOfObject = Type . GetType ( _abstractionResolver . GetImplimentationNameByAbstraction ( _typeOfObject . FullName ) );
            }
        }


        #region Initialize
        
        public void InitializeYourSelf ( object templateForSubstitution )
        {
            _objectItself = templateForSubstitution ?? throw new Exception
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


        public void InitializeYourSelfWithoutSomeParams ( object [ ] ctorParams , List <Type> without )
        {
            try
            {
                _objectItself = Activator . CreateInstance ( _typeOfObject , ctorParams );
            }
            catch ( MissingMethodException )
            {
              #if DEBUG
                Debug . WriteLine ( "" );
              #endif
               
                throw;
            }

            _isInitialized = true;
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

    }
}