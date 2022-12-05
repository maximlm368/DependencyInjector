using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;


namespace DependencyInjector
{
    public class NestedObject
    {
        public bool _IsSimple { get; private set; }

        public Type _typeOfObject { get; private set; }

        public object _objectItself
        {
            get
            {
                if ( _objectItself != null )
                {
                    return _objectItself;
                }

                throw new Exception ( ");
            }

            private set { }
        }

        public bool _isInitiolized { get; private set; }




        public NestedObject ( Type nestedType )
        {
            _typeOfObject = nestedType;
            SetSimplenessIfItIs ( );
            _isInitiolized = false;
            SetInterfaceImplimentation ( );
        }




        //public NestedParamObject ( Type nestedType , object nestedObject )
        //{
        //    this . paramObjectType = nestedType;

        //    if ( nestedObject != null )
        //    {
        //        this . paramObject = nestedObject;
        //    }

        //    SetSimplenessIfItIs ( );
        //}



        private void SetSimplenessIfItIs ( )
        {
            if ( _typeOfObject . IsPrimitive || ( _typeOfObject . FullName == "System.String" ) )
            {
                _IsSimple = true;
            }
        }



        private void SetInterfaceImplimentation ( DependencyInjection config )
        {
            if ( _typeOfObject . IsInterface )
            {
                _typeOfObject = Type . GetType ( config . GetCtorNameByInterface ( _typeOfObject . FullName ) );
            }
        }



        public void InitializeYourSelf ( object ctorParam )
        {
            _objectItself = ctorParam;
            _isInitiolized = true;
        }



        public void InitializeYourSelf ( object [ ] ctorParams )
        {
            _objectItself = Activator . CreateInstance ( _typeOfObject , ctorParams );
            _isInitiolized = true;
        }


        private List<ParameterInfo> GetCtorParamInfos ( )
        {
            ParameterInfo [ ] paramInfos;

            var ctors = _typeOfObject . GetConstructors ( );

            if ( ctors . Length > 0 )
            {
                paramInfos = ctors [ 0 ] . GetParameters ( );
            }



            return paramInfos . ToList ( );
        }



        public List<Type> GetCtorParamTypes ( )
        {
            var result = new List<Type> ( );

            var paramInfos = GetCtorParamInfos ( );

            object [ ] paramObjects = new object [ paramInfos . Count ];

            for ( var paramCounter = 0; paramCounter < paramInfos . Count; paramCounter++ )
            {
                Type paramType = paramInfos [ paramCounter ] . ParameterType;

                result . Add ( paramType );
            }

            return result;
        }



        public string GetObjectTypeName ( )
        {
            return _typeOfObject . FullName;
        }

    }
}