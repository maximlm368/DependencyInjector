using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;

namespace DependencyInjector
{


    class ParamNode
    {
        private int _ordinalNumberInParentCtor;

        private DependencyInjection _config;

        private List<ParamNode> _children;

        private NodeType _nodeType;

        public ParamNode _parent { get; private set; }

        public NestedObject _nestedObj { get; private set; }

        public int _myLevelInTree { get; private set; }

        public bool _isInitiolized { get; private set; }


        public ParamNode ( Type paramType , NodeType nodeType )
        {
            _nestedObj = new NestedObject ( paramType );
            _ordinalNumberInParentCtor = 0;
            _myLevelInTree = 0;
            _nodeType = nodeType;
            _isInitiolized = false;
        }


        public ParamNode ( Type paramType , int ordinalNumberInParentCtorParams , ParamNode parent , NodeType nodeType )
        {
            _nestedObj = new NestedObject ( paramType );
            _ordinalNumberInParentCtor = ordinalNumberInParentCtorParams;
            _parent = parent;
            _myLevelInTree = parent . _myLevelInTree + 1;
            _nodeType = nodeType;
            _isInitiolized = false;
        }


        //private DependencyCircuit CreateChain ( bool cycleFound , List<ParamNode> possibleChain )
        //{
        //    DependencyCircuit result;
        //    possibleChain . Reverse ( );

        //    if ( cycleFound )
        //    {
        //        _nodeType = new DependencyCycleParticipant ( );
        //        possibleChain . AccomplishForEach<ParamNode>
        //        (
        //           ( item ) => { if ( item . _nodeType . _kind  !=  NodeKind . DependencyCycleParticipant ) { item . _nodeType = new DependencyCycleParticipant ( ); } }
        //        );

        //        result = new DependencyCircuit ( possibleChain );
        //    }
        //    else
        //    {
        //        result = DependencyCircuit . CreateEmptyChain ( );
        //    }
        //    return result;
        //}


        //public void FindCyclicDependency ( List<DependencyChain> dependencyChains )
        //{
        //    var cycleFound = false;

        //    var possibleChain = new List<ParamNode> ( );

        //    possibleChain . Add ( this );

        //    ParamNode probableCoincidentAncestor = _parent;

        //    while ( true )
        //    {
        //        if ( probableCoincidentAncestor == null )
        //        {
        //            break;
        //        }

        //        possibleChain . Add ( probableCoincidentAncestor );

        //        if ( ! probableCoincidentAncestor . Equals ( this ) )
        //        {
        //            probableCoincidentAncestor = probableCoincidentAncestor . _parent;

        //            continue;
        //        }
        //        else
        //        {
        //            cycleFound = true;

        //            break;
        //        }
        //    }

        //    if ( cycleFound )
        //    {
        //        _nodeState = new DependencyCycleLeaf ( );

        //        possibleChain . AccomplishForEach <ParamNode> 
        //        ( 
        //           ( item ) => { if ( item . _nodeState._kind   !=   NodeKind . DependencyCycleLeaf ) { item . _nodeState = new DependencyCycleLeaf ( ); } } 
        //        );

        //        var chain = new DependencyChain ( possibleChain );

        //        dependencyChains . Add ( chain );
        //    }
        //}


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


        public void InitializeOrdinary ( )
        {
            var readyParamObjects = new List<object> ( );

            for ( var i = 0;   i < _children . Count;   i++ )
            {
                if ( ! _children [ i ] . _nestedObj . _isInitiolized  )
                {
                    throw new NotInitializedChildException ( );
                }

                object readyParam = _children [ i ] . _nestedObj . _objectItself;
                readyParamObjects . Add ( readyParam );
            }

            _nestedObj . InitializeYourSelf ( readyParamObjects . ToArray ( ) );
        }


        public void InitializeSimple ( )
        {
            var parentType = _parent . _nestedObj . _typeOfObject;
            var simpleParamInform = _config . GetSimpleParameter ( parentType . FullName , _ordinalNumberInParentCtor );
            var obj = _config . GetParamValue ( simpleParamInform );
            _nestedObj . InitializeYourSelf ( obj );
        }


