using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;

namespace DependencyInjector
{
    public class DependencyTree
    {
        private ParamNode _root;

        private List<ParamNode> _nodes;

        private List<DependencyCircuit> _circuits;

        private List<Bunch> _bunches;

        private List<LinkedBunches> _linkedBunches;

        private int _deepestLevel;

        private Dictionary<ParamNode , List<DependencyCircuit>> _nodeToCircuits;

        private Dictionary<List<DependencyCircuit> , Bunch> _circuitsToBunch;

        private string _mainExpMessage = "dll is wrong";


        public DependencyTree ( Type rootType )
        {
            _root = new ParamNode ( rootType , new OrdinaryNode ( ) );
            _nodes = new List<ParamNode> ( );
            _nodes . Add ( _root );
            _bunches = new List<Bunch> ( );
            _nodeToCircuits = new Dictionary<ParamNode , List<DependencyCircuit>> ( );
            _circuitsToBunch = new Dictionary<List<DependencyCircuit> , Bunch> ( new CircuitListComparer ( ) );
        }


        public object BuildRootObject ( )
        {
            BuildYourself ( );
            InitializeNodes ( );

            if ( _root . _nestedObj . _isInitialized )
            {
                return _root;
            }

            var bunchSetBuilder = new BunchSetBuilder ( _circuits , _nodeToCircuits , _circuitsToBunch );
            _bunches = bunchSetBuilder . Build ( );
            SetLinkedBunches ( );
            var relationArranger = new RelationsArranger ( _circuits , _nodeToCircuits , _circuitsToBunch , _linkedBunches );
            var generation = relationArranger . ArrangeRelations ( );

            while ( true ) 
            {
                generation = ResolveGenerationOfRelatives ( generation );

                if( generation.Count < 1 )
                {
                    break;
                }
            }

            if ( _root != null )
            {
                return _root;
            }
            else
            {
                throw new Exception ( _mainExpMessage );
            }
        }


        private List<CompoundRelative> ResolveGenerationOfRelatives ( List <CompoundRelative> generation )
        {
            var nextLevelOfRelatives = new List<CompoundRelative> ( );

            for ( var i = 0; i > generation . Count; i++ )
            {
                generation [ i ] . Resolve ( );
                generation [ i ] . ResolveWayToClosestAncestorOrRoot ( );
                var ancestor = generation [ i ] . GetClosestAncestor ( );

                if( ancestor != null )
                {
                    nextLevelOfRelatives . Add ( ancestor );
                }
            }

            return nextLevelOfRelatives;
        }


        private void BuildYourself ( )
        {
            var currentLevel = new List<ParamNode> ( );
            currentLevel . Add ( _root );
            int levelCounter = 0;

            while ( true )
            {
                var childLevel = new List<ParamNode> ( );
                levelCounter++;

                for ( var i = 0;   i < currentLevel . Count;   i++ )
                {
                    var beingProcessedNode = currentLevel [ i ];
                    var children = beingProcessedNode . DefineChildren ( );
                    GatherExistingCircuits ( children );
                    childLevel . AddRange ( children );
                    _nodes . AddRange ( children );
                }

                if ( childLevel . Count < 1 )
                {
                    break;
                }

                currentLevel = childLevel;
                _deepestLevel = levelCounter;
            }
        }


        private void GatherExistingCircuits ( List<ParamNode> nodes )
        {
            for ( var i = 0;    i < nodes . Count;    i++ )
            {
                List<ParamNode> nodeSequence = nodes [ i ] . GetSequenceForDependencyCircuit ( );
                DependencyCircuit circuit = CreateCircuit ( nodeSequence );

                if ( circuit != null )
                {
                     _circuits . Add ( circuit );
                }
            }
        }

        
        private DependencyCircuit CreateCircuit ( List <ParamNode> possibleCircuit )
        {
            DependencyCircuit result = null;

            // incoming 'possibleCircuit' has counting from descendant to ancestor in dependency tree
            // but 'DependencyCircuit' has opposite counting
            // so that reversing is needed

            possibleCircuit . Reverse ( );

            if ( possibleCircuit . Count  >  2 )
            {
                var first = possibleCircuit . First ( );
                var last = possibleCircuit . Last ( );
                first . ChangeState ( NodeKind . TopOfCircuit );
                last . ChangeState ( NodeKind . BottomOfCircuit );

                var border = new List<ParamNode> ( );
                border . Add ( first );
                border . Add ( last );

                possibleCircuit . AccomplishForEachExceptSome<ParamNode>
                (
                   ( item ) => { if ( item . GetNodeKind ( )  !=  NodeKind . DependencyCycleParticipant ) { item . ChangeState ( NodeKind . DependencyCycleParticipant ); } }
                   ,border
                );

                result = new DependencyCircuit ( possibleCircuit );
            }

            return result;
        }


        private void SetLinkedBunches ( )
        {
            if( _bunches . Count < 1 )
            {
                return;
            }

            for ( var i = 0;    i < _bunches . Count - 1;    i++ )
            {
                var beingProcessed = _bunches [ i ];

                if ( ! beingProcessed . _linked )
                {
                    var linked = new List<Bunch> ( );
                    var concerneds = new List<int> ( );
                    concerneds . Add ( i );
                    var circuits = beingProcessed . GetBunchedCircuits ( );
                    var circuitIds = circuits . GetListOfItemPiece ( ( circuit ) => { return circuit . _id; } );
                    var mask = circuitIds . PrintMaskOfPresence ( );
                    SearchLinkedBunches ( mask , i+1 , linked , concerneds );

                    if ( linked . Count > 0 )
                    {
                        beingProcessed . _linked = true;
                        linked . Add ( beingProcessed );
                        _linkedBunches . Add ( new LinkedBunches( linked ) );
                    }
                }
            }
        }


        private void SearchLinkedBunches ( IList<bool> mask,  int scratch,  List<Bunch> linked,  List<int> concerneds )
        {
            for ( var j = scratch;    j < _bunches . Count - 1;    j++ )
            {
                var mustBeIgnored = concerneds . Contains ( j );

                if ( ! _bunches [ j ] . _linked      &&      ! mustBeIgnored )
                {
                    var circuits = _bunches [ j ] . GetBunchedCircuits ( );
                    var circuitIds = circuits . GetListOfItemPiece ( ( circuit ) => { return circuit . _id; } );
                    var intersection = circuitIds . GetIntersectionWhithMask ( mask );

                    if ( intersection . Count > 0 )
                    {
                        _bunches [ j ] . _linked = true;
                        linked . Add ( _bunches [ j ] );
                        concerneds . Add ( j );
                        var currentMask = circuitIds . PrintMaskOfPresence ( );
                        SearchLinkedBunches ( currentMask , 0 , linked , concerneds );
                    }
                }
            }
        }


        private void InitializeNodes ( )
        {
            for ( var i = _nodes . Count - 1;    i >= 0;    i-- )
            {
                try
                {
                    _nodes [ i ] . InitializeNestedObject ( );
                }
                catch ( NotInitializedChildException )
                {
                    continue;
                }
            }
        }

    }
}