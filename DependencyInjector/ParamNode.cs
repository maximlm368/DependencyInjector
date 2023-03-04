using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;

namespace DependencyInjector
{
    class ParamNode
    {
        private static int _idCounter = 0;

        private int _ordinalNumberInParentCtor;

        private List<ParamNode> _children;

        private List<ParamNode> _temporarilyNotInitializedChildren;

        private NodeType _nodeType;

        private ParamNode _initializingTemplate;

        public int _id { get; private set; }

        public ParamNode _parent { get; private set; }

        public NestedObject _nestedObj { get; private set; }

        public int _myLevelInTree { get; private set; }

        public bool _isInitialized { get; private set; }

        public bool _isFork;


        #region Ctors
        public ParamNode ( Type typeOfNested , NodeType nodeType )
        {
            DryCtorPart ( typeOfNested , nodeType );
            _ordinalNumberInParentCtor = 0;
            _myLevelInTree = 0;
        }


        public ParamNode ( Type typeOfNested , int ordinalNumberInParentCtorParams , ParamNode parent , NodeType nodeType )
        {
            DryCtorPart ( typeOfNested , nodeType );
            _ordinalNumberInParentCtor = ordinalNumberInParentCtorParams;
            _parent = parent;
            _myLevelInTree = parent . _myLevelInTree + 1;
        }


        private void DryCtorPart ( Type typeOfNested , NodeType nodeType )
        {
            _id = _idCounter;
            _idCounter++;
            _nestedObj = new NestedObject ( typeOfNested , _id );
            _temporarilyNotInitializedChildren = new List<ParamNode> ( );
            _nodeType = nodeType;
            _isInitialized = false;
            _children = new List<ParamNode> ( );
        }
        #endregion Ctors

    
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List <ParamNode> GetSequenceForDependencyCircuit ( )
        {
            var possibleCircuit = new List<ParamNode> ( );
            var accomplishedCircuit = new List<ParamNode> ( );
            possibleCircuit . Add ( this );
            ParamNode probableCoincidentAncestor = _parent;

            while ( true )
            {
                if ( probableCoincidentAncestor == null )
                {
                    break;
                }

                possibleCircuit . Add ( probableCoincidentAncestor );

                if ( ! probableCoincidentAncestor . TypeNameEquals ( this ) )
                {
                    probableCoincidentAncestor = probableCoincidentAncestor . _parent;
                    continue;
                }
                else
                {
                    accomplishedCircuit = possibleCircuit;
                    break;
                }
            }
            return accomplishedCircuit;
        }


        public bool TypeNameEquals ( object obj )
        {
            bool equals = false;

            if ( obj is ParamNode )
            {
                var beingCompared = ( ParamNode ) obj;

                if ( beingCompared . _nestedObj . GetObjectTypeName ( )  ==  _nestedObj . GetObjectTypeName ( ) )
                {
                    equals = true;
                }
            }

            return equals;
        }


        #region Initializing

        public void InitializeOrdinary ( )
        {
            var readyParamObjects = new List<object> ( );

            for ( var i = 0;   i < _children . Count;   i++ )
            {
                var childrenNotInitialized = ! _children [ i ] . _nestedObj . _isInitialized;

                if ( childrenNotInitialized )
                {
                    throw new NotInitializedChildException ( );
                }

                object readyParam = _children [ i ] . _nestedObj . _objectItself;
                readyParamObjects . Add ( readyParam );
            }

            _nestedObj . InitializeYourSelf ( readyParamObjects . ToArray ( ) );
        }


        public void InitializePassingOverAbsentParams ( )
        {
            var readyParamObjects = new List<object> ( );
            var typesOfNotInitializedChildren = new List<Type> ( );

            for ( var i = 0;    i < _children . Count;    i++ )
            {
                var childNotInitialized = ! _children [ i ] . _nestedObj . _isInitialized;
                var childIsDependencyCycleParticipant = _children [ i ] . _nodeType . _kind  ==  NodeKind . DependencyCycleParticipant;

                if ( childNotInitialized    &&    childIsDependencyCycleParticipant )
                {
                    var childType = _children [ i ] . _nestedObj . _typeOfObject;
                    _temporarilyNotInitializedChildren . Add ( _children [ i ] );
                    typesOfNotInitializedChildren . Add ( childType );
                    continue;
                }

                if ( childNotInitialized )
                {
                    throw new NotInitializedChildException ( );
                }

                object readyParam = _children [ i ] . _nestedObj . _objectItself;
                readyParamObjects . Add ( readyParam );
            }

            _nestedObj . InitializeYourSelfWithoutSomeParams ( readyParamObjects . ToArray ( ) , typesOfNotInitializedChildren );
        }


        public void InitializeSimple ( )
        {
            var parentType = _parent . _nestedObj . _typeOfObject;
            _nestedObj . InitializeYourSelf ( parentType , _ordinalNumberInParentCtor );
        }


        public void InitializeByTemplate ( )
        {
            _nestedObj . InitializeYourSelf ( _initializingTemplate );
        }


        public void InitializeNestedObject ( )
        {
            if( ! _isInitialized )
            {
                _nodeType . InitializeNested ( this );
                _isInitialized = true;
            }
        }


        public void InitializeNestedObject ( ParamNode template )
        {
            var argNullMessage = "arg 'template' cant be null";

            if ( ! _isInitialized )
            {
                _initializingTemplate = template ?? throw new ArgumentNullException ( argNullMessage );
                InitializeNestedObject ( );
            }
        }

        #endregion Initializing


        public List<ParamNode> DefineChildren ( )
        {
            var childTypes = _nestedObj . GetCtorParamTypes ( );
            var result = _nodeType . DefineChildren ( this , childTypes );
            _children.AddRange ( result );
            return result;
        }


        public void ChangeState ( NodeKind kind )
        {
            switch ( kind ) 
            {
                case NodeKind . TopOfCircuit :
                {
                    _nodeType = new TopOfCircuit ( );
                    break;
                }

                case NodeKind . DependencyCycleParticipant :
                {
                    _nodeType = new DependencyCycleParticipant ( );
                    break;
                }
            }
        }


        public ParamNode GetNearestOrdinaryAncestor ()
        {
            ParamNode ancestor = null;
            var beingProcessed = _parent;

            while ( true )
            {
                if( beingProcessed != null   &&   (beingProcessed . _nodeType . _kind  ==  NodeKind.Ordinary) )
                {
                    ancestor = beingProcessed;
                    break;
                }

                beingProcessed = beingProcessed . _parent;
            }

            return ancestor;
        }


        public NodeKind GetNodeKind ()
        {
            return _nodeType . _kind;
        }




        public void EndUpIinitialization ( )
        {
            var mustBeInitializedAlreadyChildren = _temporarilyNotInitializedChildren;

            for ( int i = 0;    i < mustBeInitializedAlreadyChildren . Count;     i++ )
            {
                if ( ! mustBeInitializedAlreadyChildren [ i ] . _isInitialized )
                {
                    return;
                }
            }

            var arguments = mustBeInitializedAlreadyChildren . GetListOfItemPiece ( ( item ) => { return item . _nestedObj . _objectItself; } );
            _nestedObj . EndUpIinitialization ( arguments );
        }

    }


}