        public void InitializeNestedObject ( )
        {
            if( ! _isInitiolized )
            {
                _nodeType . InitializeNested ( this );
                _isInitiolized = true;
            }
        }


        //public void ResetParentState ( )
        //{
        //    if ( _nodeState . _kind == NodeKind . DependencyCycleParticipant )
        //    {
        //        _parent . _nodeState = new DependencyCycleParticipant ( );
        //    }
        //}


        //public void SetFork ( )
        //{
        //    if ( this . _nodeState . _kind == NodeKind . DependencyCycleParticipant )
        //    {
        //        this . _parent . _nodeState = new DependencyCycleParticipant ( );
        //    }


        //}


        //public List<ParamNode> DefineChildren ( int levelInTree,  out List<DependencyChain> dependencyChains )
        //{
        //    List<DependencyChain> chains = new List<DependencyChain> ( );
        //    var paramTypes = _nestedObj . GetCtorParamTypes ( );
        //    var result = _nodeState . DefineChildren ( this , paramTypes );

        //    dependencyChains = chains;


        //    if()
        //    {
        //        this . _nestedObj . SetInterfaceImplimentation ( this . _config );

        //        if ( !this . _nestedObj . IsSimple )
        //        {
        //            var paramInfos = this . _nestedObj . GetCtorParamInfos ( );
        //            object [ ] paramObjects = new object [ paramInfos . Count ];
        //            var metSimple = false;

        //            for ( var paramCounter = 0; paramCounter < paramInfos . Count; paramCounter++ )
        //            {
        //                Type paramType = paramInfos [ paramCounter ] . ParameterType;
        //                bool isSimple = ( paramType . IsPrimitive ) || ( paramType . FullName == "System.String" );

        //                if ( metSimple && isSimple )
        //                { continue; }

        //                if ( !metSimple && isSimple )
        //                {
        //                    this . _config . InitializePrimitiveParamsInWholeList ( this . _nestedObj . typeOfObject , paramInfos , paramObjects );
        //                    metSimple = true;
        //                }

        //                if ( paramType . IsInterface )
        //                {
        //                    paramType = Type . GetType ( this . _config . GetCtorNameByInterface ( paramType . FullName ) );
        //                }

        //                var node = new ParamNode ( paramType , paramCounter , this , levelInTree );

        //                List<ParamNode> oneChain;

        //                if ( node . FindCyclicDependency ( out oneChain ) )
        //                {
        //                    this . isLeaf = true;
        //                }

        //                if ( oneChain != null && oneChain . Count > 0 )
        //                {
        //                    dependencyChains . Add ( oneChain );
        //                }

        //                result . Add ( node );
        //            }

        //            for ( var i = 0; i < result . Count; i++ )
        //            {
        //                if ( ( paramObjects [ i ] != null ) && ( paramObjects [ i ] is SimpleParameter ) )
        //                {
        //                    var item = ( SimpleParameter ) paramObjects [ i ];

        //                    result [ i ] . nestedObject = _config . GetParamValue ( item );
        //                }
        //            }
        //        }
        //        this . _children = result;

        //        return result;
        //    }

        //    return result;
        //}


        public List<ParamNode> DefineChildren ( )
        {
            List<DependencyCircuit> chains = new List<DependencyCircuit> ( );
            var childTypes = _nestedObj . GetCtorParamTypes ( );
            var result = _nodeType . DefineChildren ( this , childTypes );
            return result;
        }


        public void ChangeState ( NodeKind kind )
        {
            switch ( kind ) 
            {
                case    NodeKind . Fork :
                {
                    _nodeType = new Fork ( );
                    break;
                }

                case NodeKind . TopOfLowestInBunch :
                {
                    _nodeType = new TopOfLowestInBunch ( );
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


        //public void InitializePreliminarily ()
        //{
        //    throw new NotImplementedException ( "InitializePreliminarily" );
        //}

    }


}