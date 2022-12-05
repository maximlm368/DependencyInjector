using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;

namespace DependencyInjector
{
    //public static class LinkedListOfParamsExtention
    //{
    //    public static LinkedList<ParamNode> Clone ( this LinkedList<ParamNode> list )
    //    {
    //        var reciever = new ParamNode [ list . Count ];

    //        list . CopyTo ( reciever , 0 );

    //        var nodes = reciever . ToList ( );

    //        return new LinkedList<ParamNode> ( nodes );
    //    }

    //}

    public class ParamNode
    {
        //private static int idCounter;

        //private int id;

        private int _ordinalNumberInParentCtor { get; set; }

        private DependencyInjection _config;

        private List<ParamNode> _children { get; set; }

        public ParamNode _parent { get; private set; }

        public NodeType _nodeType { get; private set; }

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

            //this . id = idCounter;
            //idCounter++;
            // this . DefineNodeType ( paramType );

            _nodeType = nodeType;
            _isInitiolized = false;
        }


        private DependencyChain CreateChain ( bool cycleFound , List<ParamNode> possibleChain )
        {
            DependencyChain result;
            possibleChain . Reverse ( );

            if ( cycleFound )
            {
                _nodeType = new DependencyCycleParticipant ( );
                possibleChain . AccomplishForEach<ParamNode>
                (
                   ( item ) => { if ( item . _nodeType . _kind  !=  NodeKind . DependencyCycleParticipant ) { item . _nodeType = new DependencyCycleParticipant ( ); } }
                );

                result = new DependencyChain ( possibleChain );
            }
            else
            {
                result = DependencyChain . CreateEmptyChain ( );
            }
            return result;
        }


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
        public DependencyChain GetDependencyChainFromAncestors ( )
        {
            var cycleFound = false;
            var possibleChain = new List<ParamNode> ( );
            possibleChain . Add ( this );
            ParamNode probableCoincidentAncestor = _parent;

            while ( true )
            {
                if ( probableCoincidentAncestor == null )
                {
                    break;
                }

                possibleChain . Add ( probableCoincidentAncestor );

                if ( !probableCoincidentAncestor . Equals ( this ) )
                {
                    probableCoincidentAncestor = probableCoincidentAncestor . _parent;
                    continue;
                }
                else
                {
                    cycleFound = true;
                    break;
                }
            }
            return CreateChain ( cycleFound , possibleChain );
        }


        public override bool Equals ( object obj )
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
            List<DependencyChain> chains = new List<DependencyChain> ( );
            var childTypes = _nestedObj . GetCtorParamTypes ( );
            var result = _nodeType . DefineChildren ( this , childTypes );
            return result;
        }


        public void ChangeState ( NodeKind kind )
        {
            _nodeType = new Fork (" );
        }


        public void InitializePreliminarily ()
        {
            throw new NotImplementedException ( "InitializePreliminarily" );
        }

    }


}